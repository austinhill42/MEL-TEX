﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace MELTEX.Database
{
    public static class DBController
    {
        #region Methods

        public static void Delete()
        {
        }

        public static DataTable GetTableFromQuery(string sqlconn, string columns, string t1, string t1join = "",
                                                          string t1Alias = "t1", string t2 = "", string t2join = "",
                                                  string t2Alias = "t2", string orderTableAlias = "t1",
                                                  string orderCol = "", string searchTableAlias = "t1",
                                                  string searchCol = "", string searchStart = "",
                                                  string searchEqual = "")
        {
            DataTable table = new DataTable();

            using (SqlConnection sql = new SqlConnection(sqlconn))
            using (SqlCommand cmd = sql.CreateCommand())
            using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
            {
                string query =
                    $"SELECT {columns} " +
                    $"FROM {t1} {t1Alias} ";

                if (t2 != "" && t1join != "" && t2join != "")
                {
                    query += $"LEFT JOIN {t2} {t2Alias} ON {t1Alias}.{t1join} = {t2Alias}.{t2join} ";
                }

                if (searchCol != "")
                {
                    if (searchStart != "")
                    {
                        query += $"WHERE {searchTableAlias}.{searchCol} LIKE @search ";
                        cmd.Parameters.AddWithValue("@search", searchStart + "%");
                    }
                    else if (searchEqual != "")
                    {
                        query += $"WHERE {searchTableAlias}.{searchCol} = @search ";
                        cmd.Parameters.AddWithValue("@search", searchEqual);
                    }
                }

                if (orderCol != "")
                    query += $"ORDER BY {orderTableAlias}.{orderCol} ";

                sql.Open();

                cmd.CommandText = query;

                sda.Fill(table);
            }

            return table;
        }

        public static void Insert(string sqlconn, string tableName, List<Tuple<string, string>> values)
        {
            using (SqlConnection sql = new SqlConnection(sqlconn))
            using (SqlCommand cmd = sql.CreateCommand())
            using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
            {
                List<string> p = (values.Select(item => $"@{item.Item1}")).ToList();
                List<string> setString = new List<string>();

                values.ForEach(val => setString.Add($"[{val.Item1}]"));

                string query =
                    $"INSERT INTO {tableName} ({string.Join(", ",setString)}) " +
                    $"VALUES ({string.Join(",", p)}) ";

                sql.Open();

                cmd.CommandText = query;

                values.ForEach(el => cmd.Parameters.AddWithValue($"@{el.Item1}", el.Item2));

                cmd.ExecuteNonQuery();
            }
        }

        public static bool Update(string sqlconn, string tableName, List<Tuple<string, string>> setValues, List<Tuple<string, string>> whereValues)
        {
            using (SqlConnection sql = new SqlConnection(sqlconn))
            using (SqlCommand cmd = sql.CreateCommand())
            using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
            {
                List<string> setString = new List<string>();
                List<string> whereString = new List<string>();

                setValues.ForEach(val => setString.Add($"[{val.Item1}] = @{val.Item1}"));
                whereValues.ForEach(val => whereString.Add($"[{val.Item1}] = @old{val.Item1}"));

                setValues.ForEach(val => cmd.Parameters.AddWithValue($"@{val.Item1}", val.Item2));
                whereValues.ForEach(val => cmd.Parameters.AddWithValue($"@old{val.Item1}", val.Item2));

                string query =
                    $"UPDATE {tableName} " +
                    $"SET {string.Join(", ", setString)} " +
                    $"WHERE {string.Join(" AND ", whereString)} ";

                sql.Open();

                cmd.CommandText = query;

                return cmd.ExecuteNonQuery() > 0 ? true : false;
            }
        }

        #endregion Methods
    }
}