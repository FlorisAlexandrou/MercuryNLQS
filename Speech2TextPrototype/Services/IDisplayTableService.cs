using Speech2TextPrototype.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Speech2TextPrototype.Services
{
    public interface IDisplayTableService
    {
        public List<DisplayTable> GetTableData();
        public List<DisplayTable> GetChartData();
    }
}
