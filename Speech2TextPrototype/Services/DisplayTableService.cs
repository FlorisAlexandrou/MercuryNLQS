using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Speech2TextPrototype.Data;
using Speech2TextPrototype.Repositories;

namespace Speech2TextPrototype.Services
{
    public class DisplayTableService : IDisplayTableService
    {
        private readonly IDisplayTableRepository _displayTableRepository = null;
        
        public DisplayTableService(IDisplayTableRepository displayTableRepository)
        {
            _displayTableRepository = displayTableRepository;
        }
        public List<DisplayTable> GetTableData()
        {
            return _displayTableRepository.GetTableData();
        }

        public List<DisplayTable> GetChartData()
        {
            return _displayTableRepository.GetChartData();
        }

        public List<DisplayTable> GetTablePaged(int pageIndex, int pageSize)
        {
            var data = _displayTableRepository.GetTableData();
            var pagedData = data.Skip(pageIndex * pageSize).Take(pageSize).ToList();
            return pagedData;
        }

        public List<DisplayTable> GetTableSorted(string column, string sortOrder, int pageIndex, int pageSize)
        {
            var data = _displayTableRepository.GetTableData();
            if (!string.IsNullOrEmpty(sortOrder))
            {
                switch (column)
                {
                    case "brand":
                        if (sortOrder == "asc")
                            data = data.OrderBy(r => r.BRAND).ToList();
                        else
                            data = data.OrderByDescending(r => r.BRAND).ToList();
                        break;
                    case "categoryName":
                        if (sortOrder == "asc")
                            data = data.OrderBy(r => r.CATEGORY_NAME).ToList();
                        else
                            data = data.OrderByDescending(r => r.CATEGORY_NAME).ToList();
                        break;
                    case "periodStart":
                        if (sortOrder == "asc")
                            data = data.OrderBy(r => r.PERIOD_START).ToList();
                        else
                            data = data.OrderByDescending(r => r.PERIOD_START).ToList();
                        break;
                    case "M_SALES_VALUE":
                        if (sortOrder == "asc")
                            data = data.OrderBy(r => r.M_SALES_VALUE).ToList();
                        else
                            data = data.OrderByDescending(r => r.M_SALES_VALUE).ToList();
                        break;
                    case "M_SALES_ITEMS":
                        if (sortOrder == "asc")
                            data = data.OrderBy(r => r.M_SALES_ITEMS).ToList();
                        else
                            data = data.OrderByDescending(r => r.M_SALES_ITEMS).ToList();
                        break;
                    case "M_SALES_VOLUME":
                        if (sortOrder == "asc")
                            data = data.OrderBy(r => r.M_SALES_VOLUME).ToList();
                        else
                            data = data.OrderByDescending(r => r.M_SALES_VOLUME).ToList();
                        break;
                }
            }
            return data.Skip(pageIndex * pageSize).Take(pageSize).ToList();
        }
    }
}
