using System;

namespace CSM.WaterUsage.Customers
{
    public interface IAccount
    {
        int account_number { get; set; }
        short occupant_code { get; set; }
        int? debtor_number { get; set; }
        decimal? street_number { get; set; }
        string street { get; set; }
        string unit { get; set; }
        string region { get; set; }
        string city { get; set; }
        string province { get; set; }
        string zip { get; set; }
    }

    public interface IAccountService
    {
        int account_number { get; set; }
        short occupant_code { get; set; }
        string utility_type { get; set; }
        string bill_code { get; set; }
        DateTime? start_date { get; set; }
        DateTime? end_date { get; set; }
        string category_code { get; set; }
    }

    public interface IUsageCategory
    {
        string code { get; set; }
        string description { get; set; }
        string flag { get; set; }
    }

    public interface IUsageRecord
    {
        int account_number { get; set; }
        short occupant_code { get; set; }
        DateTime? bill_date { get; set; }
        decimal? net { get; set; }
        DateTime? prorate_from { get; set; }
        DateTime? prorate_to { get; set; }
        decimal? usage_billed { get; set; }
        int batch_number { get; set; }
        string utility_type { get; set; }
        string bill_code { get; set; }
        short canrev { get; set; }
    }
}
