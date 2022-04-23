using System;
using System.Collections.Generic;
using System.Text;

namespace ai_scheduler.src.actions
{
    public class HousingTransformTemplate: TransformTemplate
    {
        public HousingTransformTemplate()
        {
            // Set the INPUTS and OUTPUTS from the project doc
            INPUTS.Add("Population", 5);
            INPUTS.Add("MetallicElements", 1);
            INPUTS.Add("Timber", 5);
            INPUTS.Add("MetallicAlloys", 3);
            
            OUTPUTS.Add("Housing", 1);
            OUTPUTS.Add("HousingWaste", 1);
            OUTPUTS.Add("Population", 5);
        }
    }
}
