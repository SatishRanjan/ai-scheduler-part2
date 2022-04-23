using System;
using System.Collections.Generic;
using System.Text;

namespace ai_scheduler.src.models
{
    /// <summary>
    /// Represents the list of virtual world generated after applying the transform and transfer templates
    /// </summary>
    public class Successors
    {
        public List<VirtualWorld> SuccessorVirtualWorlds { get; set; }
    }
}
