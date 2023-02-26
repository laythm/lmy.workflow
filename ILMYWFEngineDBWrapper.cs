using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMY.Workflow
{
    public interface ILMYWFEngineDBWrapper
    {
        public void ConfigureDB(string connectionString);
        public void OpenConnection();
        public void CloseConnection();
        public void BeginTransaction();
        public void CommitTransaction();
        public void RollbackTransaction();
        public DataTable ExecuteSelect(string query);
        public int ExecuteNonQuery(string query);
        public object ExecuteScalar(string query);
        public int Insert(string tableName, LMYWFEngineDictionary data);
        public int Update(string tableName, LMYWFEngineDictionary data, LMYWFEngineDictionary where);
    }
}
