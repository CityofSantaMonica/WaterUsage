using SODA.Models;
using System;

namespace CSM.WaterUsage.ETL
{
    public class SodaRecord
    {
        //account
        public int account_number { get; set; }
        public short occupant_code { get; set; }
        public int? debtor_number { get; set; }
        public string category_code { get; set; }
        public string category_description { get; set; }
        public string bill_code { get; set; }
        public string utility_type { get; set; }
        public string start_date { get; set; }
        public string end_date { get; set; }

        //usage
        public string current_read_date { get; set; }
        public string last_read_date { get; set; }
        public decimal? usage_hcf { get; set; }
        public decimal? usage_gallons
        {
            get
            {
                return usage_hcf.HasValue ? usage_hcf.Value * 748 : default(decimal?);
            }
        }
        public decimal? net { get; set; }
        public string bill_date { get; set; }
        public int batch_number { get; set; }

        //location
        public decimal? street_number { get; set; }
        public string street_name { get; set; }
        public string street_scrubbed { get; set; }
        public string street_side { get; set; }
        public string zip_code { get; set; }
        public string census_block_id { get; set; }
        public double? street_centroid_lat { get; set; }
        public double? street_centroid_long { get; set; }
        public string street_centroid_wkt { get; set; }
        public LocationColumn street_centroid
        {
            get
            {
                if (street_centroid_lat.HasValue
                 && street_centroid_lat.Value > 34
                 && street_centroid_long.HasValue
                 && street_centroid_long.Value < -118)
                {
                    return new LocationColumn { Latitude = street_centroid_lat.ToString(), Longitude = street_centroid_long.ToString(), NeedsRecoding = false };
                }

                return new LocationColumn();
            }
        }
        public string street_segment_wkt { get; set; }

        //unique id
        public string id { get; private set; }
        public void SetId(string canrev)
        {
            id = String.Format("{0}{1}{2}{3}{4}", account_number, occupant_code, batch_number, canrev, current_read_date);
        }
    }

    public class LatestRecord
    {
        public DateTime current_read_date { get; set; }
    }
}
