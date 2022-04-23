using ai_scheduler.src.models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace ai_scheduler.src.actions
{
    public class OperationProvider
    {
        private UtilityCalculator utilityCalculator = new UtilityCalculator();
        public TemplateBase GetOperation(VirtualWorld virtualWorld)
        {
            TemplateBase operation = null;
            VirtualCountry self = virtualWorld.VirtualCountries.Where(c => c.IsSelf).FirstOrDefault();
            if (self == null)
            {
                return operation;
            }

            // Check if metallic alloy is zero then return the AlloyTransformOperator


            return operation;
        }
    }
}
