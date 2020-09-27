using Speech2TextPrototype.Data;
using Speech2TextPrototype.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Speech2TextPrototype.Services
{
    public interface ILookupValuesService
    {
        public LookupOutputModel token2Sql(PyRes res);

        public string HandleErrors(int queryResultLen, int listMeasuresLen, int listDatesLen, double scalar);
    }
}
