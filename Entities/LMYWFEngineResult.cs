using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMY.Workflow.Entities
{
    public class LMYWFEngineResult<T>
    {
        public string? Error { get; set; }
        public T MethodResult { get; set; }
    }
}
