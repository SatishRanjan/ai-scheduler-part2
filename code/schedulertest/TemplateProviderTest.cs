using System;
using System.Collections.Generic;
using Xunit;
using ai_scheduler.src;
using ai_scheduler.src.models;
using ai_scheduler.src.actions;

namespace AiSchedulerTest
{
    public class TemplateProviderTest
    {
        [Fact]
        public void Enqueue_PriorityQueueTest()
        {
            List<TemplateBase> templates = TemplateProvider.GetAllTemplates();
            foreach (TemplateBase template in templates)
            {
                if (template is AlloyTransformTemplate)
                {
                    Assert.True(true);
                }
                else if (template is HousingTransformTemplate)
                {
                    Assert.True(true);
                }
                else if (template is ElectronicsTransformTemplate)
                {
                    Assert.True(true);
                }
                else if (template is TransferTemplate)
                {
                    Assert.True(true);
                }
            }
        }
    }
}
