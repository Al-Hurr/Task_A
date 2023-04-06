using System.Data.SqlClient;
using System;
using System.Text;
using System.IO;

namespace Task_A.DbContext
{
    internal class DbCreator : ProcessDbConnectionBase
    {
        protected const string _defaultDbName = "master";

        public DbCreator() : base(_dbConnection.Replace(_dbName, _defaultDbName))
        {

        }

        public bool TryCreateDb()
        {
            string createDbScript = new StringBuilder()
                .Append($"IF NOT EXISTS (SELECT name FROM sysdatabases WHERE ('[' + name + ']' = '{_dbName}' OR name = '{_dbName}')) ")
                .Append("begin ")
                .Append($"CREATE DATABASE {_dbName} ON PRIMARY ")
                .Append($"(NAME = {_dbName}_Data, ")
                .Append($"FILENAME = '{Directory.GetCurrentDirectory()}\\{_dbName}Data.mdf', ")
                .Append("SIZE = 2MB, MAXSIZE = 10MB, FILEGROWTH = 10%)")
                .Append($"LOG ON (NAME = {_dbName}Data_Log, ")
                .Append($"FILENAME = '{Directory.GetCurrentDirectory()}\\{_dbName}Data.ldf', ")
                .Append("SIZE = 1MB, ")
                .Append("MAXSIZE = 5MB, ")
                .Append("FILEGROWTH = 10%) ")
                .Append("end ")
                .ToString();

            return TryConnectAndExecute((string sqlScript) =>
            {
                bool isDbExists = false;
                using (SqlCommand sqlCommand = new SqlCommand("SELECT name FROM sysdatabases", _sqlConnection))
                {
                    using (SqlDataReader reader = sqlCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (reader[0].Equals(_dbName))
                            {
                                isDbExists = true;
                                break;
                            };
                        }
                    }
                }

                if (isDbExists)
                {
                    return;
                }

                using (SqlCommand sqlCommand = new SqlCommand(sqlScript, _sqlConnection))
                {
                    sqlCommand.ExecuteNonQuery();

                    Console.WriteLine("DataBase is Created Successfully");
                }
            }, 
            createDbScript);
        }

        public static DbCreator Create()
        {
            return new DbCreator();
        }
    }
}