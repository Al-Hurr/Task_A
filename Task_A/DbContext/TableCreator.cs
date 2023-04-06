using System.Data.SqlClient;
using System;
using Task_A.Models;

namespace Task_A.DbContext
{
    internal class TableCreator : ProcessDbConnectionBase
    {
        public TableCreator() : base()
        {

        }

        public bool TryCreateTable()
        {
            string createTableScript = "IF OBJECT_ID(N'dbo.Inventory', N'U') IS NULL " +
            "begin " +
            "CREATE TABLE [dbo].[Inventory](" +
            "[InventoryID] uniqueidentifier NOT NULL PRIMARY KEY, " +
            "[Latitude] decimal(31, 10) NOT NULL, " +
            "[Longitude] decimal(31, 10) NOT NULL) " +
            "end";

            return TryConnectAndExecute((string sqlScript) =>
            {
                bool isTableExists = false;
                using (SqlCommand sqlCommand = new SqlCommand("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES", _sqlConnection))
                {
                    using (SqlDataReader reader = sqlCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (reader[0].Equals(nameof(Inventory)))
                            {
                                isTableExists = true;
                                break;
                            };
                        }
                    }
                }

                if (isTableExists)
                {
                    return;
                }

                using (SqlCommand sqlCommand = new SqlCommand(sqlScript, _sqlConnection))
                {
                    sqlCommand.ExecuteNonQuery();

                    Console.WriteLine("Table is Created Successfully");
                }
            },
            createTableScript);
        }

        public static TableCreator Create()
        {
            return new TableCreator();
        }
    }
}