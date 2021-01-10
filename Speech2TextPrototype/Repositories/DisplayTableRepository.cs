using Speech2TextPrototype.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Speech2TextPrototype.Repositories
{
    public class DisplayTableRepository : IDisplayTableRepository
    {
        private readonly florisContext _context;

        public DisplayTableRepository(florisContext context)
        {
            _context = context;
        }

        public List<DisplayTable> GetTableData(string uuid)
        {
            return _context.displayTable.Where(r => r.UUID == uuid).ToList();
        }

        public List<DisplayTable> GetChartData(string uuid)
        {
            var chartData = _context.displayTable.Where(r => r.UUID == uuid).Select(r => new DisplayTable() 
            {
                M_SALES_VALUE = r.M_SALES_VALUE,
                M_SALES_ITEMS = r.M_SALES_ITEMS,
                M_SALES_VOLUME = r.M_SALES_VOLUME,
                PERIOD_START = r.PERIOD_START
            }).ToList();

            return chartData;
        }

        public void SaveData(List<DisplayTable> tableData)
        {
            _context.displayTable.AddRange(tableData);
            _context.SaveChanges();
        }

        public void DeleteData(string uuid)
        {
            _context.displayTable.RemoveRange(GetTableData(uuid));
            _context.SaveChanges();
        }

    }
}
