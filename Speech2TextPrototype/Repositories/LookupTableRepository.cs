using Consumer_Retail_Research_Analytics_NLP.Models;
using Microsoft.EntityFrameworkCore;
using Speech2TextPrototype.Data;
using Speech2TextPrototype.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Speech2TextPrototype.Repositories
{
    public class LookupTableRepository : ILookupTableRepository
    {
        private readonly florisContext _context;

        /// <summary>List of selected measures, e.g. M_SALES_VALUE</summary>
        private List<string> listMeasures = new List<string>();

        /// <summary>Value of the selected measure in the lookup table, e.g. sale item </summary>
        private List<string> listMeasureValues = new List<string>();

        /// <summary>List of sql date filters, e.g. convert(varchar(7), PERIOD_START, 23) = '2016-01'</summary>
        private List<string> listDates = new List<string>();

        /// <summary>Value of date filters, e.g. january 2016</summary>
        private List<string> listDateValues = new List<string>();

        /// <summary>List of database-related filters e.g. Brand: "Agros"</summary>
        private List<string> listWhereStatements = new List<string>();

        /// <summary>Array of statically defined aggregate functions "max, sum"</summary>
        string[] aggregates = { "max", "min", "sum", "count", "avg" };

        /// <summary>Array of statically defined aggregate functions closer to natural language e.g. "maximum, total"</summary>
        string[] aggAlternatives = { "maximum", "minimum", "total", "number", "average" };

        /// <summary>String of the aggregate function that is mapped between the 2 arrays (aggregates, aggAlternatives), e.g. Maximum => max</summary>
        private string aggregateFunction = "";

        /// <summary>Indicates whether there is a "between" keyword in the query</summary>
        private bool isBetween = false;

        // Used for Past x Months, Days
        /// <summary>String that indicates the number of Past Years or Months or Days</summary>
        private string timeNum = "1";

        /// <summary>String which is used for Past X {Year, Month, Day}</summary>
        private string timeType = "";

        /// <summary>Indicates wheter there is a "top" keyword in the query</summary>
        private bool isTop = false;

        /// <summary>Indicates the value of top (e.g. top 5 sales), 10 by default</summary>
        private string topNum = "10";

        public LookupTableRepository(florisContext context)
        {
            _context = context;
        }

        /// <summary>
        /// MAIN FUNCTION
        /// Query the database based on the natural language tokens received from the Python NLTK api
        /// </summary>
        /// <param name="res">Python Tokenizer Response</param>
        /// <returns>Tuple containing: the table response, the list of measurables, the list of dates and the query as a string</returns>
        public SqlAnswer token2Sql(PyRes res)
        {
            string[] tokens = res.tokens;
            string[] bigrams = res.bigrams;

            BigramLookup(bigrams);

            tokens = DeleteKnownTokens(tokens);

            TokenLookup(tokens);

            string query = ConstructQuery();

            double scalar = -1;

            // Check for scalar values (Max, Sum, etc.)
            // Uses reflection for the measurable to avoid numerous if statements (x3 code)
            if (!String.IsNullOrEmpty(aggregateFunction))
                {
                var result = _context.tdata.FromSqlRaw(query).ToList();
                Type type = typeof(TData);

                switch (aggregateFunction)
                {
                    case "max":
                        scalar = Convert.ToDouble(result.ToList().Max(r => type.GetProperty(listMeasures[0]).GetValue(r)));
                        break;
                    case "min":
                        scalar = Convert.ToDouble(result.ToList().Min(r => type.GetProperty(listMeasures[0]).GetValue(r)));
                        break;
                    case "sum":
                        scalar = result.ToList().Sum(r => Convert.ToDouble(type.GetProperty(listMeasures[0]).GetValue(r)));
                        break;
                    case "avg":
                        scalar = result.ToList().Average(r => Convert.ToDouble(type.GetProperty(listMeasures[0]).GetValue(r)));
                        break;
                }

                return new SqlAnswer { measures = listMeasures, sqlQuery = query, scalar = scalar };
            }

            return new SqlAnswer { measures = listMeasures, sqlQuery = query, scalar = scalar };
        }
    

        /// <summary>
        /// Search for 2-word tokens as measures or dates
        /// </summary>
        /// <param name="bigrams">Array of 2-word tokens from the python script</param>
        /// <returns>void - passing by erence</returns>
        private void BigramLookup(string[] bigrams)
        {
            foreach (string bigram in bigrams)
            {
                LookupTable bigramLookupValues = _context.lookupvalues.Where(row => row.Value == bigram).FirstOrDefault();
                if (bigramLookupValues != null)
                {
                    // Find Measurables
                    if (bigramLookupValues.Type == "Measure")
                    {
                        listMeasures.Add(bigramLookupValues.WhereStmt);
                        listMeasureValues.Add(bigramLookupValues.Value);
                    }
                    // Find Dates
                    else if (bigramLookupValues.Type == "Date")
                    {
                        listDates.Add(bigramLookupValues.WhereStmt);
                        listDateValues.Add(bigramLookupValues.Value);
                    }
                    // Find other "where" statements
                    else 
                    {
                        string statement = bigramLookupValues.WhereStmt + " = " + "'" + bigramLookupValues.Value + "'";
                        listWhereStatements.Add(statement);
                    }
                }
            }
        }


        /// <summary>
        /// Delete Tokens identified as known bigrams
        /// </summary>
        /// <param name="tokens">Array of single-word tokens from the python script</param>
        /// <returns>Updated tokens array</returns>
        private string[] DeleteKnownTokens(string[] tokens)
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
        /// <returns>void - passing by erence</returns>
        private void TokenLookup(string[] tokens)
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
                    if (int.TryParse(tokens[tokenIndex + 1], out int number))
                    {
                        timeNum = (int.Parse(tokens[tokenIndex + 1]) - 1).ToString();
                        timeType = tokens[tokenIndex + 2];
                    }
                    else
                    {
                        timeType = tokens[tokenIndex + 1];
                    }
                    continue;
                }

                if (token == "top")
                {
                    isTop = true;
                    if (int.TryParse(tokens[tokenIndex + 1], out int number))
                    {
                        topNum = tokens[tokenIndex + 1];
                    }
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

                LookupTable lookupValues = _context.lookupvalues.Where(row => row.Value == token).FirstOrDefault();
                if (lookupValues != null)
                {
                    // Find Measurables
                    if (lookupValues.Type == "Measure")
                    {
                        listMeasures.Add(lookupValues.WhereStmt);
                    }
                    // Find Dates
                    else if (lookupValues.Type == "Date")
                    {
                        listDates.Add(lookupValues.WhereStmt);
                    }
                    // Find other "where" statements
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
        /// <returns>query</returns>
        private string ConstructQuery()
        {
            string query = "";
            string measures = "";
            string wheres = "";
            bool listDatesExist = listDates.Any();

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
                if (!listDatesExist)
                {
                    // Display last months or days of current year
                    if (timeType == "month" || timeType == "day")
                        wheres += timeType + "(PERIOD_START) between (" + timeType + "(getdate()) - " + timeNum + ") and getdate() and YEAR(PERIOD_START) = YEAR(getdate())";  
                
                    // Display last x years
                    if (timeType == "year")
                        wheres += timeType + "(PERIOD_START) between (" + timeType + "(getdate()) - " + timeNum + ") and getdate()";
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
            else if (listDatesExist && isBetween)
            {
                wheres += listDates.Aggregate((i, j) => i.Split('=')[0] + "Between" + i.Split('=')[1] + " AND" + j.Split('=')[1]);
            }
            else if (listDatesExist)
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
            if (isTop)
                query = $"SELECT TOP {topNum} * FROM TDATA";
            else
                query = "SELECT * FROM TDATA";

            if (!String.IsNullOrEmpty(wheres))
                query += " WHERE " + wheres;

            return query;
        }

        public string GroupByFilters (string query, string groupByFilter, string uuid)
        {
            // Add TOP statement
            string topStatement = "";
            if (query.Contains("TOP"))
                topStatement = "TOP " + query.Split("TOP")[1].Trim().Split()[0];

            string selectStatement = $"SELECT {topStatement} SUM(M_SALES_VALUE) AS M_SALES_VALUE, " +
                                     "SUM(M_SALES_VOLUME) AS M_SALES_VOLUME, " +
                                     "COUNT(M_SALES_ITEMS) AS M_SALES_ITEMS, " +
                                     "PERIOD_START, " + groupByFilter;

            // Group by the selected granularity
            if (groupByFilter == "PRODUCT_NAME")
                selectStatement += ", MAX(CATEGORY_NAME) AS CATEGORY_NAME, MAX(BRAND) AS BRAND";
            else if (groupByFilter == "CATEGORY_NAME")
                selectStatement += ", MAX(PRODUCT_NAME) AS PRODUCT_NAME, MAX(BRAND) AS BRAND";
            else if (groupByFilter == "BRAND")
                selectStatement += ", MAX(CATEGORY_NAME) AS CATEGORY_NAME, MAX(PRODUCT_NAME) AS PRODUCT_NAME";

            string fromStatement = " FROM TDATA";
            string whereStatement = "";
            if (query.Contains("WHERE"))
                whereStatement = " WHERE " + query.Split(new[] { "WHERE" }, StringSplitOptions.None)[1];
            string groupByStatement = " GROUP BY PERIOD_START, " + groupByFilter;

            // Sort to show top/best sales
            if (topStatement.Length > 0)
            {
                groupByStatement += " ORDER BY M_SALES_VALUE DESC, M_SALES_VOLUME DESC, M_SALES_ITEMS DESC";
            }

            // Delete previously generated results of current user
            var toDelete = _context.displayTable.Where(r => r.UUID == uuid).ToList();
            _context.displayTable.RemoveRange(toDelete);
            _context.SaveChanges();

            var result = _context.tdata.FromSqlRaw(selectStatement + fromStatement + whereStatement + groupByStatement);

            var dt = result.Select(r => new DisplayTable()
            {
                UUID = uuid,
                BRAND = r.BRAND,
                CATEGORY_NAME = r.CATEGORY_NAME,
                PRODUCT_NAME = r.PRODUCT_NAME,
                PERIOD_START = r.PERIOD_START,
                M_SALES_VALUE = r.M_SALES_VALUE,
                M_SALES_VOLUME = r.M_SALES_VOLUME,
                M_SALES_ITEMS = r.M_SALES_ITEMS
            }).ToList();
            // Save new results to display table
            _context.displayTable.AddRange(dt);
            _context.SaveChanges();

            string error = "";

            if (result.Count() == 0)
                error = "ERROR:No Query Result";

            return error;
        }

        // Add all cypriot brands, areas and outlets to the speech recognizer
        public List<string> GetSpeechRecognitionCustomWords()
        {
            var sql = @"SELECT DISTINCT BRAND
                        FROM TDATA
                        UNION SELECT DISTINCT AREA_NAME
                        FROM TDATA
                        UNION SELECT DISTINCT OUTLET_NAME
                        FROM TDATA";

            var result = _context.tdata.FromSqlRaw(sql);
            return result.Select(r => r.BRAND).ToList();
        }
    }
}
