using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MELTEX.DBController
{
    public class DBController
    {
        public static DataTable ExecuteQuery()
        {
            DataTable table = new DataTable();


            return table;
        }

        public static void Insert(string sqlconn, string tableName, ArrayList values)
        {
            using (SqlConnection sql = new SqlConnection(sqlconn))
            using (SqlCommand cmd = sql.CreateCommand())
            using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
            {
                List<string> p = new List<string>();

                foreach (var item in values)
                    p.Add($"@{Guid.NewGuid().ToString().Replace("-", "")}");

                String query =
                    $"INSERT INTO {tableName} " +
                    $"VALUES ({string.Join(",", p)}) ";

                sql.Open();

                cmd.CommandText = query;

                int index = 0;
                p.ForEach(el => cmd.Parameters.AddWithValue(el, values[index++]));

                cmd.ExecuteNonQuery();
            }

        }

        public static void Update()
        {

        }

        public static void Delete()
        {

        }
    }
}
