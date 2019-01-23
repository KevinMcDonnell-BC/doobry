using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Doobry.Features.QueryDeveloper
{
    public class ResultMetrics
    {
        public ResultMetrics(FeedResponse<dynamic> result)
        {
            this.id = "Query Metrics";
            this._self = "Query Metrics";
            this.SessionToken = result.SessionToken;
            this.RequestCharge = result.RequestCharge;
            this.Count = result.Count;
            this.CollectionQuota = result.CollectionQuota;
            this.CollectionSizeQuota = result.CollectionSizeQuota;
            this.CollectionSizeUsage = result.CollectionSizeUsage;
            this.CollectionUsage = result.CollectionUsage;
            this.DatabaseQuota = result.DatabaseQuota;
            this.DatabaseUsage = result.DatabaseUsage;
            this.PermissionQuota = result.PermissionQuota;
            this.PermissionUsage = result.PermissionUsage;
            this.StoredProceduresQuota = result.StoredProceduresQuota;
            this.StoredProceduresUsage = result.StoredProceduresUsage;
            this.TriggersQuota = result.TriggersQuota;
            this.TriggersUsage = result.TriggersUsage;
            this.UserDefinedFunctionsQuota = result.UserDefinedFunctionsQuota;
            this.UserDefinedFunctionsUsage = result.UserDefinedFunctionsUsage;
            this.UserQuota = result.UserQuota;
            this.UserUsage = result.UserUsage;
            this.ResponseHeaders = result.ResponseHeaders;
            this.ActivityId = result.ActivityId;
            this.ContentLocation = result.ContentLocation;
            this.CurrentResourceQuotaUsage = result.CurrentResourceQuotaUsage;
            this.MaxResourceQuota = result.MaxResourceQuota;
            this.ResponseContinuation = result.ResponseContinuation;
            this.QueryMetrics = result.QueryMetrics;
        }

        public string id { get; }
        public string _self { get; }
        //
        // Summary:
        //     Gets the session token for use in sesssion consistency reads from the Azure DocumentDB
        //     database service.
        public string SessionToken { get; }
        //
        // Summary:
        //     Gets the continuation token to be used for continuing enumeration of the Azure
        //     DocumentDB database service.
        public string ResponseContinuation { get; }
        //
        // Summary:
        //     Gets the activity ID for the request from the Azure DocumentDB database service.
        public string ActivityId { get; }
        //
        // Summary:
        //     Gets the request charge for this request from the Azure DocumentDB database service.
        public double RequestCharge { get; }
        //
        // Summary:
        //     Gets the current size of this entity from the Azure DocumentDB database service.
        public string CurrentResourceQuotaUsage { get; }
        //
        // Summary:
        //     Gets the maximum size limit for this entity from the Azure DocumentDB database
        //     service.
        public string MaxResourceQuota { get; }
        //
        // Summary:
        //     Gets the number of items returned in the response from the Azure DocumentDB database
        //     service.
        public int Count { get; }
        //
        // Summary:
        //     Gets the current number of user defined functions for a collection from the Azure
        //     DocumentDB database service.
        public long UserDefinedFunctionsUsage { get; }
        //
        // Summary:
        //     Gets the maximum quota of user defined functions for a collection from the Azure
        //     DocumentDB database service.
        public long UserDefinedFunctionsQuota { get; }
        //
        // Summary:
        //     Get the current number of triggers for a collection from the Azure DocumentDB
        //     database service.
        public long TriggersUsage { get; }
        //
        // Summary:
        //     Gets the maximum quota of triggers for a collection from the Azure DocumentDB
        //     database service.
        public long TriggersQuota { get; }
        //
        // Summary:
        //     Gets the current number of stored procedures for a collection from the Azure
        //     DocumentDB database service.
        public long StoredProceduresUsage { get; }
        //
        // Summary:
        //     Gets the maximum quota of stored procedures for a collection from the Azure DocumentDB
        //     database service.
        public long StoredProceduresQuota { get; }
        //
        // Summary:
        //     Gets the current size of a collection in kilobytes from the Azure DocumentDB
        //     database service.
        public long CollectionSizeUsage { get; }
        //
        // Summary:
        //     Gets the maximum size of a collection in kilobytes from the Azure DocumentDB
        //     database service.
        public long CollectionSizeQuota { get; }
        //
        // Summary:
        //     Gets the current number of permission resources within the account from the Azure
        //     DocumentDB database service.
        public long PermissionUsage { get; }
        //
        // Summary:
        //     Gets the maximum quota for permission resources within an account from the Azure
        //     DocumentDB database service.
        public long PermissionQuota { get; }
        //
        // Summary:
        //     Gets the current number of user resources within the account from the Azure DocumentDB
        //     database service.
        public long UserUsage { get; }
        //
        // Summary:
        //     Gets the maximum quota for user resources within an account from the Azure DocumentDB
        //     database service.
        public long UserQuota { get; }
        //
        // Summary:
        //     Gets the current number of collection resources within the account from the Azure
        //     DocumentDB database service.
        public long CollectionUsage { get; }
        //
        // Summary:
        //     Gets the maximum quota for collection resources within an account from the Azure
        //     DocumentDB database service.
        public long CollectionQuota { get; }
        //
        // Summary:
        //     Gets the current number of database resources within the account from the Azure
        //     DocumentDB database service.
        public long DatabaseUsage { get; }
        //
        // Summary:
        //     Gets the maximum quota for database resources within the account from the Azure
        //     DocumentDB database service.
        public long DatabaseQuota { get; }
        //
        // Summary:
        //     Gets the content parent location, for example, dbs/foo/colls/bar, from the Azure
        //     DocumentDB database service.
        public string ContentLocation { get; }
        //
        // Summary:
        //     Gets the response headers from the Azure DocumentDB database service.
        public NameValueCollection ResponseHeaders { get; }

        public IReadOnlyDictionary<string, QueryMetrics> QueryMetrics { get; }

    }
}