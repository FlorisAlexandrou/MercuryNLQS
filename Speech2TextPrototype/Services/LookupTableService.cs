using Consumer_Retail_Research_Analytics_NLP.Models;
using Speech2TextPrototype.Data;
using Speech2TextPrototype.Models;
using Speech2TextPrototype.Repositories;
using System.Collections.Generic;
using System.Linq;

namespace Speech2TextPrototype.Services
{
    public class LookupTableService : ILookupTableService
    {
        private readonly ILookupTableRepository _lookupTableRepository = null;
        public LookupTableService(ILookupTableRepository lookupTableRepository)
        {
            _lookupTableRepository = lookupTableRepository;
        }

        public SqlAnswer Token2Sql(PyRes res)
        {
            var answer = _lookupTableRepository.token2Sql(res);
            answer.error = HandleErrors(answer);
            return answer;
        }

        public string GroupByFilters(string query, string groupByFilter, string uuid)
        {
            return _lookupTableRepository.GroupByFilters(query, groupByFilter, uuid);
        }


        /// <summary>
        /// Sends error codes to qnamaker and then the qnamaker sends helpful messages to the user
        /// </summary>
        /// <param name="queryResult">The data returned from the TData table</param>
        /// <param name="listMeasures">A list of measures to display (sales)</param>
        /// <returns>Custom error code string for the qnamaker</returns>
        private string HandleErrors(SqlAnswer sqlAnswer)
        {
            int listMeasuresLen = sqlAnswer.measures.Count();
            double scalar = sqlAnswer.scalar;

            if (listMeasuresLen == 0)
            {
                return "ERROR:No List Measures";
            }
            return string.Empty;
        }

        public List<string> GetSpeechRecognitionCustomWords()
        {
            return _lookupTableRepository.GetSpeechRecognitionCustomWords();
        }

    }
}
