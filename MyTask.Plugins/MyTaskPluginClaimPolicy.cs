
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using MyTask.Constants;

namespace MyTask.Plugins
{
    public class MyTaskPluginClaimPolicy : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            ITracingService Trace = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            IOrganizationService service = factory.CreateOrganizationService(context.UserId);
            try
            {
                Entity claim = context.InputParameters["Target"] as Entity;
                var updateClaim = new Entity(Claim.ENTITYNAME, claim.Id);
                EntityReference policyReference = claim.GetAttributeValue<EntityReference>(Claim.Fields.POLICY_ID);

                Entity policy = service.Retrieve(Policy.ENTITYNAME, policyReference.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
                EntityReference customer = policy.GetAttributeValue<EntityReference>(Policy.Fields.CUSTOMER_ID);

                double policyAmount = Convert.ToDouble(policy[Policy.Fields.POlICY_AMOUNT]),
                       claimAmount = Convert.ToDouble(claim[Claim.Fields.CLAIM_AMOUNT]);
                double Percent = (claimAmount / policyAmount) * 100;
               

                    updateClaim[Claim.Fields.CUSTOMER_ID] = customer;
                    updateClaim[Claim.Fields.CLAIM_PERCENTAGE] = Percent;
                    service.Update(updateClaim);

                    ApprovalRecord(context, service, Trace, customer, Percent);
                
                
            }

            catch (Exception e)
            {
                Trace.Trace(e.Message);
            }
        }

        public void ApprovalRecord(IPluginExecutionContext context, IOrganizationService service, ITracingService trace,EntityReference customer,double claimPercentage)
        {
            try
            {
                Entity claim = context.InputParameters["Target"] as Entity;
                //EntityReference customer = claim.GetAttributeValue<EntityReference>(Claim.Fields.CUSTOMER_ID);
                Entity Contact = service.Retrieve(Contacts.ENTITYNAME, customer.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
                trace.Trace("one scrum");

                EntityReference userRefernce = Contact.GetAttributeValue<EntityReference>(Contacts.Fields.SUPERVISING_AGENT);
                Entity user = service.Retrieve(SystemUsers.ENTITYNAME, userRefernce.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
                trace.Trace("two scrum");

                //double claimPercentage = claim.GetAttributeValue<double>(Claim.Fields.CLAIM_PERCENTAGE);
                Entity approval = ApprovalStatus(claim, userRefernce, Approval.STATUS_DRAFT);
                trace.Trace("three scrum");
                trace.Trace(claimPercentage.ToString());

                if (claimPercentage < 25)
                {

                    service.Create(approval);
                }
                else if (claimPercentage >= 25 && claimPercentage < 50)
                {
                    EntityReference managerRef = user.GetAttributeValue<EntityReference>(SystemUsers.Fields.PARENT_USER);
                    Entity approvalManager = ApprovalStatus(claim, managerRef, Approval.STATUS_REVIEW);
                    service.Create(approval);
                    service.Create(approvalManager);

                }
                else
                {
                    EntityReference managerRef = user.GetAttributeValue<EntityReference>(SystemUsers.Fields.PARENT_USER);
                    Entity manager = service.Retrieve(SystemUsers.ENTITYNAME, managerRef.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
                    EntityReference seniorRef = manager.GetAttributeValue<EntityReference>(SystemUsers.Fields.PARENT_USER);

                    Entity approvalManager = ApprovalStatus(claim, managerRef, Approval.STATUS_REVIEW);
                    Entity approvalSenior = ApprovalStatus(claim, seniorRef, Approval.STATUS_REVIEW);

                    trace.Trace("operation begins");
                    service.Create(approval);
                    service.Create(approvalManager);
                    service.Create(approvalSenior);
                    trace.Trace("operation ends");
                }
            }catch(Exception e)
            {
                trace.Trace(e.Message);
            }
        }
        private Entity ApprovalStatus(Entity claim, EntityReference user, int statusCode)
        {
            Entity approvalRecord = new Entity(Approval.ENTITYNAME)
            {
                [Approval.Fields.NAME] = claim.GetAttributeValue<string>(Claim.Fields.NAME),
                [Approval.Fields.ASSIGNED_AGENT] = user,
                [Approval.Fields.CLAIM_ID] = claim.ToEntityReference(),
                [Approval.Fields.STATUSCODE] = new OptionSetValue(statusCode)
            };
            return approvalRecord;
        }
    }
   
}
