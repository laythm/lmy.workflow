
using System.IO;
using System.Text.Json;

namespace LMY.Workflow
{
    public class LMYWFEngineTransition
    {
        private string _LMYWFEngineTransitionID { get; set; }
        private string _LMYWFEngineRequestVersionID { get; set; }
        private string[] _UserGroups { get; set; }
        private string _TransitionName { get; set; }
        private string[] _CurrentStatuses { get; set; }
        private string _NextStatus { get; set; }
        private string _TransitionPageName { get; set; }
        private JsonElement _TransitionCustomData { get; set; }

        public string LMYWFEngineTransitionID { get { return _LMYWFEngineTransitionID; } set { CheckIsLocked(); _LMYWFEngineTransitionID = value; } }
        public string LMYWFEngineRequestVersionID { get { return _LMYWFEngineRequestVersionID; } set { CheckIsLocked(); _LMYWFEngineRequestVersionID = value; } }
        public string[] UserGroups { get { return _UserGroups; } set { CheckIsLocked(); _UserGroups = value; } }
        public string TransitionName { get { return _TransitionName; } set { CheckIsLocked(); _TransitionName = value; } }
        public string[] CurrentStatuses { get { return _CurrentStatuses; } set { CheckIsLocked(); _CurrentStatuses = value; } }
        public string NextStatus { get { return _NextStatus; } set { CheckIsLocked(); _NextStatus = value; } }
        public string TransitionPageName { get { return _TransitionPageName; } set { CheckIsLocked(); _TransitionPageName = value; } }
        public JsonElement TransitionCustomData { get { return _TransitionCustomData; } set { CheckIsLocked(); _TransitionCustomData = value; } }

        public T TransitionCustomData_GetValue<T>(string propertyPath)
        {
            return LMYWFEngineUtilities.GetJsonElementValue<T>(TransitionCustomData, propertyPath);
        }
        private bool _IsLocked { get; set; }
        public void Lock()
        {
            _IsLocked = true;
        }

        private void CheckIsLocked()
        {
            if (_IsLocked)
            {
                throw new Exception("The object is locked and cannot be updated.");
            }
        }
    }

    internal class LMYWFEngineWorkFlow
    {
        private string _WorkFlowName { get; set; }
        private LMYWFEngineTransition[] _Transitions { get; set; }

        public string WorkFlowName { get { return _WorkFlowName; } set { CheckIsLocked(); _WorkFlowName = value; } }
        public LMYWFEngineTransition[] Transitions { get { return _Transitions; } set { CheckIsLocked(); _Transitions = value; } }

        private bool _IsLocked { get; set; }
        public void Lock()
        {
            _IsLocked = true;

            foreach (var transition in Transitions)
            {
                transition.Lock();
            }
        }

        private void CheckIsLocked()
        {
            if (_IsLocked)
            {
                throw new Exception("The object is locked and cannot be updated.");
            }
        }
    }

    internal class LMYWFEngineConfig
    {
        private string _DBConnectionString { get; set; }
        private LMYWFEngineWorkFlow[] _WorkFlows { get; set; }

        public string DBConnectionString { get { return _DBConnectionString; } set { CheckIsLocked(); _DBConnectionString = value; } }
        public LMYWFEngineWorkFlow[] WorkFlows { get { return _WorkFlows; } set { CheckIsLocked(); _WorkFlows = value; } }

        private bool _IsLocked { get; set; }
        public void Lock()
        {
            _IsLocked = true;
            foreach (var workflow in WorkFlows)
            {
                workflow.Lock();
            }
        }

        private void CheckIsLocked()
        {
            if (_IsLocked)
            {
                throw new Exception("The object is locked and cannot be updated.");
            }
        }
    }
}