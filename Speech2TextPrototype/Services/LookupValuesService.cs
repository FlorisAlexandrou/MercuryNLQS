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
        public (List<TData>, List<string>, string) token2Sql(PyRes res)
        {
            return _lookup.token2Sql(res);
        }
    }
}
