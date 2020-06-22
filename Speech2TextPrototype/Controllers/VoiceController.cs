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
    public class VoiceController : ControllerBase
    {
        private static readonly AzureKeyCredential credentials = new AzureKeyCredential("a80f6dfc55004cc9a3b49af55826c3c2");
        private static readonly Uri endpoint = new Uri("https://floris-textanalytics.cognitiveservices.azure.com/");
        private static readonly SpeechConfig speechConfig = SpeechConfig.FromSubscription(
                    "7e02a98e81db4d2ebcd09ec25472af3d",
                    "eastus");
        private LookupValuesController _lvc;

        public VoiceController(Controllers.LookupValuesController lvc)
        {
            _lvc = lvc;
            Console.WriteLine(_lvc);
        }

        // GET: api/Voice
        [HttpGet]
        public async Task<ActionResult> GetAsync()
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


        [HttpGet]
        [Route("token/{sentence}")]
        public async Task<IActionResult> Test(string sentence)
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
                    qna = GetQnA(sentence, false);
                return Ok(new { queryResult, listMeasures, qna, query });
            }
        }

        [HttpGet]
        [Route("qna/{question}")]
        public QnASearchResultList GetQnA(string question, bool voice)
        {
            var endpointhostName = "https://query-assistant.azurewebsites.net";
            var endpointKey = "4c627627-04b6-4439-9969-87e92e45fe64";
            string kbId = "17d474de-7b95-4035-bfe0-c78fee641eaf";
            var runtimeClient = new QnAMakerRuntimeClient(new EndpointKeyServiceClientCredentials(endpointKey)) { RuntimeEndpoint = endpointhostName };
            var response = runtimeClient.Runtime.GenerateAnswerAsync(kbId, new QueryDTO { Question = question }).Result;
            if (voice)
            {
                _ = textToSpeechAsync(response.Answers[0].Answer);
            }
            return response;
        }

        [HttpGet]
        [Route("text2speech/{text}")]
        public async Task textToSpeechAsync(string text)
        {
            using var synthesizer = new SpeechSynthesizer(speechConfig);
            await synthesizer.SpeakTextAsync(text);
        }
    }
}
