using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTask.Plugins
{
    public class ClaimValidation : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            IOrganizationService service = factory.CreateOrganizationService(context.UserId);

            Entity claim = context.InputParameters["Target"] as Entity;
            EntityReference policyRef = claim.GetAttributeValue<EntityReference>(Constants.Claim.Fields.POLICY_ID);
            Entity policy=service.Retrieve(Constants.Policy.ENTITYNAME,policyRef.Id,new Microsoft.Xrm.Sdk.Query.ColumnSet(Constants.Policy.Fields.POlICY_AMOUNT));
            int claimAmount = claim.GetAttributeValue<int>(Constants.Claim.Fields.CLAIM_AMOUNT);
            int policyAmount = policy.GetAttributeValue<int>(Constants.Policy.Fields.POlICY_AMOUNT);

            tracingService.Trace(claimAmount+" "+policyAmount);
            
            
            if (claimAmount>policyAmount)
            {
                throw new InvalidPluginExecutionException("Cannot create Claim with entire policy amount or more");
            }
        }
    }
}
