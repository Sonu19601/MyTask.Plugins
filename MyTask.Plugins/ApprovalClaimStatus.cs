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
                    int count = ApprovalStatusCount(approvals);
                    Trace.Trace("Accepted and Rejected approvals found");

                    StatePair statePair = GetApprovalStatus(count, approvals.Entities.Count);
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
        public static  StatePair GetApprovalStatus(int count, int claimsCount)
        {
            StatePair temp;
            if (count == claimsCount)
            {
                temp.stateCode = Claim.STATE_ACTIVE;
                temp.statusCode = Claim.STATUS_ACCEPTED;
                return temp;
            }
            else if (count >= 100 )
            {
                temp.statusCode= Claim.STATUS_REJECTED;
                temp.stateCode = Claim.STATE_INACTIVE;
                return temp;
            }
            else if (count < 3)
            {
                temp.statusCode= Claim.STATUS_REVIEW;
                temp.stateCode = Claim.STATE_ACTIVE;
                return temp;
            }
            throw new NotSupportedException("Not supported.");
        }
        
        public static int ApprovalStatusCount(EntityCollection approvals)
        {
            //var rejected = claims.Entities.Any(x => x.GetAttributeValue<OptionSetValue>("statuscode").Value==778390000);
            int result = 0;
            foreach (var v in approvals.Entities)
            {
                if (((OptionSetValue)v.Attributes[Approval.Fields.STATUSCODE]).Value == Approval.STATUS_ACCEPTED)
                {
                    result++;
                }
                else if (((OptionSetValue)v.Attributes[Approval.Fields.STATUSCODE]).Value == Approval.STATUS_REJECTED)
                {
                    result = 100;
                }
            }
            return result;
        }
    }
}



