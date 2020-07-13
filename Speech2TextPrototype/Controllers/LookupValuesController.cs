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


        /// <summary>
        /// MAIN FUNCTION
        /// Query the database based on the natural language tokens received from the Python NLTK api
        /// </summary>
        /// <param name="res">Python Tokenizer Response</param>
        /// <returns>Tuple containing: the table response, the list of measurables and the query as a string</returns>
        public (List<TData>, List<string>, string) token2Sql(PyRes res)
        {
            string[] tokens = res.tokens;
            string[] bigrams = res.bigrams;

            List<string> listMeasures = new List<string>();
            List<string> listMeasureValues = new List<string>();
            List<string> listDates = new List<string>();
            List<string> listDateValues = new List<string>();
            List<string> listWhereStatements = new List<string>();
            // SUM, MAX, MIN, COUNT, AVG
            string aggregateFunction = "";
            string[] aggregates = { "max", "min", "sum", "count", "avg" };
            string[] aggAlternatives = { "maximum", "minimum", "total", "number", "average" };
            bool isBetween = false;
            // Used for Past x Months, Days
            string timeNum = "1";
            string timeType = "";

            bigramLookup(bigrams, ref listMeasures, ref listMeasureValues, ref listDates, ref listDateValues);

            tokens = DeleteKnownTokens(tokens, listMeasures, listMeasureValues, listDates, listDateValues);

            tokenLookup(tokens, aggregates, aggAlternatives, ref listMeasures, ref listDates, ref listWhereStatements,
                ref aggregateFunction, ref timeNum, ref timeType, ref isBetween);

            string query = ConstructQuery(listMeasures, listDates, listWhereStatements, aggregateFunction, timeType, timeNum, isBetween);

            var result = _context.tdata.FromSqlRaw(query).ToList();

            return (result, listMeasures, query);
        }


        /// <summary>
        /// Search for 2-word tokens as measures or dates
        /// </summary>
        /// <param name="bigrams">Array of 2-word tokens from the python script</param>
        /// <param name="listMeasures">List of selected measures, e.g. M_SALES_VALUE</param>
        /// <param name="listMeasureValues">Value of the selected measure in the lookup table, e.g. sale item</param>
        /// <param name="listDates">List of date filters, e.g. convert(varchar(7), PERIOD_START, 23) = '2016-01'</param>
        /// <param name="listDateValues">Value of date filters, e.g. january 2016</param>
        /// <returns>void - passing by reference</returns>
        private void bigramLookup(string[] bigrams,
            ref List<string> listMeasures, ref List<string> listMeasureValues, ref List<string> listDates, ref List<string> listDateValues)
        {
            foreach (string bigram in bigrams)
            {
                LookupValues bigramLookupValues = _context.lookupvalues.Where(row => row.Value == bigram).FirstOrDefault();
                if (bigramLookupValues != null)
                {
                    // Find Measurables
                    if (bigramLookupValues.Type == "Measure")
                    {
                        listMeasures.Add(bigramLookupValues.WhereStmt);
                        listMeasureValues.Add(bigramLookupValues.Value);
                    }
                    // Find WhereStmts
                    else if (bigramLookupValues.Type == "Date")
                    {
                        listDates.Add(bigramLookupValues.WhereStmt);
                        listDateValues.Add(bigramLookupValues.Value);
                    }
                }
            }
        }


        /// <summary>
        /// Delete Tokens identified as known bigrams
        /// </summary>
        /// <param name="tokens">Array of single-word tokens from the python script</param>
        /// <param name="listMeasures">List of selected measures, e.g. M_SALES_VALUE</param>
        /// <param name="listMeasureValues">Value of the selected measure in the lookup table, e.g. sale item</param>
        /// <param name="listDates">List of date filters, e.g. convert(varchar(7), PERIOD_START, 23) = '2016-01'</param>
        /// <param name="listDateValues">Value of date filters, e.g. january 2016</param>
        /// <returns>Updated tokens array</returns>
        private string[] DeleteKnownTokens(string[] tokens,
              List<string> listMeasures, List<string> listMeasureValues, List<string> listDates, List<string> listDateValues)
        {
            var listTokens = new List<string>(tokens);
            bool tokenDeleted = false;

            if (listMeasures.Any() || listDates.Any())
            {
                for (int i = listTokens.Count - 1; i >= 0; i--)
                {
                    // Delete measures - tokens
                    foreach (string measure in listMeasureValues)
                    {
                        if (measure.Contains(listTokens[i], StringComparison.OrdinalIgnoreCase))
                        {
                            listTokens.RemoveAt(i);
                            tokenDeleted = true;
                            break;
                        }
                    }


                    // Delete dates - tokens
                    if (!tokenDeleted) // Hotfix: Crashed because listTokens[i] was already deleted
                    {
                        foreach (string date in listDateValues)
                        {
                            if (date.Contains(listTokens[i], StringComparison.OrdinalIgnoreCase))
                            {
                                listTokens.RemoveAt(i);
                                break;
                            }
                        }
                    }
                    tokenDeleted = false;
                }
                tokens = listTokens.ToArray();
            }
            return tokens;
        }


        /// <summary>
        /// Search for single-word tokens as measures, aggregates, date or any other db-related filter
        /// </summary>
        /// <param name="tokens">Array of single-word tokens from the python script</param>
        /// <param name="aggregates">Array of statically defined aggregate functions "max, sum"</param>
        /// <param name="aggAlternatives">Array of statically defined aggregate functions closer to natural language e.g. "maximum, total"</param>
        /// <param name="listMeasures">List of selected measures, e.g. M_SALES_VALUE</param>
        /// <param name="listDates">List of date filters, e.g. convert(varchar(7), PERIOD_START, 23) = '2016-01'</param>
        /// <param name="listWhereStatements">List of database-related filters e.g. Brand: "Agros"</param>
        /// <param name="aggregateFunction">String of the aggregate function that is mapped between the 2 arrays (aggregates, aggAlternatives), e.g. Maximum => max</param>
        /// <param name="timeNum">String that indicates the number of Past Years or Months or Days</param>
        /// <param name="timeType">String which is used for Past X {Year, Month, Day}</param>
        /// <returns>void - passing by reference</returns>
        private void tokenLookup(string[] tokens, string[] aggregates, string[] aggAlternatives,
            ref List<string> listMeasures, ref List<string> listDates, ref List<string> listWhereStatements,
            ref string aggregateFunction, ref string timeNum, ref string timeType,
            ref bool isBetween)
        {
            int tokenIndex = 0;

            // Lookup Tokens
            foreach (string token in tokens)
            {
                // Find Between Keyword
                if (token == "between")
                {
                    isBetween = true;
                    continue;
                }

                if (token == "last" || token == "past")
                {
                    // Only Accept 2-digit numbers so that it will not produce an error when query does not contain timeNum "Past Month"
                    // Refine so that it accepts any number of digits, mabye check if token[tokenIndex+1] can be converted to INT or if it is    in ['day','month','year']
                    if (tokens[tokenIndex + 1].Length < 3)
                    {
                        timeNum = tokens[tokenIndex + 1];
                        timeType = tokens[tokenIndex + 2];
                    }
                    else
                    {
                        timeType = tokens[tokenIndex + 1];
                    }
                    continue;
                }

                // Check for aggregates
                for (int i = 0; i < aggregates.Length; i++)
                {
                    if (aggregates[i] == token || aggAlternatives[i] == token)
                    {
                        aggregateFunction = aggregates[i];
                        break;
                    }
                }

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
                }
                tokenIndex++;
            }
        }


        /// <summary>
        /// Construct the query by adding the known filters
        /// </summary>
        /// <param name="listMeasures">List of selected measures, e.g. M_SALES_VALUE</param>
        /// <param name="listDates">List of date filters, e.g. convert(varchar(7), PERIOD_START, 23) = '2016-01'</param>
        /// <param name="listWhereStatements">Value of date filters, e.g. january 2016</param>
        /// <param name="aggregateFunction">Aggregate function e.g. SUM, MAX</param>
        /// <param name="timeType">String which is used for Past X {Year, Month, Day}</param>
        /// <param name="timeNum">String that indicates the number of Past Years or Months or Days</param>
        /// <returns>query</returns>
        private string ConstructQuery(List<string> listMeasures, List<string> listDates, List<string> listWhereStatements,
            string aggregateFunction, string timeType, string timeNum,
            bool isBetween)
        {
            string query = "";
            string measures = "";
            string wheres = "";

            if (listMeasures.Any())
            {
                measures = listMeasures.Aggregate((i, j) => i + ", " + j);

                // Add aggregate function
                if (!String.IsNullOrEmpty(aggregateFunction))
                {
                    measures = measures.Insert(0, aggregateFunction.ToUpper() + "(");
                    measures += ")";
                }
            }

            // If past,last keyword is detected
            if (!String.IsNullOrEmpty(timeType))
            {
                // Display last months or days of current year
                if ((timeType == "month" || timeType == "day") && !listDates.Any())
                {
                    wheres += timeType + "(PERIOD_START) between (" + timeType + "(getdate()) - " + timeNum + ") and getdate() and YEAR(PERIOD_START) = YEAR(getdate())";
                }
                // Display last months or days of specific year, e.g last 2 months of 2016
                else
                {
                    var year = listDates[0].Split('=')[1].Replace(" ", "");

                    if (timeType == "month")
                    {
                        var monthDiff = 12 - Int32.Parse(timeNum);
                        string month = monthDiff.ToString();
                        if (monthDiff >= 1 && monthDiff <= 9)
                        {
                            month = "0" + month;
                        }
                        wheres += "CONVERT(VARCHAR(10), PERIOD_START) between '" + year + "-" + month + "-01' and '" + year + "-12-31'";
                    }
                    else if (timeType == "day")
                    {
                        var dayDiff = 31 - Int32.Parse(timeNum);
                        string day = dayDiff.ToString();
                        if (dayDiff >= 1 && dayDiff <= 9)
                        {
                            day = "0" + day;
                        }
                        wheres += "CONVERT(VARCHAR(10), PERIOD_START) between '" + year + "-12-" + dayDiff + "' and '" + year + "-12-31'";
                    }
                }
            }

            // If dates are BETWEEN a certain range
            else if (listDates.Any() && isBetween)
            {
                wheres += listDates.Aggregate((i, j) => i.Split('=')[0] + "Between" + i.Split('=')[1] + " AND" + j.Split('=')[1]);
            }
            else if (listDates.Any())
            {
                if (!String.IsNullOrEmpty(wheres))
                    wheres += " AND ";
                wheres += listDates.Aggregate((i, j) => "(" + i + " OR " + j + ")");

            }


            // Add the rest of the "where" Statements
            if (listWhereStatements.Any())
            {
                if (!String.IsNullOrEmpty(wheres))
                    wheres += " AND ";
                wheres += listWhereStatements.Aggregate((i, j) => i + " AND " + j);
            }
            // Construct Query
            if (String.IsNullOrEmpty(wheres))
                query = "SELECT * FROM TDATA";
            else
                query = "SELECT * FROM TDATA WHERE " + wheres;

            return query;
        }
    }
}
