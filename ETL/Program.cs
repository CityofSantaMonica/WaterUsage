using CSM.WaterUsage.Customers;
using CSM.WaterUsage.Geography;
using Newtonsoft.Json;
using SODA;
using SODA.Utilities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;

namespace CSM.WaterUsage.ETL
{
    class Program
    {
        static readonly string dataFile = "data.csv";
        static readonly string errorFile = "error.json";
        static readonly string exceptionFile = "exceptions.txt";
        static readonly string batchFile = "batch.json";
        static readonly string batchRetryFile = "batchRetry.json";
        static readonly string logFile = "log.txt";
        static readonly string failedFile = "failed.json";

        static readonly string dateFormat = "yyyy-MM-dd";

        static SimpleFileLogger logger;

        static Resource<SodaRecord> waterUsageDataSet;
        static ICustomersService customers;
        static IGeographyService geography;

        static int batchSize = 1;

        static void Main(string[] args)
        {
            using (logger = new SimpleFileLogger(logFile))
            {
                try
                {
                    logger.WriteLine("TransferWaterUsage started");

                    if (args.Length > 0 && args.Any(a => a.StartsWith("/b")))
                        batchSize = Int32.Parse(args.First(a => a.StartsWith("/b")).Split('=')[1]);

                    logger.WriteLine("Batch size {0}", batchSize);

                    waterUsageDataSet = initializeWaterUsageDataset();

                    logger.WriteLine("Connected to dataset: {0} with ID {1}", waterUsageDataSet.Metadata.Name, waterUsageDataSet.Metadata.Identifier);

                    customers = new CustomersService();
                    geography = new GeographyService();

                    if (args.Length > 0 && args.Any(a => a.StartsWith("/start")))
                    {
                        var startDate = DateTime.Parse(args.First(a => a.StartsWith("/start")).Split('=')[1]);
                        upsertSince(startDate);
                    }
                    else
                    {
                        upsertLatest();
                    }
                }
                catch (Exception ex)
                {
                    logger.Write(ex);
                }
            }
        }

        static Resource<SodaRecord> initializeWaterUsageDataset()
        {
            string host = ConfigurationManager.AppSettings["SocrataHost"];
            string appToken = ConfigurationManager.AppSettings["SocrataAppToken"];
            string username = ConfigurationManager.AppSettings["SocrataUsername"];
            string password = ConfigurationManager.AppSettings["SocrataPassword"];
            string resourceId = ConfigurationManager.AppSettings["SocrataResourceId"];

            var soda = new SodaClient(host, appToken, username, password);
            soda.RequestTimeout = 300000;

            return soda.GetResource<SodaRecord>(resourceId);
        }

        static void cleanupFiles()
        {
            if (File.Exists(dataFile))
                File.Delete(dataFile);

            if (File.Exists(errorFile))
                File.Delete(errorFile);

            if (File.Exists(exceptionFile))
                File.Delete(exceptionFile);

            if (File.Exists(batchFile))
                File.Delete(batchFile);
        }

        static void upsertAll()
        {
            logger.WriteLine("Upserting all records...");

            var usageRecords = customers.GetUsageRecords();

            upsertInBatches(usageRecords);
        }

        static void upsertLatest()
        {
            logger.WriteLine("Upserting latest records...");

            var soql = new SoqlQuery().Select("current_read_date").Order(SoqlOrderDirection.DESC, "current_read_date").Limit(1);
            var latest = waterUsageDataSet.Query<LatestRecord>(soql).SingleOrDefault();
            var latestDate = latest == null ? DateTime.Now.AddDays(-7) : latest.current_read_date;

            upsertSince(latestDate);
        }

        static void upsertSince(DateTime startDate)
        {
            logger.WriteLine("Upsert starting from {0:yyyy-MM-dd}", startDate);

            var usageRecords = customers.GetUsageRecords(startDate);

            logger.WriteLine("Found {0} records to upsert", usageRecords.Count());
            logger.WriteLine("Approximate # batches: {0}", usageRecords.Count() / batchSize);

            upsertInBatches(usageRecords, true);
        }

        static void upsertFailures(IList<SodaResult> failedUpserts = null)
        {
            var failedUpsertRecords = new List<SodaRecord>();

            if (failedUpserts == null)
            {
                logger.WriteLine("Initial call: {0}", DateTime.Now);
                foreach (string line in File.ReadLines(batchRetryFile))
                {
                    var result = JsonConvert.DeserializeObject<SodaResult>(line);
                    if (result.IsError)
                    {
                        var data = (IEnumerable<SodaRecord>)JsonConvert.DeserializeObject<IEnumerable<SodaRecord>>(result.Data);
                        var validData = data.Where(u => !String.IsNullOrEmpty(u.id));
                        logger.WriteLine("Found {0} failed ({1} valid) upserts", data.Count(), validData.Count());
                        failedUpsertRecords.AddRange(validData);
                    }
                }
            }
            else
            {
                logger.WriteLine("Recursive call: {0}", DateTime.Now);
                failedUpsertRecords = failedUpserts.SelectMany(result => (IEnumerable<SodaRecord>)JsonConvert.DeserializeObject<IEnumerable<SodaRecord>>(result.Data))
                                                   .Where(usage => !String.IsNullOrEmpty(usage.id))
                                                   .ToList();
            }

            logger.WriteLine("Upserting {0} records in batches of size {1}", failedUpsertRecords.Count, batchSize);

            failedUpserts = new List<SodaResult>();

            foreach (var result in waterUsageDataSet.BatchUpsert(failedUpsertRecords, batchSize))
            {
                string json = JsonConvert.SerializeObject(result);

                if (result.IsError)
                {
                    failedUpserts.Add(result);
                    logger.WriteLine("Failed: {0}", json);
                    File.AppendAllLines(failedFile, new[] { json });
                }
                else
                {
                    logger.WriteLine("Success: {0}", json);
                }
            }

            if (failedUpserts.Any())
                upsertFailures(failedUpserts);
        }

        static void upsertInBatches(IEnumerable<IUsageRecord> water_usages, bool writeLogFiles = false)
        {
            if (writeLogFiles)
                cleanupFiles();

            int batchNumber = 0;
            int i = 0;
            bool firstRecord = true;

            while (true)
            {
                var batch = new List<SodaRecord>();

                foreach (var usage in water_usages.Skip(batchNumber * batchSize).Take(batchSize))
                {
                    var usage_record = makeSodaRecord(usage);

                    if (usage_record != null)
                    {
                        batch.Add(usage_record);
                        string log = String.Format("Added record {0}", i++);

                        logger.WriteLine(log);

                        if (writeLogFiles)
                        {
                            File.AppendAllLines(dataFile, new[] { SeparatedValuesSerializer.SerializeToString(new[] { usage_record }, SeparatedValuesDelimiter.Comma, firstRecord) });
                        }
                        
                        firstRecord = false;
                    }
                }

                if (batch.Count == 0)
                {
                    break;
                }
                else
                {
                    logger.WriteLine("Sending batch number {0} with size {1}", batchNumber, batch.Count);
                    foreach (var result in waterUsageDataSet.BatchUpsert(batch, batchSize))
                    {
                        logSodaResult(result, writeLogFiles: writeLogFiles);
                    }
                    batchNumber++;
                }
            }
        }

        static SodaRecord makeSodaRecord(IUsageRecord usage)
        {
            SodaRecord soda = null;

            try
            {
                var account = customers.GetAccount(usage);
                var service = customers.GetAccountService(usage);
                var category = customers.GetUsageCategory(service);

                string scrubbedAddress = null;
                string censusBlockId = null;
                StreetSegment streetSegment = null;

                string trimmedStreet = account.street.SafeTrim().ToUpper();

                if (account.street_number.HasValue)
                {
                    int number = (int)account.street_number.Value;
                    scrubbedAddress = geography.AnonymizeAddress(number, trimmedStreet);
                    censusBlockId = geography.GetCensusBlockId(number, trimmedStreet);
                    streetSegment = geography.GetStreetSegment(number, trimmedStreet);
                }

                soda = new SodaRecord();
                
                //account
                soda.account_number = usage.account_number;
                soda.occupant_code = usage.occupant_code;
                soda.debtor_number = account.debtor_number;
                soda.category_code = category.code;
                soda.category_description = category.description.SafeTrim();
                soda.bill_code = usage.bill_code == service.bill_code ? usage.bill_code.SafeTrim() : String.Empty;
                soda.utility_type = usage.utility_type == service.utility_type ? usage.utility_type.SafeTrim() : String.Empty;
                soda.start_date = service.start_date.HasValue ? service.start_date.Value.ToString(dateFormat) : null;
                soda.end_date = service.end_date.HasValue ? service.end_date.Value.ToString(dateFormat) : null;

                //usage
                soda.current_read_date = usage.prorate_to.HasValue ? usage.prorate_to.Value.ToString(dateFormat) : null;
                soda.last_read_date = usage.prorate_from.HasValue ? usage.prorate_from.Value.ToString(dateFormat) : null;
                soda.usage_hcf = usage.usage_billed;
                soda.net = usage.net;
                soda.bill_date = usage.bill_date.HasValue ? usage.bill_date.Value.ToString(dateFormat) : null;
                soda.batch_number = usage.batch_number;

                //location
                soda.street_number = account.street_number;
                soda.street_name = trimmedStreet;
                soda.street_scrubbed = scrubbedAddress;
                soda.zip_code = account.zip.SafeTrim();
                soda.census_block_id = censusBlockId;

                if (streetSegment != null)
                {
                    soda.street_side = streetSegment.Side;

                    if (streetSegment.Centroid != null)
                    {
                        soda.street_centroid_lat = streetSegment.Centroid.YCoordinate;
                        soda.street_centroid_long = streetSegment.Centroid.XCoordinate;
                        soda.street_centroid_wkt = streetSegment.Centroid.AsText();
                    }
                    if (streetSegment.Shape != null)
                    {
                        soda.street_segment_wkt = streetSegment.Shape.AsText();
                    }
                }

                soda.SetId(usage.canrev.ToString());
            }
            catch (Exception ex)
            {
                File.AppendAllLines(exceptionFile, new[] { ex.ToString() });
                logger.Write(ex);
                soda = null;
            }

            return soda;
        }

        static void logSodaResult(SodaResult result, string file = null, bool writeLogFiles = false)
        {
            file = file ?? batchFile;

            var json = JsonConvert.SerializeObject(result);
            logger.WriteLine("Got response: {0}", json);

            if (writeLogFiles)
            {
                File.AppendAllLines(file, new[] { String.Format("{0}", json) });
            }
        }
    }
}
