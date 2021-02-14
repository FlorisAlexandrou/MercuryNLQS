using System.Collections.Generic;
using Speech2TextPrototype.Models;
using Speech2TextPrototype.Data;
using Speech2TextPrototype.Services;
using Microsoft.AspNetCore.Mvc;
using Consumer_Retail_Research_Analytics_NLP.Models;

namespace Speech2TextPrototype.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class UserInputController : ControllerBase
    {
        private readonly ILookupTableService _lookupTableService;
        private readonly IDisplayTableService _displayTableService;

        public UserInputController(ILookupTableService lookupTableService, IDisplayTableService displayTableService)
        {
            _lookupTableService = lookupTableService;
            _displayTableService = displayTableService;
        }

        [HttpPost]
        [Route("sqlAnswer")]
        public SqlAnswer GetSqlAnswer([FromBody] PyRes pyRes)
        {
            _lookupTableService.GetSpeechRecognitionCustomWords();
            return _lookupTableService.Token2Sql(pyRes);   
        }

        [HttpGet]
        [Route("groupByAnswer")]
        public string GetGroupByAnswer(string uuid, string sqlQuery, string groupByFilter)
        {
            return _lookupTableService.GroupByFilters(sqlQuery, groupByFilter, uuid);
        }

        [HttpGet]
        [Route("table")]
        public List<DisplayTable> GetTableData(string uuid)
        {
            return _displayTableService.GetTableData(uuid);
        }

        [HttpGet]
        [Route("chart")]
        public List<DisplayTable> GetChartData(string uuid)
        {
            return _displayTableService.GetChartData(uuid);
        }

        /// <summary>
        /// Server-side Sorting
        /// </summary>
        /// <param name="column">The column to be sorted</param>
        /// <param name="sortOrder">Ascending or Descending sorting order</param>
        /// <param name="pageIndex">The index of the datatable page we are on</param>
        /// <param name="pageSize">The number of rows per page</param>
        /// <param name="uuid">User ID to enable concurrent usage of the displayTable</param>
        /// <returns>Sorted data back to the datatable</returns>
        [HttpGet]
        [Route("table/sort")]
        public List<DisplayTable> GetTableSorted(string column, string sortOrder, int pageIndex, int pageSize, string uuid)
        {
            return _displayTableService.GetTableSorted(column, sortOrder, pageIndex, pageSize, uuid);
        }

        /// <summary>
        /// Server-side Pagination
        /// </summary>
        /// <param name="pageIndex">The index of the datatable page we are on</param>
        /// <param name="pageSize">The number of rows per page</param>
        /// <param name="uuid">User ID to enable concurrent usage of the displayTable</param>
        /// <returns>Paged data back to the datatable</returns>
        [HttpGet]
        [Route("table/page")]
        public List<DisplayTable> GetTablePaged(int pageIndex, int pageSize, string uuid)
        {
            return _displayTableService.GetTablePaged(pageIndex, pageSize, uuid);
        }

        [HttpPost]
        [Route("table/save")]
        public void SaveTableData([FromBody] List<DisplayTable> tableData)
        {
            _displayTableService.SaveData(tableData);
        }

        [HttpGet]
        [Route("table/delete")]
        public void DeleteTable(string uuid)
        {
            _displayTableService.DeleteData(uuid);
        }

        [HttpGet]
        [Route("speechRecogitionCustomWords")]
        public List<string> GetSpeechRecognitionCustomWords()
        {
            return _lookupTableService.GetSpeechRecognitionCustomWords();
        }
    }
}
