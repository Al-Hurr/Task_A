using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Task_A.Models;

namespace Task_A.DbContext
{
    public abstract class ProcessDbConnectionBase
    {
        protected const string _dbConnection = "Server=(LocalDb)\\MSSQLlocalDB;Integrated security=SSPI;database=Gmap";
        protected const string _dbName = "Gmap";
        protected readonly SqlConnection _sqlConnection;
        protected Dictionary<Guid, Inventory> _inventories;

        public ProcessDbConnectionBase()
        {
            _sqlConnection = new SqlConnection(_dbConnection);
        }

        public ProcessDbConnectionBase(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new Exception("Wrong connection string");
            }

            _sqlConnection = new SqlConnection(connectionString);
        }

        protected bool TryConnectAndExecute(Action<string> executeSqlCommand, string sqlCommand)
        {
            if (string.IsNullOrEmpty(sqlCommand))
            {
                return false;
            }

            try
            {
                _sqlConnection.Open();
                executeSqlCommand(sqlCommand);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{nameof(DbCreator)} {ex}");

                return false;
            }
            finally
            {
                if (_sqlConnection.State == ConnectionState.Open)
                {
                    _sqlConnection.Close();
                }
            }
        }
    }
}
