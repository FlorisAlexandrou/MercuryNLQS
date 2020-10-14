using Speech2TextPrototype.Data;
using Speech2TextPrototype.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Speech2TextPrototype.Repositories
{
    public interface ILookupTableRepository
    {
        public LookupOutputModel token2Sql(PyRes res);

        public List<DisplayTable> GroupByFilters(string query, string groupByFilter, string uuid);

    }
}
