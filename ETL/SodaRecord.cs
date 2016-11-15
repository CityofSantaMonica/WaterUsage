using System;
using SODA.Models;
using CSM.WaterUsage.Customers.EF;
using CSM.WaterUsage.Geography;
using System.Data.Entity.Spatial;

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

        public static SodaRecord From(
            UsageRecord water,
            Account account,
            AccountService service,
            UsageCategory category,
            StreetSegmentLocator streetSegments,
            CensusBlockLocator censusBlocks)
        {
            string dateFormat = "yyyy-MM-dd";
            string trimmedStreet = account.street.SafeTrim().ToUpper();
            string scrubbedStreet = null;
            string side = null;
            StreetSegment segment = null;
            DbGeometry convertedSegment = null;
            DbGeometry convertedSegmentCentroid = null;
            CensusBlock censusBlock = null;

            if (account.street_number.HasValue)
            {
                int number = (int)account.street_number.Value;
                scrubbedStreet = AddressAnonymizer.Anonymize(number, trimmedStreet);

                segment = streetSegments.Locate(number, trimmedStreet);
                if (segment != null && segment.Shape != null)
                {
                    side = segment.Parity == 0 ? "even" : "odd";
                    convertedSegmentCentroid = ProjectionConverter.Convert(segment.BufferedShape.Centroid);
                    convertedSegment = ProjectionConverter.Convert(segment.Shape);
                }

                censusBlock = censusBlocks.Locate(number, trimmedStreet);
            }

            var usage = new SodaRecord()
            {
                //account
                account_number = water.account_number,
                occupant_code = water.occupant_code,
                debtor_number = account.debtor_number,
                category_code = KeyMaker.ForCategory(category),
                category_description = category.description.SafeTrim(),
                bill_code = water.bill_code == service.bill_code ? water.bill_code.SafeTrim() : String.Empty,
                utility_type = water.utility_type == service.utility_type ? water.utility_type.SafeTrim() : String.Empty,
                start_date = service.start_date.HasValue ? service.start_date.Value.ToString(dateFormat) : null,
                end_date = service.end_date.HasValue ? service.end_date.Value.ToString(dateFormat) : null,

                //usage
                current_read_date = water.prorate_to.HasValue ? water.prorate_to.Value.ToString(dateFormat) : null,
                last_read_date = water.prorate_from.HasValue ? water.prorate_from.Value.ToString(dateFormat) : null,
                usage_hcf = water.usage_billed,
                net = water.net,
                bill_date = water.bill_date.HasValue ? water.bill_date.Value.ToString(dateFormat) : null,
                batch_number = water.batch_number,

                //location
                street_number = account.street_number,
                street_name = trimmedStreet,
                street_scrubbed = scrubbedStreet,
                street_side = side,
                zip_code = account.zip.SafeTrim(),
                census_block_id = (censusBlock == null || censusBlock.Census == null) ? null : censusBlock.Census.GEOID10,
                street_centroid_lat = convertedSegmentCentroid == null ? default(double) : convertedSegmentCentroid.YCoordinate,
                street_centroid_long = convertedSegmentCentroid == null ? default(double) : convertedSegmentCentroid.XCoordinate,
                street_centroid_wkt = convertedSegmentCentroid == null ? null : convertedSegmentCentroid.AsText(),
                street_segment_wkt = convertedSegment == null ? null : convertedSegment.AsText(),
            };

            usage.SetId(water.canrev.ToString());

            return usage;
        }
    }

    public class LatestRecord
    {
        public DateTime current_read_date { get; set; }
    }
}
