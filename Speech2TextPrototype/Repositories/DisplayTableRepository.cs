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

        public List<DisplayTable> GetTableData()
        {
            return _context.displayTable.ToList();
        }

        public List<DisplayTable> GetChartData()
        {
            var chartData = _context.displayTable.Select(r => new DisplayTable() 
            {
                M_SALES_VALUE = r.M_SALES_VALUE,
                M_SALES_ITEMS = r.M_SALES_ITEMS,
                M_SALES_VOLUME = r.M_SALES_VOLUME,
                PERIOD_START = r.PERIOD_START
            }).ToList();

            return chartData;
        }

    }
}
