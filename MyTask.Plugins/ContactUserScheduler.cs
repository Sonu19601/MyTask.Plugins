using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyTask.Constants;

namespace MyTask.Plugins
{
    public class ContactUserScheduler : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            ITracingService Trace = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            IOrganizationService service = factory.CreateOrganizationService(context.UserId);
            Trace.Trace("execution start ");

            Entity contact = context.InputParameters["Target"] as Entity;
            if (((OptionSetValue)contact.Attributes[Contacts.Fields.STATUSCODE]).Value==1)
            {
                QueryExpression agentsQuery = new QueryExpression(SystemUsers.ENTITYNAME);
                agentsQuery.Criteria.AddCondition(SystemUsers.Fields.POSITION_ID, ConditionOperator.Equal, SystemUsers.SALES_AGENTS);
                agentsQuery.ColumnSet = new ColumnSet(true);

                Trace.Trace("Retrieving all sales Agents");
                EntityCollection activeContacts = service.RetrieveMultiple(agentsQuery);
                Entity referenceAgent = activeContacts.Entities[0];
                try
                {

                    if (referenceAgent.GetAttributeValue<int>(SystemUsers.Fields.CONTACT_FLAG).Equals(null))
                    {
                        contact[Contacts.Fields.SUPERVISING_AGENT] = referenceAgent.ToEntityReference();
                        service.Update(contact);
                        referenceAgent[SystemUsers.Fields.CONTACT_FLAG] = 1;
                        service.Update(referenceAgent);
                    }
                    else
                    {
                        Trace.Trace("Iterating for least assigned agent");
                        foreach (var agent in activeContacts.Entities)
                        {
                            if ((referenceAgent.GetAttributeValue<int>(SystemUsers.Fields.CONTACT_FLAG) > agent.GetAttributeValue<int>(SystemUsers.Fields.CONTACT_FLAG)) || (agent.GetAttributeValue<int>(SystemUsers.Fields.CONTACT_FLAG).Equals(null)))
                            {
                                referenceAgent = service.Retrieve(SystemUsers.ENTITYNAME, agent.Id, new ColumnSet(true));
                            }
                        }
                        Trace.Trace("Least assigned agent found ");

                        Entity contactUpdate = new Entity
                        {
                            LogicalName = contact.LogicalName,
                            Id = contact.Id
                        };
                        contactUpdate[Contacts.Fields.SUPERVISING_AGENT] = referenceAgent.ToEntityReference();
                        service.Update(contactUpdate);
                        //Contact["sp_supervisingagent"] = Ragent.ToEntityReference();
                        //service.Update(Contact);
                        Trace.Trace("Contact assigned with agent");

                        referenceAgent[SystemUsers.Fields.CONTACT_FLAG] = referenceAgent.GetAttributeValue<int>(SystemUsers.Fields.CONTACT_FLAG) + 1;
                        service.Update(referenceAgent);
                        Trace.Trace("Agent Flag increased");

                    }
                }
                catch (Exception e)
                {
                    Trace.Trace(e.Message);
                }
            }
            else
            {
                Trace.Trace(contact.GetAttributeValue<OptionSetValue>("statuscode").Value.ToString());
            }

        }
    }
}
