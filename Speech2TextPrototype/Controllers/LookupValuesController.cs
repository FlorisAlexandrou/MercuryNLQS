using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Speech2TextPrototype.Data;
using Speech2TextPrototype.Models;

namespace Speech2TextPrototype.Controllers
{
    [Route("api/lookup")]
    [ApiController]
    public class LookupValuesController : ControllerBase
    {
        private readonly florisContext _context;

        public LookupValuesController(florisContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("token/{text}")]
        public async Task<string> tokenLookupAsync(string text)
        {
            string url = "https://tokens-api.herokuapp.com/tokenize/";

            using (HttpClient client = new HttpClient())
            {
                // Tokenize Sentence
                var httpResponse = await client.GetStringAsync(url + text);
                PyRes res = JsonConvert.DeserializeObject<PyRes>(httpResponse);
                /**************** IF IS SQL QUERY *******************/
                string[] tokens = res.tokens;
                string[] bigrams = res.bigrams;

                List<LookupValues> listKnownTokens = new List<LookupValues>();
                List<string> listMeasures = new List<string>();
                List<string> listMeasureValues = new List<string>();
                List<string> listDates = new List<string>();
                List<string> listDateValues = new List<string>();
                List<string> listWhereStatements = new List<string>();

                // Check for keywords Between or Past - Mabye can be implemented in Python
                // TODO

                // Lookup Bigrams
                foreach (string bigram in bigrams)
                {
                    LookupValues lookupValues = _context.lookupvalues.Where(row => row.Value == bigram).FirstOrDefault();
                    if (lookupValues != null)
                    {
                        // Find Measurables
                        if (lookupValues.Type == "Measure")
                        {
                            listMeasures.Add(lookupValues.WhereStmt);
                            listMeasureValues.Add(lookupValues.Value);
                        }
                        // Find WhereStmts
                        else if (lookupValues.Type == "Date")
                        {
                            listDates.Add(lookupValues.WhereStmt);
                            listDateValues.Add(lookupValues.Value);
                        }
                    }
                }

                // Delete Tokens identified as known bigrams
                if (listMeasures.Any() || listDates.Any())
                {
                    var listTokens = new List<string>(tokens);

                    for (int i = listTokens.Count - 1; i >= 0; i--)
                    {
                        // Delete measures - tokens
                        foreach (string measure in listMeasureValues)
                        {
                            if (measure.Contains(listTokens[i], StringComparison.OrdinalIgnoreCase))
                            {
                                listTokens.RemoveAt(i);
                                break;
                            }
                        }

                        // Delete dates - tokens
                        foreach (string date in listDateValues)
                        {
                            if (date.Contains(listTokens[i], StringComparison.OrdinalIgnoreCase))
                            {
                                listTokens.RemoveAt(i);
                                break;
                            }
                        }
                    }
                    tokens = listTokens.ToArray();
                }

                // Lookup Tokens
                foreach (string token in tokens)
                {
                    LookupValues lookupValues = _context.lookupvalues.Where(row => row.Value == token).FirstOrDefault();
                    if (lookupValues != null)
                    {
                        // Find Measurables
                        if (lookupValues.Type == "Measure")
                        {
                            listMeasures.Add(lookupValues.WhereStmt);
                        }
                        // Find WhereStmts
                        else if (lookupValues.Type == "Date")
                        {
                            listDates.Add(lookupValues.WhereStmt);
                        }
                        else
                        {
                            string statement = lookupValues.WhereStmt + " = " + "'" + lookupValues.Value + "'";
                            listWhereStatements.Add(statement);
                        }
                        listKnownTokens.Add(lookupValues);
                    }
                }
                // Query Construction
                string query = "";
                string measures = "";
                string wheres = "";

                if (listMeasures.Any())
                    measures = listMeasures.Aggregate((i, j) => i + ", " + j);

                if (listDates.Any())
                    wheres = listDates.Aggregate((i, j) => i + " OR " + j);

                if (!String.IsNullOrEmpty(wheres))
                    wheres += " AND ";
                wheres += listWhereStatements.Aggregate((i, j) => i + " AND " + j);

                if (String.IsNullOrEmpty(wheres))
                    query = "SELECT " + measures + " FROM TDATA";
                else
                    query = "SELECT " + measures + " FROM TDATA WHERE " + wheres;

                return query;
                var q = _context.tdata.
                    FromSqlRaw("SELECT " + listMeasures[0] + " FROM TData WHERE " + listWhereStatements[0]).ToList();
                //return listKnownTokens;
            }
        }
    }
}
