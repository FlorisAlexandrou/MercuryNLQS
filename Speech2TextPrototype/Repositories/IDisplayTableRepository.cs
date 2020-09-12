using Speech2TextPrototype.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Speech2TextPrototype.Repositories
{
    public interface IDisplayTableRepository
    {
        public List<DisplayTable> GetTableData();
        public List<DisplayTable> GetChartData();
    }
}
