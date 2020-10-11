using Speech2TextPrototype.Data;
using Speech2TextPrototype.Models;
using Speech2TextPrototype.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Speech2TextPrototype.Services
{
    public class LookupValuesService : ILookupValuesService
    {
        private readonly ILookupValuesRepository _lookupValuesRepository = null;
        public LookupValuesService(ILookupValuesRepository lookupValuesRepository)
        {
            _lookupValuesRepository = lookupValuesRepository;
        }
        public LookupOutputModel Token2Sql(PyRes res)
        {

            return _lookupValuesRepository.token2Sql(res);
        }

        public List<DisplayTable> GroupByFilters(string query, string groupByFilter)
        {
            return _lookupValuesRepository.GroupByFilters(query, groupByFilter);
        }


        /// <summary>
        /// Sends error codes to qnamaker and then the qnamaker sends helpful messages to the user
        /// </summary>
        /// <param name="queryResult">The data returned from the TData table</param>
        /// <param name="listMeasures">A list of measures to display (sales)</param>
        /// <returns>Custom error code string for the qnamaker</returns>
        public string HandleErrors(LookupOutputModel lookupOutput)
        {
            int listMeasuresLen = lookupOutput.measures.Count();
            int listDatesLen = lookupOutput.dates.Count();
            double scalar = lookupOutput.scalarValue;

            if (listMeasuresLen == 0)
            {
                return "ERROR:No List Measures";
            }
            else if (listDatesLen == 0)
            {
                return "WARNING:No List Dates";
            }
            return string.Empty;
        }
    }
}
