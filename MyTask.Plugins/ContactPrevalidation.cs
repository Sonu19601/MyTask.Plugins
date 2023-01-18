using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTask.Plugins
{
    class ContactPrevalidation : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            Entity claim = context.InputParameters["Target"] as Entity;
            double percent = claim.GetAttributeValue<double>(Constants.Claim.Fields.CLAIM_PERCENTAGE);
            if (percent >= 100)
            {
                throw new InvalidPluginExecutionException("Cannot Claim entire policy amount or more");
            }
        }
    }
}
