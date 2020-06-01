using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Azure;
using System.Globalization;
using Microsoft.CognitiveServices.Speech;
using Azure.AI.TextAnalytics;
using Speech2TextPrototype.Models;
using Microsoft.Azure.CognitiveServices.Knowledge.QnAMaker;
using Microsoft.Azure.CognitiveServices.Knowledge.QnAMaker.Models;

namespace Speech2TextPrototype.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class VoiceController : ControllerBase
    {
        private static readonly AzureKeyCredential credentials = new AzureKeyCredential("a80f6dfc55004cc9a3b49af55826c3c2");
        private static readonly Uri endpoint = new Uri("https://floris-textanalytics.cognitiveservices.azure.com/");
        // GET: api/Voice
        [HttpGet]
        public async Task<ActionResult<Language>> GetAsync()
        {
            var config =
                SpeechConfig.FromSubscription(
                    "7e02a98e81db4d2ebcd09ec25472af3d",
                    "eastus");

            using var recognizer = new SpeechRecognizer(config);

            string text = "";
            Entity[] Entities = { };

            var result = await recognizer.RecognizeOnceAsync();
            switch (result.Reason)
            {
                case ResultReason.RecognizedSpeech:
                    text = result.Text;
                    var client = new TextAnalyticsClient(endpoint, credentials);
                    Entities = EntityRecognitionExample(client, result.Text);
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

            return Ok(new Language() { text = text, entities = Entities });

            //return test;
        }

        [HttpGet("{question}")]
        public QnASearchResultList GetQnA(string question)
        {
            var endpointhostName = "https://query-assistant.azurewebsites.net";
            var endpointKey = "3db47372-6124-4a27-89fe-0e43465e0d0c";
            string kbId = "9df255bc-67b5-4424-a60a-8f0438c23679";
            var runtimeClient = new QnAMakerRuntimeClient(new EndpointKeyServiceClientCredentials(endpointKey)) { RuntimeEndpoint = endpointhostName };
            var response = runtimeClient.Runtime.GenerateAnswerAsync(kbId, new QueryDTO { Question = question }).Result;
            return response;
        }

        static Entity[] EntityRecognitionExample(TextAnalyticsClient client, String text)
        {
            var response = client.RecognizeEntities(text);
            List<Entity> result = new List<Entity>();
            foreach (var entity in response.Value)
            {
                result.Add(new Entity(entity.Text, entity.Category.ToString(), entity.SubCategory, entity.Text.Length, entity.ConfidenceScore));
            }
            Entity[] resArray = result.ToArray();
            return resArray;
        }

        [HttpGet]
        [Route("text2speech/{text}")]
        public async Task textToSpeechAsync(string text)
        {
            var config =
                SpeechConfig.FromSubscription(
                    "7e02a98e81db4d2ebcd09ec25472af3d",
                    "eastus");
            using var synthesizer = new SpeechSynthesizer(config);
            await synthesizer.SpeakTextAsync(text);
        }
    }
}
