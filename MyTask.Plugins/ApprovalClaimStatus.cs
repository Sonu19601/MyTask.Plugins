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
    public class ApprovalClaimStatus : IPlugin
    {
       
        public void Execute(IServiceProvider serviceProvider)
        {
            try {
                IExecutionContext context = (IExecutionContext)serviceProvider.GetService(typeof(IExecutionContext));
                IOrganizationServiceFactory factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                ITracingService Trace = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
                IOrganizationService service = factory.CreateOrganizationService(context.UserId);

                Entity approval = context.InputParameters["Target"] as Entity;
                Entity approvalLoad = service.Retrieve(Approval.ENTITYNAME, approval.Id, new ColumnSet(true));

                EntityReference claim = approvalLoad.GetAttributeValue<EntityReference>(Approval.Fields.CLAIM_ID);


                QueryExpression query = new QueryExpression(Approval.ENTITYNAME);
                query.Criteria.AddCondition(Approval.Fields.CLAIM_ID, ConditionOperator.Equal, claim.Id);
                query.ColumnSet = new ColumnSet(true);
                EntityCollection approvals = service.RetrieveMultiple(query);

                if (approvals.Entities.Count > 0)
                {
                    int count = Methods.ApprovalStatusCount(approvals);
                    Trace.Trace("Accepted and Rejected approvals found");

                    StatePair statePair = Methods.GetApprovalStatus(count, approvals.Entities.Count);
                    Trace.Trace("Status Found Succesfully");

                    Entity parentClaim = new Entity(Claim.ENTITYNAME)
                    {
                        Id = claim.Id,
                        [Claim.Fields.STATUSCODE] = new OptionSetValue(statePair.statusCode),
                        [Claim.Fields.STATECODE] = new OptionSetValue(statePair.stateCode)

                    };
                    service.Update(parentClaim);
                    Trace.Trace("Update Succesfull");
                }
            }
            catch(Exception ex)
            {
                throw new InvalidPluginExecutionException(ex.Message);
            }
            
        }
       
    }
}



