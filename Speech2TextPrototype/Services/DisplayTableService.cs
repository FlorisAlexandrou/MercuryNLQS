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
    }
}
