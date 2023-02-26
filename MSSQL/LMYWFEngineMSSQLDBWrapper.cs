using LMY.Workflow;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Text;

namespace LMY.Workflow.SQL
{
    internal class LMYWFEngineMSSQLDBWrapper : ILMYWFEngineDBWrapper
    {
        SqlTransaction _transaction;
        SqlConnection _connection;

        public LMYWFEngineMSSQLDBWrapper()
        {

        }
 
        public void ConfigureDB(string connectionString)
        {
            _connection = new SqlConnection(connectionString);
            OpenConnection();
            ExecuteNonQuery(GetFileContent("MSSQL.createRequestsTable.sql"));
            ExecuteNonQuery(GetFileContent("MSSQL.createRequestStatusLogsTable.sql"));
            ExecuteNonQuery(GetFileContent("MSSQL.createRequestTasksTable.sql"));
            CloseConnection();
        }

        public void OpenConnection()
        {
            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }
        }

        public void CloseConnection()
        {
            if (_connection.State != ConnectionState.Closed)
            {
                _connection.Close();
            }
        }

        public void BeginTransaction()
        {
            OpenConnection();
            _transaction = _connection.BeginTransaction();
        }

        public void CommitTransaction()
        {
            if (_transaction != null)
            {
                try
                {
                    _transaction.Commit();
                }
                catch
                {

                }
            }

            CloseConnection();
        }

        public void RollbackTransaction()
        {
            if (_transaction != null)
            {
                try
                {
                    _transaction.Rollback();
                }
                catch
                {

                }
            }

            CloseConnection();
        }

        public DataTable ExecuteSelect(string query)
        {
            using (var command = new SqlCommand(query, _connection, _transaction))
            {
                var adapter = new SqlDataAdapter(command);
                var table = new DataTable();
                adapter.Fill(table);
                return table;
            }
        }

        public int ExecuteNonQuery(string query)
        {
            using (var command = new SqlCommand(query, _connection, _transaction))
            {
                return command.ExecuteNonQuery();
            }
        }

        public object ExecuteScalar(string query)
        {
            using (var command = new SqlCommand(query, _connection, _transaction))
            {
                return command.ExecuteScalar();
            }
        }

        public int Insert(string tableName, LMYWFEngineDictionary data)
        {
            using (var command = new SqlCommand())
            {
                command.Connection = _connection;
                command.Transaction = _transaction;
                command.CommandType = CommandType.Text;

                var columns = string.Join(",", data.GetKeys());
                var valuesVariables = string.Join(",", data.GetKeys("@"));

                command.CommandText = $"INSERT INTO {tableName} ({columns}) VALUES ({valuesVariables})";

                foreach (var pair in data.Pairs)
                {
                    command.Parameters.AddWithValue("@" + pair.Key, pair.Value == null ? DBNull.Value : pair.Value);
                }

                return command.ExecuteNonQuery();
            }
        }

        public int Update(string tableName, LMYWFEngineDictionary data, LMYWFEngineDictionary where)
        {
            using (SqlCommand command = new SqlCommand())
            {
                command.Connection = _connection;
                command.Transaction = _transaction;
                command.CommandType = CommandType.Text;

                StringBuilder updates = new StringBuilder();
                StringBuilder conditions = new StringBuilder();

                foreach (LMYWFEngineKeyValuePair pair in data.Pairs)
                {

                    updates.AppendFormat("[{0}] = @{0},", pair.Key);
                    command.Parameters.AddWithValue(pair.Key, pair.Value == null ? DBNull.Value : pair.Value);
                }

                updates.Remove(updates.Length - 1, 1);

                foreach (LMYWFEngineKeyValuePair pair in where.Pairs)
                {
                    conditions.AppendFormat("[{0}] = @{0} AND ", pair.Key);
                    command.Parameters.AddWithValue(pair.Key, pair.Value == null ? DBNull.Value : pair.Value);
                }

                conditions.Remove(conditions.Length - 5, 5);

                command.CommandText = string.Format("UPDATE {0} SET {1} WHERE {2}", tableName, updates, conditions);
                return command.ExecuteNonQuery();
            }
        }

        private string GetFileContent(string fileName)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourceName = "LMY.Workflow." + fileName;

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                string result = reader.ReadToEnd();
                return result;
            }
        }

    }
}