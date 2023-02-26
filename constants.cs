namespace LMY.Workflow
{
    internal static class ErrorCodes
    {
        public static string E001 { get { return "E001, error occurred while reading configuration file or trying to create table in database, check inner exception for more info"; } }
        public static string E002 { get { return "E002, error occurred while trying To Start new workflow, "; } }
        public static string E003 { get { return "E003, error occurred while trying To get available transitions, "; } }
        public static string E004 { get { return "E004, error occurred while trying To take transition, "; } }

        public static string E005 { get { return "E004, error occurred while trying To get tasks, "; } }
        public static string E006 { get { return "E004, error occurred while trying To get request logs, "; } }

        public static string E007 { get { return "E004, error occurred while trying To delegate task, "; } }
    }
}