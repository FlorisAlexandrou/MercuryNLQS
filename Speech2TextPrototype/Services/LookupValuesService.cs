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
        public LookupOutputModel token2Sql(PyRes res)
        {

            return _lookupValuesRepository.token2Sql(res);
        }

        /// <summary>
        /// Sends error codes to qnamaker and then the qnamaker sends helpful messages to the user
        /// </summary>
        /// <param name="queryResult">The data returned from the TData table</param>
        /// <param name="listMeasures">A list of measures to display (sales)</param>
        /// <returns>Custom error code string for the qnamaker</returns>
        public string HandleErrors(int queryResultLen, int listMeasuresLen, int listDatesLen)
        {
            if (listMeasuresLen == 0)
            {
                return "ERROR:No List Measures";
            }
            else if (listDatesLen == 0)
            {
                return "WARNING:No List Dates";
            }
            else if (queryResultLen == 0)
            {
                return "ERROR:No Query Result";
            }
            return string.Empty;
        }
    }
}
