using System;
using System.Collections.Generic;
using System.Text;

namespace ai_scheduler.src.actions
{
    public class TransferTemplate : TemplateBase
    {
        public TransferTemplate()
        {
            TemplateName = Operation.TRANSFER.ToString();
            ResourceAndQuantityMapToTransfer = new Dictionary<string, int>();
        }

        public string FromCountry { get; set; }
        public string ToCountry { get; set; }
        public Dictionary<string, int> ResourceAndQuantityMapToTransfer { get; private set; }
    }
}
