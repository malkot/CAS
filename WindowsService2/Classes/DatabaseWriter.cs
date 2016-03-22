using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsService2.Classes
{
    internal class DatabaseWriter
    {
        private string mConnectionString;

        public DatabaseWriter()
        {
            this.mConnectionString = String.Empty;
        }

        public void Configure(string connectionString)
        {
            this.mConnectionString = connectionString;
        }

        public void SaveValue(int deviceId, double weightValue)
        {
            using (SqlConnection connection = new SqlConnection(this.mConnectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("insert into Arhiv (date, value, id) values (@date, @value, @id);", connection))
                {
                    command.Parameters.Add(new SqlParameter()
                    {
                        ParameterName = "@date",
                        Value = DateTime.Now,
                        SqlDbType = System.Data.SqlDbType.DateTime
                    });
                    command.Parameters.Add(new SqlParameter()
                    {
                        ParameterName = "@value",
                        Value = weightValue,
                        SqlDbType = System.Data.SqlDbType.Float
                    });
                    command.Parameters.Add(new SqlParameter()
                    {
                        ParameterName = "@id",
                        Value = deviceId,
                        SqlDbType = System.Data.SqlDbType.Int
                    });

                    int insertedRecords = command.ExecuteNonQuery();
                }

                connection.Close();
            }
        }
    }
}
