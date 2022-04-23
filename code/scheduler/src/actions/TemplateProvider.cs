using System;
using System.Collections.Generic;
using System.Text;

namespace ai_scheduler.src.actions
{
    public class TemplateProvider
    {
        public static List<TemplateBase> GetAllTemplates()
        {
            List<TemplateBase> templates = new List<TemplateBase>();
            templates.Add(new AlloyTransformTemplate());
            templates.Add(new ElectronicsTransformTemplate());
            templates.Add(new HousingTransformTemplate());
            templates.Add(new TransferTemplate());
            return templates;
        }

        public static TransformTemplate GetTransformTemplate(string resourceName)
        {
            List<TransformTemplate> transformTemplates = new List<TransformTemplate>();
            transformTemplates.Add(new AlloyTransformTemplate());
            transformTemplates.Add(new ElectronicsTransformTemplate());
            transformTemplates.Add(new HousingTransformTemplate());

            foreach (TransformTemplate template in transformTemplates)
            {
                if (template.OUTPUTS.ContainsKey(resourceName))
                {
                    return template;
                }
            }

            return null;
        }

        public static TransferTemplate GetTransferTemplate()
        {
            return new TransferTemplate();
        }
    }
}
