using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Azure;
using System.Globalization;
using Microsoft.CognitiveServices.Speech;
using Azure.AI.TextAnalytics;
using Speech2TextPrototype.Models;
using Microsoft.Azure.CognitiveServices.Knowledge.QnAMaker;
using Microsoft.Azure.CognitiveServices.Knowledge.QnAMaker.Models;
using Speech2TextPrototype.Data;
using Newtonsoft.Json;
using Speech2TextPrototype.Services;

namespace Speech2TextPrototype.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class UserInputController : ControllerBase
    {
        private readonly ILookupValuesService _lookupValuesService;

        private static readonly SpeechConfig speechConfig = SpeechConfig.FromSubscription(
                    "38df3e4febac4df48490c9f3d8eaa23f",
                    "eastus");

        public UserInputController(ILookupValuesService lookupValuesService)
        {
            _lookupValuesService = lookupValuesService;
        }


        /// <summary>
        /// Recognize speech from microphone
        /// </summary>
        /// <returns>A string of recognized speech</returns>
        // GET: api/UserInput
        [HttpGet]
        public async Task<ActionResult> RecognizeSpeech()
        {
            using var recognizer = new SpeechRecognizer(speechConfig);
            string text = "";
            var result = await recognizer.RecognizeOnceAsync();
            switch (result.Reason)
            {
                case ResultReason.RecognizedSpeech:
                    text = result.Text;
                    break;
                case ResultReason.NoMatch:
                    text = "Speech could not be recognized.";
                    break;
                case ResultReason.Canceled:
                    var cancellation = CancellationDetails.FromResult(result);
                    text = "Cancelled: Reason= " + cancellation.Reason;
                    if (cancellation.Reason == CancellationReason.Error)
                    {
                        text = "Canceled: " + cancellation.ErrorCode;
                    }
                    break;
            }
            return Ok(text);
        }


        /// <summary>
        /// Get user's query, send to python tokenizer and decide what to do with it
        /// </summary>
        /// <param name="sentence">The user's question/query</param>
        /// <returns>Json object that either contains bot answer or DB query answer</returns>
        [HttpGet]
        [Route("token/{sentence}")]
        public async Task<IActionResult> HandleUserQuery(string sentence, bool voiceOutput)
        {
            string url = "https://tokens-api.herokuapp.com/tokenize/";
            int responseThershold = 3000;
            QnASearchResultList qna = new QnASearchResultList();
            LookupOutputModel lookupOutput = new LookupOutputModel();
            List<TData> queryResult = new List<TData>();
            List<string> listMeasures = new List<string>();
            List<string> listDates = new List<string>();
            string query = "";

            using (HttpClient client = new HttpClient())
            {
                // Tokenize Sentence
                var httpResponse = await client.GetStringAsync(url + sentence);
                PyRes res = JsonConvert.DeserializeObject<PyRes>(httpResponse);
                // Return either a table or a bot answer
                if (res.isSqlQuery)
                {
                    lookupOutput = _lookupValuesService.token2Sql(res);
                    //(queryResult, listMeasures, query) = _lookupValuesService.token2Sql(res);

                    if (lookupOutput.data.Any() && lookupOutput.data.Count() <= responseThershold)
                    {
                        qna = GetQnA("Output Type", voiceOutput);
                    }

                    //TODO: Error Handling here
                    // Idea: Whenever an error occurs, throw exception and pass it to qnamaker to get the corresponding answer

                    // If listMeasures.Count() < 0 then throw exception: did not understand the parameters
                    // try for sales, sales items, sales volume

                    // Else If string.IsNullOrEmpty(queryResult) then throw exception: Could not bring results to your query
                    // try a simpler query with fewer/different filters

                    //string error = _lookupValuesService.HandleErrors(queryResult.Count(), listMeasures.Count(), 1);
                    //string error2 = HandleErrors(queryResult, listMeasures);
                    //if (!string.IsNullOrEmpty(error))
                    //    qna = GetQnA(error, voiceOutput);

                    queryResult = lookupOutput.data;
                    listMeasures = lookupOutput.measures;
                    query = lookupOutput.querySql;
                }
                else
                    qna = GetQnA(sentence, voiceOutput);

                return Ok(new { queryResult, listMeasures, qna, query });
                //return Ok(new { lookupOutput.data, lookupOutput.measures, qna, lookupOutput.querySql });

            }
        }

        /// <summary>
        /// Ask a question and get an answer from the qna maker bot
        /// </summary>
        /// <param name="question">The user's question</param>
        /// <param name="voiceOutput">If enabled, then the bot's answer will be also turned to voice</param>
        /// <returns>Bot's answer and asynchronous voice output</returns>
        [HttpGet]
        [Route("qna/{question}")]
        public QnASearchResultList GetQnA(string question, bool voiceOutput)
        {
            var endpointhostName = "https://floris-qnaservice.azurewebsites.net";
            var endpointKey = "f3536082-ee9c-408f-9948-1fb77b30c0c6";
            string kbId = "f158eca8-f7f3-4c88-a7d7-b7724956df4c";
            var runtimeClient = new QnAMakerRuntimeClient(new EndpointKeyServiceClientCredentials(endpointKey)) { RuntimeEndpoint = endpointhostName };
            var response = runtimeClient.Runtime.GenerateAnswerAsync(kbId, new QueryDTO { Question = question }).Result;
            if (voiceOutput)
            {
                _ = TextToSpeechAsync(response.Answers[0].Answer);
            }
            return response;
        }

        /// <summary>
        /// Turns text to speech
        /// </summary>
        /// <param name="text">The text to be returned as speech</param>
        /// <returns>Speech/Sound output of the given text</returns>
        [HttpGet]
        [Route("text2speech/{text}")]
        public async Task TextToSpeechAsync(string text)
        {
            using var synthesizer = new SpeechSynthesizer(speechConfig);
            await synthesizer.SpeakTextAsync(text);
        }

        /// <summary>
        /// Sends error codes to qnamaker and then the qnamaker sends helpful messages to the user
        /// </summary>
        /// <param name="queryResult">The data returned from the TData table</param>
        /// <param name="listMeasures">A list of measures to display (sales)</param>
        /// <returns>Custom error code string for the qnamaker</returns>
        private string HandleErrors(List<TData> queryResult, List<string> listMeasures)
        {
            if (queryResult.Count() == 0)
            {
                return "ERROR:No Query Result";
            }
            else if (listMeasures.Count() == 0)
            {
                return "ERROR:No List Measures";
            }
            return string.Empty;
        }
    }
}
