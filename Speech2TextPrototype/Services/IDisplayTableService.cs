using Speech2TextPrototype.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Speech2TextPrototype.Services
{
    public interface IDisplayTableService
    {
        public List<DisplayTable> GetTableData(string uuid);
        public List<DisplayTable> GetChartData(string uuid);
        public List<DisplayTable> GetTablePaged(int pageIndex, int pageSize, string uuid);
        public List<DisplayTable> GetTableSorted(string column, string sortOrder, int pageIndex, int pageSize, string uuid);
        public void SaveData(List<DisplayTable> tableData);
        public void DeleteData(string uuid);
    }
}
