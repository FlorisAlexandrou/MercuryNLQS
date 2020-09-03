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
        private readonly ILookupValuesRepository _lookup = null;
        public LookupValuesService(ILookupValuesRepository lookup)
        {
            _lookup = lookup;
        }
        public LookupOutputModel token2Sql(PyRes res)
        {

            return _lookup.token2Sql(res);
        }

        /// <summary>
        /// Sends error codes to qnamaker and then the qnamaker sends helpful messages to the user
        /// </summary>
        /// <param name="queryResult">The data returned from the TData table</param>
        /// <param name="listMeasures">A list of measures to display (sales)</param>
        /// <returns>Custom error code string for the qnamaker</returns>
        public string HandleErrors(int queryResultLen, int listMeasuresLen, int listDatesLen)
        {
            if (queryResultLen == 0)
            {
                return "ERROR:No Query Result";
            }
            else if (listMeasuresLen == 0)
            {
                return "ERROR:No List Measures";
            }
            return string.Empty;
        }
    }
}
