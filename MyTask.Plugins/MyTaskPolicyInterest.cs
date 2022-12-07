using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk.Query;
using MyTask.Constants;
namespace MyTask.Plugins
{
    public class MyTaskPolicyInterest : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            ITracingService Trace = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            IOrganizationService service = factory.CreateOrganizationService(context.UserId);
            try
            {
                Trace.Trace("Execution Start");
                Entity policy = context.InputParameters["Target"] as Entity;
                DateTime DT1 = policy.GetAttributeValue<DateTime>(Policy.Fields.START_DATE);
                DateTime DT2 = policy.GetAttributeValue<DateTime>(Policy.Fields.END_DATE);
                int Year = DT2.Year - DT1.Year;

                string FetchXml = @"<?xml version='1.0'?>
                                   <fetch no-lock='false' distinct='true'  output-format='xml-platform' version='1.0'>
                                           <entity name='sp_policymaster'/>
                                   </fetch >";
                EntityCollection policyMasters = service.RetrieveMultiple(new FetchExpression(FetchXml));
                foreach (var policyMaster in policyMasters.Entities)
                {
                    if (Year >= policyMaster.GetAttributeValue<int>(PolicyMaster.Fields.START_DATE) && Year <= policyMaster.GetAttributeValue<int>(PolicyMaster.Fields.END_DATE))
                    {
                        policy[Policy.Fields.POLICY_INTEREST] = policyMaster.GetAttributeValue<int>(PolicyMaster.Fields.INTEREST);
                        service.Update(policy);
                        break;
                    }

                }
                Trace.Trace("Execution Done");
            }
            catch(Exception e)
            {
                Trace.Trace(e.Message);
            }

        }
    }
}
