using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ai_scheduler.src.actions
{
    public class TransformTemplate : TemplateBase
    {
        public Dictionary<string, int> INPUTS { get; set; }
        public Dictionary<string, int> OUTPUTS { get; set; }
        public string CountryName { get; set; }

        public TransformTemplate()
        {
            INPUTS = new Dictionary<string, int>();
            OUTPUTS = new Dictionary<string, int>();
            TemplateName = Operation.TRANSFORM.ToString();
        }

        public TransformTemplate IncreaseYield(int numberOfTimes)
        {
            if (numberOfTimes <= 0 || (INPUTS == null || INPUTS.Count == 0) || (OUTPUTS == null || OUTPUTS.Count == 0))
            {
                return null;
            }

            // Increase the INPUTS and OUTPUTS with the numberOfTimes
            for (int i = 0; i < INPUTS.Count; ++i)
            {
                KeyValuePair<string, int> item = INPUTS.ElementAt(i);
                INPUTS[item.Key] = item.Value * numberOfTimes;
            }

            // Increase the INPUTS and OUTPUTS with the numberOfTimes
            for (int i = 0; i < OUTPUTS.Count; ++i)
            {
                KeyValuePair<string, int> item = OUTPUTS.ElementAt(i);
                OUTPUTS[item.Key] = item.Value * numberOfTimes;
            }

            return this;
        }
    }
}
