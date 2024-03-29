﻿using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyTask.Constants
{
    public class Approval
    {
        public const string ENTITYNAME = "sp_approval";
        public const int STATUS_REJECTED = 778390000;
        public const int STATUS_ACCEPTED = 1;
        public const int STATUS_DRAFT = 778390001;
        public const int STATUS_REVIEW = 778390002;
        public const int STATE_ACTIVE = 0;
        public const int STATE_INACTIVE = 1;


        public class Fields
        {
            public const string CLAIM_ID = "sp_claimid";
            public const string STATUSCODE = "statuscode";
            public const string STATECODE = "statecode";
            public const string NAME = "sp_name";
            public const string ASSIGNED_AGENT = "sp_assignedagent";
        }
    }

    public class Claim
    {
        public const string ENTITYNAME = "sp_claim";
        public const int STATUS_ACCEPTED = 778390001;
        public const int STATUS_REVIEW = 778390000;
        public const int STATUS_REJECTED = 2;
        public const int STATE_ACTIVE = 0;
        public const int STATE_INACTIVE = 1;
        public class Fields
        {
            public const string NAME = "sp_name";
            public const string STATUSCODE = "statuscode";
            public const string STATECODE = "statecode";
            public const string POLICY_ID = "sp_policyid";
            public const string CLAIM_AMOUNT = "sp_claimamount";
            public const string CUSTOMER_ID = "sp_customerid";
            public const string CLAIM_PERCENTAGE = "sp_claimpercentage";
        }
    }
    public class Contacts
    {
        public const string ENTITYNAME = "contact";
        public class Fields
        {
            public const string STATUSCODE = "statuscode";
            public const string SUPERVISING_AGENT = "sp_supervisingagent";

        }
    }
    public class SystemUsers
    {
        public const string ENTITYNAME = "systemuser";
        public static Guid SALES_AGENTS =  new Guid("7e6eaddc-eb63-ed11-9562-000d3ac9bb62");

        public class Fields
        {
            public const string POSITION_ID = "positionid";
            public const string CONTACT_FLAG = "sp_contactflag";
            public const string PARENT_USER = "parentsystemuserid";

        }
    }
    public class Policy
    {
        public const string ENTITYNAME = "sp_policy";
        public class Fields
        {
            public const string CUSTOMER_ID = "sp_customerid";
            public const string POlICY_AMOUNT = "sp_policyamount";
            public const string START_DATE = "sp_startdate";
            public const string END_DATE = "sp_enddate";
            public const string POLICY_INTEREST = "sp_policyinterest";


        }

    }
    public class PolicyMaster
    {
        public class Fields
        {
            public const string START_DATE = "sp_startdate";
            public const string END_DATE = "sp_enddate";
            public const string INTEREST = "sp_interestpercentage";
        }
    }


    public struct StatePair { public int stateCode; public int statusCode; }
    public class Methods
    {

        public static StatePair GetApprovalStatus(int count, int claimsCount)
        {
            StatePair statePair;
            if (count == claimsCount)
            {
                statePair.stateCode = Claim.STATE_ACTIVE;
                statePair.statusCode = Claim.STATUS_ACCEPTED;
            }
            else if (count >= 100)
            {
                statePair.statusCode = Claim.STATUS_REJECTED;
                statePair.stateCode = Claim.STATE_INACTIVE;
            }
            else if (count < 3)
            {
                statePair.statusCode = Claim.STATUS_REVIEW;
                statePair.stateCode = Claim.STATE_ACTIVE;
            }
            else
            {
                throw new NotSupportedException("Not supported.");
            }
            return statePair;
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
