
using System.Data;
using System.Reflection;
using System.Text.Json;

namespace LMY.Workflow
{
    internal class LMYWFEngineUtilities
    {
        public static LMYWFEngineRequest[] ConvertDataTableToRequestsObject(DataTable table)
        {
            LMYWFEngineRequest[] requests = new LMYWFEngineRequest[table.Rows.Count];

            for (int i = 0; i < table.Rows.Count; i++)
            {
                DataRow row = table.Rows[i];
                LMYWFEngineRequest lmyWFEngineRequest = new LMYWFEngineRequest()
                {
                    LMYWFEngineRequestID = CastDBObjectAsValue<string>(row["LMYWFEngineRequestID"]),
                    LMYWFEngineRequestVersionID = CastDBObjectAsValue<string>(row["LMYWFEngineRequestVersionID"]),
                    WorkFlowName = CastDBObjectAsValue<string>(row["WorkFlowName"]),
                    Status = CastDBObjectAsValue<string>(row["Status"]),
                    RequestCustomData = CastDBObjectAsValue<string>(row["RequestCustomData"]),
                    Created_BY = CastDBObjectAsValue<string>(row["Created_BY"]),
                    Created_DATE = CastDBObjectAsValue<DateTime>(row["Created_DATE"]),
                    Modified_BY = CastDBObjectAsValue<string>(row["Modified_BY"]),
                    Modified_DATE = CastDBObjectAsValue<DateTime>(row["Modified_DATE"])
                };

                requests[i] = lmyWFEngineRequest;
            };


            return requests;
        }

        public static LMYWFEngineRequestTask[] ConvertDataTableToRequestTasksObject(DataTable table)
        {
            LMYWFEngineRequestTask[] tasks = new LMYWFEngineRequestTask[table.Rows.Count];

            for (int i = 0; i < table.Rows.Count; i++)
            {
                DataRow row = table.Rows[i];
                LMYWFEngineRequestTask lmyWFEngineRequestTask = new LMYWFEngineRequestTask
                {
                    LMYWFEngineTaskID = CastDBObjectAsValue<string>(row["tasks_LMYWFEngineTaskID"]),
                    LMYWFEngineRequestID = CastDBObjectAsValue<string>(row["tasks_LMYWFEngineRequestID"]),
                    UserGroup = CastDBObjectAsValue<string>(row["tasks_UserGroup"]),
                    DelegatedToUser = CastDBObjectAsValue<string>(row["tasks_DelegatedToUser"]), 
                    Created_BY = CastDBObjectAsValue<string>(row["tasks_Created_BY"]),
                    Created_DATE = CastDBObjectAsValue<DateTime>(row["tasks_Created_DATE"]),
                    Modified_BY = CastDBObjectAsValue<string>(row["tasks_Modified_BY"]),
                    Modified_DATE = CastDBObjectAsValue<DateTime>(row["tasks_Modified_DATE"]),

                    Request = new LMYWFEngineRequest()
                    {
                        LMYWFEngineRequestID = CastDBObjectAsValue<string>(row["requests_LMYWFEngineRequestID"]),
                        LMYWFEngineRequestVersionID = CastDBObjectAsValue<string>(row["requests_LMYWFEngineRequestVersionID"]),
                        Status = CastDBObjectAsValue<string>(row["requests_Status"]),
                        RequestCustomData = CastDBObjectAsValue<string>(row["requests_RequestCustomData"]),
                        Created_BY = CastDBObjectAsValue<string>(row["requests_Created_BY"]),
                        Created_DATE = CastDBObjectAsValue<DateTime>(row["requests_Created_DATE"]),
                        Modified_BY = CastDBObjectAsValue<string>(row["requests_Modified_BY"]),
                        Modified_DATE = CastDBObjectAsValue<DateTime>(row["requests_Modified_DATE"]),
                    }
                };


                tasks[i] = lmyWFEngineRequestTask;
            }

            return tasks;
        }

        public static LMYWFEngineRequestStatusLog[] ConvertDataTableToRequestStatusLogsObject(DataTable table)
        {
            LMYWFEngineRequestStatusLog[] statusLogs = new LMYWFEngineRequestStatusLog[table.Rows.Count];

            for (int i = 0; i < table.Rows.Count; i++)
            {
                DataRow row = table.Rows[i];
                LMYWFEngineRequestStatusLog lmyFEngineRequestStatusLog = new LMYWFEngineRequestStatusLog
                {
                    LMYWFEngineLogID = CastDBObjectAsValue<string>(row["LMYWFEngineLogID"]),
                    LMYWFEngineRequestID = CastDBObjectAsValue<string>(row["RequestID"]),
                    OldStatus = CastDBObjectAsValue<string>(row["OldStatus"]),
                    NewStatus = CastDBObjectAsValue<string>(row["NewStatus"]),
                    Created_BY = CastDBObjectAsValue<string>(row["Created_BY"]),
                    Created_DATE = CastDBObjectAsValue<DateTime>(row["Created_DATE"]),
                    Modified_BY = CastDBObjectAsValue<string>(row["Modified_BY"]),
                    Modified_DATE = CastDBObjectAsValue<DateTime>(row["Modified_DATE"])
                };

                statusLogs[i] = lmyFEngineRequestStatusLog;
            }

            return statusLogs;
        }

        public static string JoinStringArrayForSqlQuery(string[] values)
        {

            for (int i = 0; i < values.Length; i++)
            {
                values[i] = $"'{values[i]}'";
            }

            return string.Join(",", values);
        }


        public static T CastDBObjectAsValue<T>(object value)
        {
            if (value == DBNull.Value)
            {
                return default(T);
            }
            else
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
        }


        public static T GetJsonElementValue<T>(JsonElement jsonElement, string path)
        {
            var elements = path.Split('.');

            for (int i = 0; i < elements.Length; i++)
            {
                string element = elements[i];

                if (jsonElement.ValueKind == JsonValueKind.Object)
                {
                    if (!jsonElement.TryGetProperty(element, out JsonElement next))
                    {
                        return default;
                    }
                    jsonElement = next;
                }
                else if (jsonElement.ValueKind == JsonValueKind.Array)
                {
                    int index = int.Parse(element);
                    if (index >= jsonElement.GetArrayLength())
                    {
                        return default;
                    }
                    jsonElement = jsonElement[index];
                }
                else
                {
                    return default;
                }
            }

            switch (jsonElement.ValueKind)
            {
                case JsonValueKind.String:
                    if (typeof(T) == typeof(string))
                    {
                        return (T)(object)jsonElement.GetString();
                    }
                    return default;

                case JsonValueKind.Number:
                    if (typeof(T) == typeof(int))
                    {
                        return (T)(object)jsonElement.GetInt32();
                    }
                    else if (typeof(T) == typeof(double))
                    {
                        return (T)(object)jsonElement.GetDouble();
                    }
                    else if (typeof(T) == typeof(float))
                    {
                        return (T)(object)(float)jsonElement.GetDouble();
                    }
                    return default;

                case JsonValueKind.True:
                    if (typeof(T) == typeof(bool))
                    {
                        return (T)(object)true;
                    }
                    return default;

                case JsonValueKind.False:
                    if (typeof(T) == typeof(bool))
                    {
                        return (T)(object)false;
                    }
                    return default;

                case JsonValueKind.Null:
                    return default;

                default:
                    return default;
            }
        }

        // public static AddObjectToArrary(object[] arr,object newObject){
        //     Array.Resize(ref arr, arr.Length + 1);
        //     arr[arr.Length - 1] = newObject;
        // }
    }
}