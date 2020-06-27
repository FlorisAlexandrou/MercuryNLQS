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

namespace Speech2TextPrototype.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class UserInputController : ControllerBase
    {
        private static readonly AzureKeyCredential credentials = new AzureKeyCredential("a80f6dfc55004cc9a3b49af55826c3c2");
        private static readonly Uri endpoint = new Uri("https://floris-textanalytics.cognitiveservices.azure.com/");
        private static readonly SpeechConfig speechConfig = SpeechConfig.FromSubscription(
                    "7e02a98e81db4d2ebcd09ec25472af3d",
                    "eastus");
        private LookupValuesController _lvc;

        public UserInputController(Controllers.LookupValuesController lvc)
        {
            _lvc = lvc;
            Console.WriteLine(_lvc);
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
            string query = "";
            List<TData> queryResult = new List<TData>();
            QnASearchResultList qna = new QnASearchResultList();
            List<string> listMeasures = new List<string>();
            using (HttpClient client = new HttpClient())
            {
                // Tokenize Sentence
                var httpResponse = await client.GetStringAsync(url + sentence);
                PyRes res = JsonConvert.DeserializeObject<PyRes>(httpResponse);
                // Return either a table or a bot answer
                if (res.isSqlQuery)
                   (queryResult, listMeasures, query) = _lvc.token2Sql(res);
                else
                    qna = GetQnA(sentence, voiceOutput);
                if (queryResult.Any() && queryResult.Count() <= 3000)
                {
                    qna = GetQnA("Output Type", voiceOutput);
                }
                return Ok(new { queryResult, listMeasures, qna, query });
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
            var endpointhostName = "https://query-assistant.azurewebsites.net";
            var endpointKey = "4c627627-04b6-4439-9969-87e92e45fe64";
            string kbId = "17d474de-7b95-4035-bfe0-c78fee641eaf";
            var runtimeClient = new QnAMakerRuntimeClient(new EndpointKeyServiceClientCredentials(endpointKey)) { RuntimeEndpoint = endpointhostName };
            var response = runtimeClient.Runtime.GenerateAnswerAsync(kbId, new QueryDTO { Question = question }).Result;
            if (voiceOutput)
            {
                _ = textToSpeechAsync(response.Answers[0].Answer);
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
        public async Task textToSpeechAsync(string text)
        {
            using var synthesizer = new SpeechSynthesizer(speechConfig);
            await synthesizer.SpeakTextAsync(text);
        }
    }
}
