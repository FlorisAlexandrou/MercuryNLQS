using Speech2TextPrototype.Data;
using Speech2TextPrototype.Models;
using System.Collections.Generic;

namespace Speech2TextPrototype.Services
{
    public interface ILookupTableService
    {
        public LookupOutputModel Token2Sql(PyRes res);

        public string GroupByFilters(string query, string groupByFilter, string uuid);

        public string HandleErrors(LookupOutputModel lookupOutput);

        public List<string> GetSpeechRecognitionCustomWords();

    }
}
