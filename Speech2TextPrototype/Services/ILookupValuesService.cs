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
        public LookupOutputModel Token2Sql(PyRes res);

        public List<DisplayTable> GroupByFilters(string query, string groupByFilter);

        public string HandleErrors(LookupOutputModel lookupOutput);
    }
}
