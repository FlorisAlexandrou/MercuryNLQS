using Speech2TextPrototype.Data;
using Speech2TextPrototype.Models;
using System.Collections.Generic;

namespace Speech2TextPrototype.Repositories
{
    public interface ILookupTableRepository
    {
        public LookupOutputModel token2Sql(PyRes res);

        public List<DisplayTable> GroupByFilters(string query, string groupByFilter, string uuid);

    }
}
