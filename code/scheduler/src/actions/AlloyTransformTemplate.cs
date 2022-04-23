using System;
using System.Collections.Generic;
using System.Text;

namespace ai_scheduler.src.actions
{
    public class AlloyTransformTemplate: TransformTemplate
    {
        public AlloyTransformTemplate()
        {
            INPUTS.Add("Population", 1);
            INPUTS.Add("MetallicElements", 2);

            OUTPUTS.Add("Population", 1);
            OUTPUTS.Add("MetallicAlloys", 1);
            OUTPUTS.Add("MetallicAlloysWaste", 1);
        }
    }
}
