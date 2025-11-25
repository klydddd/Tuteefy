using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace TuteefyWPF
{
    public class Database
    {
        public string connectionString;
        public Database()
        {
            var cs = ConfigurationManager.ConnectionStrings["TuteefyDB"];
            if (cs == null || string.IsNullOrWhiteSpace(cs.ConnectionString))
            {
                throw new InvalidOperationException(
                    "Connection string 'TuteefyDB' not found in App.config. " +
                    "Add a <connectionStrings> entry with name='TuteefyDB'.");
            }

            connectionString = cs.ConnectionString;
        }

        public bool TestConnection(out string error)
        {
            error = null;
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    conn.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                error = ex.ToString();
                return false;
            }
        }

        public DataTable LoadUsers()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT * FROM UserTable", conn);
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                return dt;
            }
        }
    }
}
