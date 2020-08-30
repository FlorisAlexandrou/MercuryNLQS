using Speech2TextPrototype.Data;
using Speech2TextPrototype.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Speech2TextPrototype.Repositories
{
    public interface ILookupValuesRepository
    {
        public (List<TData>, List<string>, string) token2Sql(PyRes res);
    }
}
