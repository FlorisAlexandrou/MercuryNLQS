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
        private readonly IDisplayTableService _displayTableService;

        private static readonly SpeechConfig speechConfig = SpeechConfig.FromSubscription(
                    "38df3e4febac4df48490c9f3d8eaa23f",
                    "eastus");

        public UserInputController(ILookupValuesService lookupValuesService, IDisplayTableService displayTableService)
        {
            _lookupValuesService = lookupValuesService;
            _displayTableService = displayTableService;
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
        /// <param name="voiceOutput">Indicates whether the answer should be sent as speech</param>
        /// <returns>Json object that either contains bot answer or DB query answer</returns>
        [HttpGet]
        [Route("token/{sentence}")]
        public async Task<IActionResult> HandleUserQuery(string sentence, bool voiceOutput)
        {
            string url = "https://tokens-api.herokuapp.com/tokenize/";
            int responseThershold = 3000;
            QnASearchResultList qna = new QnASearchResultList();
            LookupOutputModel lookupOutput = new LookupOutputModel();
            List<DisplayTable> queryResult = new List<DisplayTable>();
            List<string> listMeasures = new List<string>();
            List<string> listDates = new List<string>();
            string query = "";
            string error = "";
            double scalar = -1;

            using (HttpClient client = new HttpClient())
            {
                // Tokenize Sentence
                var httpResponse = await client.GetStringAsync(url + sentence);
                PyRes res = JsonConvert.DeserializeObject<PyRes>(httpResponse);
                // Return either a table or a bot answer
                if (res.isSqlQuery)
                {
                    lookupOutput = _lookupValuesService.token2Sql(res);

                    queryResult = lookupOutput.data;
                    listMeasures = lookupOutput.measures;
                    query = lookupOutput.querySql;
                    listDates = lookupOutput.dates;
                    scalar = lookupOutput.scalarValue;

                    // Error Handling
                    error = _lookupValuesService.HandleErrors(queryResult.Count(), listMeasures.Count(), listDates.Count(), scalar);

                    if (!String.IsNullOrEmpty(error))
                    {
                        qna = GetQnA(error, voiceOutput);
                    }

                    else if (queryResult.Any() && queryResult.Count() <= responseThershold)
                    {
                        qna = GetQnA("Output Type", voiceOutput);
                    }

                }
                else
                    qna = GetQnA(sentence, voiceOutput);

                if (qna.Answers != null)
                {
                    string botAnswer = qna.Answers[0].Answer;
                    if (botAnswer == "Table")
                    {
                        queryResult = _displayTableService.GetTableData();
                    }

                    else if (botAnswer == "What type of chart?")
                    {
                        queryResult = _displayTableService.GetChartData();
                    }

                    else if (botAnswer == "M_SALES_VALUE" || botAnswer == "M_SALES_VOLUME" || botAnswer == "M_SALES_ITEMS")
                    {
                        listMeasures.Add(botAnswer);
                        queryResult = _displayTableService.GetTableData();
                        if (queryResult.Count() <= responseThershold)
                            qna = GetQnA("Output Type", voiceOutput);
                    }
                    else
                        queryResult = null;
                }
                return Ok(new { queryResult, listMeasures, qna, query, scalar });

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
        /// Server-side Pagination
        /// </summary>
        /// <param name="pageIndex">The index of the datatable page we are on</param>
        /// <param name="pageSize">The number of rows per page</param>
        /// <returns>Paged data back to the datatable</returns>
        [HttpGet]
        [Route("table/page")]
        public List<DisplayTable> getPagedTable(int pageIndex, int pageSize)
        {
            return _displayTableService.GetTablePaged(pageIndex, pageSize);
        }

        [HttpGet]
        [Route("table/sort")]
        public List<DisplayTable> getSortedTable(string column, string sortOrder, int pageIndex, int pageSize)
        {
            return _displayTableService.GetTableSorted(column, sortOrder, pageIndex, pageSize);
        }
    }
}
