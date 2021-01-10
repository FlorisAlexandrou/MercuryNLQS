using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CognitiveServices.Speech;
using Speech2TextPrototype.Models;
using Microsoft.Azure.CognitiveServices.Knowledge.QnAMaker;
using Microsoft.Azure.CognitiveServices.Knowledge.QnAMaker.Models;
using Speech2TextPrototype.Data;
using Newtonsoft.Json;
using Speech2TextPrototype.Services;
using System.Text.RegularExpressions;

namespace Speech2TextPrototype.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class UserInputController : ControllerBase
    {
        private readonly ILookupTableService _lookupTableService;
        private readonly IDisplayTableService _displayTableService;

        private static readonly SpeechConfig speechConfig = SpeechConfig.FromSubscription(
                    "38df3e4febac4df48490c9f3d8eaa23f",
                    "eastus");

        public UserInputController(ILookupTableService lookupTableService, IDisplayTableService displayTableService)
        {
            _lookupTableService = lookupTableService;
            _displayTableService = displayTableService;
        }


        /// <summary>
        /// Recognize speech from microphone
        /// </summary>
        /// <returns>A string of recognized speech</returns>
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
        /// <param name="sqlQuery">Generated query which is passed back to the backend for execution</param>
        /// <param name="uuid">User ID to enable concurrent usage of the displayTable</param>
        /// <returns>Json object that either contains bot answer or DB query answer</returns>
        [HttpGet]
        [Route("token/{sentence}")]
        public async Task<IActionResult> HandleUserQuery(string sentence, string uuid, string sqlQuery, bool voiceOutput)
        {
            string pythonApiURL = "https://tokens-api.herokuapp.com/tokenize/";
            /// The maximum rows of data that are allowed to be returned to the frontend
            int responseThershold = 3000;
            /// QnA Maker object which contains the answer
            QnASearchResultList qna = new QnASearchResultList();
            /// The data object returned to the frontend
            List<DisplayTable> queryResult = new List<DisplayTable>();
            /// The list of measures that the database supports
            List<string> listMeasures = new List<string>();
            /// Value returned by algebraic and distributive measures (sum, avg, etc)
            double scalar = -1;

            using HttpClient client = new HttpClient();
            // Tokenize Sentence
            var httpResponse = await client.GetStringAsync(pythonApiURL + sentence);
            PyRes res = JsonConvert.DeserializeObject<PyRes>(httpResponse);

            if (res.isSqlQuery)
            {
                LookupOutputModel lookupOutput = _lookupTableService.Token2Sql(res);
                sqlQuery = lookupOutput.querySql;
                listMeasures = lookupOutput.measures;
                scalar = lookupOutput.scalarValue;
                if (scalar != -1)
                    return Ok(new { queryResult, listMeasures, qna, sqlQuery, scalar });

                // Error Handling
                string error = _lookupTableService.HandleErrors(lookupOutput);

                if (!String.IsNullOrEmpty(error))
                    qna = GetQnA(error, voiceOutput);
                else
                    qna = GetQnA("Granularity Type", voiceOutput);
            }
            else
                qna = GetQnA(sentence, voiceOutput);

            if (qna.Answers != null)
            {
                string botAnswer = qna.Answers[0].Answer;
                if (botAnswer == "Table")
                {
                    queryResult = _displayTableService.GetTableData(uuid);
                }

                else if (botAnswer == "What type of chart?")
                {
                    queryResult = _displayTableService.GetChartData(uuid);
                }

                else if (botAnswer == "M_SALES_VALUE" || botAnswer == "M_SALES_VOLUME" || botAnswer == "M_SALES_ITEMS")
                {
                    listMeasures.Add(botAnswer);
                    qna = GetQnA("Granularity Type", voiceOutput);
                }

                else if (botAnswer == "PRODUCT_NAME" || botAnswer == "CATEGORY_NAME" || botAnswer == "BRAND")
                {
                    queryResult = _lookupTableService.GroupByFilters(sqlQuery, botAnswer, uuid);
                    if (queryResult.Count() == 0)
                        qna = GetQnA("ERROR:No Query Result", voiceOutput);
                    else if (queryResult.Count() <= responseThershold)
                        qna = GetQnA("Output Type", voiceOutput);
                }
                else
                    queryResult = null;
            }
            return Ok(new { queryResult, listMeasures, qna, sqlQuery, scalar });
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
            using var runtimeClient = new QnAMakerRuntimeClient(new EndpointKeyServiceClientCredentials(endpointKey)) { RuntimeEndpoint = endpointhostName };
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
            string[] keywords = new string[] { "M_SALES_VALUE", "M_SALES_VOLUME", "M_SALES_ITEMS",
                "PRODUCT_NAME", "CATEGORY_NAME", "BRAND" };

            // Do not speak keyword text
            if (keywords.Contains(text)) return;

            // Speak specific text according to the frontend
            if (text == "Table")
                text = "Please find the table below";
            else if (text.Contains("Chart"))
                text = $"Your {text} is on the way";

            // Delete # from markdown
            text = Regex.Replace(text, "#", "");
            using var synthesizer = new SpeechSynthesizer(speechConfig);
            await synthesizer.SpeakTextAsync(text);
        }

        /// <summary>
        /// Server-side Pagination
        /// </summary>
        /// <param name="pageIndex">The index of the datatable page we are on</param>
        /// <param name="pageSize">The number of rows per page</param>
        /// <param name="uuid">User ID to enable concurrent usage of the displayTable</param>
        /// <returns>Paged data back to the datatable</returns>
        [HttpGet]
        [Route("table/page")]
        public List<DisplayTable> GetTablePaged(int pageIndex, int pageSize, string uuid)
        {
            return _displayTableService.GetTablePaged(pageIndex, pageSize, uuid);
        }

        [HttpGet]
        [Route("table/sort")]
        public List<DisplayTable> GetTableSorted(string column, string sortOrder, int pageIndex, int pageSize, string uuid)
        {
            return _displayTableService.GetTableSorted(column, sortOrder, pageIndex, pageSize, uuid);
        }

        [HttpPost]
        [Route("table/save")]
        public void SaveTableData([FromBody] List<DisplayTable> tableData)
        {
            _displayTableService.SaveData(tableData);
        }

        [HttpGet]
        [Route("table/delete")]
        public void DeleteTable(string uuid)
        {
            _displayTableService.DeleteData(uuid);
        }
    }
}
