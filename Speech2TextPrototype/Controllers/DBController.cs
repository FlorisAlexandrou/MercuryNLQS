using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;

namespace Speech2TextPrototype.Controllers
{
    public class DBController : ControllerBase
    {
        private SqlConnection client = new SqlConnection(@"Server=DESKTOP-H7CTPU9\SQLEXPRESS; Database=floris; Integrated Security=SSPI;");
        
        public void openClient()
        {
            client.Open();
        }

        public void closeClient()
        {
            client.Close();
        }

        public List<string[]> tokenLookup(string token)
        {
            SqlCommand cmd = new SqlCommand("SELECT * FROM LOOKUP_VALUES WHERE [Value]='" + token + "'", client);
            SqlDataReader rdr = cmd.ExecuteReader();
            List<string[]> result = new List<string[]>();
            while (rdr.Read())
            {
                string[] response = new string[4]; 
                string value = rdr["Value"].ToString();
                string type = rdr["Type"].ToString();
                string whereStmt = rdr["WhereStmt"].ToString();
                string whereType = rdr["WhereType"].ToString();
                response[0] = value;
                response[1] = type;
                response[2] = whereStmt;
                response[3] = whereType;
                result.Add(response);
            }
            rdr.Close();
            return result;
        }
    }
}