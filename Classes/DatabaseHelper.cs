using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace TuteefyWPF.Classes
{
    public class DatabaseHelper
    {
        // Update this with your actual SQL Server connection details
        // Example: @"Data Source=.\SQLEXPRESS;Initial Catalog=TuteefyDB;Integrated Security=True"
        // Or with SQL Server authentication: @"Data Source=YOUR_SERVER;Initial Catalog=TuteefyDB;User ID=sa;Password=yourpassword"
        private static string connectionString = @"Data Source=.\SQLEXPRESS;Initial Catalog=TuteefyDB;User ID=sa;Password=123456;TrustServerCertificate=True";

        // Test database connection
        public static bool TestConnection()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        // Execute SELECT query and return DataTable
        public static DataTable ExecuteQuery(string query, Dictionary<string, object> parameters = null)
        {
            DataTable dt = new DataTable();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        if (parameters != null)
                        {
                            foreach (var param in parameters)
                            {
                                cmd.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                            }
                        }

                        conn.Open();
                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            adapter.Fill(dt);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Database query error: {ex.Message}", ex);
            }

            return dt;
        }

        // Execute INSERT, UPDATE, DELETE and return affected rows
        public static int ExecuteNonQuery(string query, Dictionary<string, object> parameters = null)
        {
            int affectedRows = 0;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        if (parameters != null)
                        {
                            foreach (var param in parameters)
                            {
                                cmd.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                            }
                        }

                        conn.Open();
                        affectedRows = cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Database operation error: {ex.Message}", ex);
            }

            return affectedRows;
        }

        // Execute scalar query and return single value
        public static object ExecuteScalar(string query, Dictionary<string, object> parameters = null)
        {
            object result = null;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        if (parameters != null)
                        {
                            foreach (var param in parameters)
                            {
                                cmd.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                            }
                        }

                        conn.Open();
                        result = cmd.ExecuteScalar();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Database scalar operation error: {ex.Message}", ex);
            }

            return result;
        }
    }
}