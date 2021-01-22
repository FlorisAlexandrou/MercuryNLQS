using Speech2TextPrototype.Data;
using System.Collections.Generic;

namespace Speech2TextPrototype.Repositories
{
    public interface IDisplayTableRepository
    {
        public List<DisplayTable> GetTableData(string uuid);
        public List<DisplayTable> GetChartData(string uuid);
        public void SaveData(List<DisplayTable> tableData);
        public void DeleteData(string uuid);
    }
}
