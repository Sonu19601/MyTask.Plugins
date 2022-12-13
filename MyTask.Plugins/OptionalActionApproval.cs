using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using MyTask.Constants;


namespace MyTask.Plugins
{
    public class OptionalActionApproval : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IExecutionContext context = (IExecutionContext)serviceProvider.GetService(typeof(IExecutionContext));
            IOrganizationServiceFactory factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            ITracingService Trace = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            IOrganizationService service = factory.CreateOrganizationService(context.UserId);

            string approvalId = context.InputParameters["approvalId"] as string;
            Trace.Trace("Got Guid");
            int flag = (Int32)context.InputParameters["flag"];
            StatePair statePair,parentState;
            if (flag == 0)
            {
                statePair.stateCode = Approval.STATE_ACTIVE;
                statePair.statusCode = Approval.STATUS_ACCEPTED;               
                StateSetter(service, approvalId, statePair,Trace);

                parentState.stateCode = Approval.STATE_ACTIVE;
                parentState.statusCode = Approval.STATUS_DRAFT;
                ParentApprovalSetter(service, approvalId, parentState);
                context.OutputParameters["Alert"] = "Approval Activated";
            }
            else if (flag == 1)
            {
                statePair.stateCode = Approval.STATE_INACTIVE;
                statePair.statusCode = Approval.STATUS_REJECTED;               
                StateSetter(service, approvalId, statePair,Trace);

                ParentApprovalSetter(service, approvalId, statePair);
                context.OutputParameters["Alert"] = "Approval Rejected";
            }

        }

        private void ParentApprovalSetter(IOrganizationService service, string approvalId, StatePair parentState)
        {
            Entity approval = service.Retrieve(Approval.ENTITYNAME, new Guid(approvalId), new ColumnSet(true));
            EntityReference claim = approval.GetAttributeValue<EntityReference>(Approval.Fields.CLAIM_ID);
            EntityReference userRef = approval.GetAttributeValue<EntityReference>(Approval.Fields.ASSIGNED_AGENT);
            Entity user = service.Retrieve(SystemUsers.ENTITYNAME, userRef.Id, new ColumnSet(true));
            EntityReference parentUser = user.GetAttributeValue<EntityReference>(SystemUsers.Fields.PARENT_USER);

            QueryExpression query = new QueryExpression(Approval.ENTITYNAME);
            query.Criteria.AddCondition(Approval.Fields.CLAIM_ID, ConditionOperator.Equal, claim.Id);
            if (parentState.stateCode == Approval.STATE_ACTIVE)
            {
                query.Criteria.AddCondition(Approval.Fields.ASSIGNED_AGENT, ConditionOperator.Equal, parentUser.Id);
            }
            EntityCollection approvals = service.RetrieveMultiple(query);
            foreach(var record in approvals.Entities)
            {
                record[Approval.Fields.STATECODE] = new OptionSetValue(parentState.stateCode);
                record[Approval.Fields.STATUSCODE] = new OptionSetValue(parentState.statusCode);
                service.Update(record);

            }
        }

        private  void StateSetter(IOrganizationService service, string approvalId, StatePair state,ITracingService trace)
        {
            Guid id = new Guid(approvalId);
            Entity approval = service.Retrieve(Approval.ENTITYNAME, id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
            approval[Approval.Fields.STATUSCODE] = new OptionSetValue(state.statusCode);
            approval[Approval.Fields.STATECODE] = new OptionSetValue(state.stateCode);
            service.Update(approval);
            trace.Trace("Approval Updated");
            //Calling Claim update function
            ClaimUpdate(approval, service,trace);
            
        }

        private  void ClaimUpdate(Entity approval, IOrganizationService service,ITracingService Trace)
        {
            EntityReference claim = approval.GetAttributeValue<EntityReference>(Approval.Fields.CLAIM_ID);
            Trace.Trace("Approval for Claim update fetched");

            QueryExpression query = new QueryExpression(Approval.ENTITYNAME);
            query.Criteria.AddCondition(Approval.Fields.CLAIM_ID, ConditionOperator.Equal, claim.Id);
            query.ColumnSet = new ColumnSet(true);
            EntityCollection approvals = service.RetrieveMultiple(query);
            Trace.Trace("All Related Approvals found");

            if (approvals.Entities.Count > 0)
            {
                int count =Methods.ApprovalStatusCount(approvals);
                StatePair statePair = Methods.GetApprovalStatus(count, approvals.Entities.Count);
                Entity parentClaim = new Entity(Claim.ENTITYNAME)
                {
                    Id = claim.Id,
                    [Claim.Fields.STATUSCODE] = new OptionSetValue(statePair.statusCode),
                    [Claim.Fields.STATECODE] = new OptionSetValue(statePair.stateCode)

                };
                Trace.Trace("Updating Approval realtion");
                service.Update(parentClaim);
                
                Trace.Trace(" Approval complete");
            }
        }       
    }
   }


