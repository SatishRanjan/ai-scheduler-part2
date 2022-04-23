using System;
using System.Collections.Generic;
using System.Text;

namespace ai_scheduler.src.actions
{
    public class ElectronicsTransformTemplate: TransformTemplate
    {
        public ElectronicsTransformTemplate()
        {
            INPUTS.Add("Population", 1);
            INPUTS.Add("MetallicElements", 3);
            INPUTS.Add("MetallicAlloys", 2);

            OUTPUTS.Add("Population", 1);
            OUTPUTS.Add("Electronics", 2);
            OUTPUTS.Add("ElectronicsWaste", 1);
        }
    }
}
