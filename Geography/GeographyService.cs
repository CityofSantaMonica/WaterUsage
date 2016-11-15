using CSM.WaterUsage.Geography.EF;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CSM.WaterUsage.Geography
{
    public interface IGeographyService
    {
        string AnonymizeAddress(decimal? number, string street);
        string GetCensusBlockId(int number, string street);
        StreetSegment GetStreetSegment(int number, string street);
    }

    public class GeographyService : IGeographyService
    {
        private readonly GeographyEntities entities;

        private IEnumerable<EF.CensusBlocks> allCensusBlocks;
        private IEnumerable<EF.Centerlines> allCenterlines;
        private IEnumerable<EF.ParcelCentroids> allParcelCentroids;
        private Dictionary<string, IEnumerable<StreetSegment>> segmentsMap;

        public GeographyService()
        {
            entities = new GeographyEntities();
            entities.Database.CommandTimeout = 300;

            allCensusBlocks = entities.CensusBlocks.ToArray();
            allCenterlines = entities.Centerlines.ToArray();
            allParcelCentroids = entities.ParcelCentroids.ToArray();
            
            segmentsMap = new Dictionary<string, IEnumerable<StreetSegment>>();
        }

        public string AnonymizeAddress(decimal? number, string street)
        {
            if (!number.HasValue)
            {
                return street;
            }

            int intNumber = (int)number.Value;

            string format = "{0}BLK {1}";
            int result = (int)(Math.Round(number.Value / 100M, MidpointRounding.ToEven) * 100);

            if (result > intNumber)
                result -= 100;

            return String.Format(format, result, street);
        }

        public string GetCensusBlockId(int number, string street)
        {
            var parcel = findParcelCentroid(number, street);
            var censusBlock = findCensusBlock(parcel);

            if (censusBlock != null)
                return censusBlock.GEOID10;

            return String.Empty;
        }

        public StreetSegment GetStreetSegment(int number, string street)
        {
            var parity = getParity(number);
            var possibleSegments = getPossibleSegments(street, parity);
            return possibleSegments.FirstOrDefault(s => s.Start <= number && s.End >= number);
        }

        private IParcelCentroid findParcelCentroid(int number, string street)
        {
            return allParcelCentroids.Where(p => p.SitusStree.Equals(street, StringComparison.OrdinalIgnoreCase))
                                     .FirstOrDefault(p => p.SitusHouse.Equals(number.ToString(), StringComparison.OrdinalIgnoreCase))
                                     as IParcelCentroid;
        }

        private ICensusBlock findCensusBlock(IParcelCentroid parcel)
        {
            if (parcel == null || parcel.Shape == null)
                return null;

            foreach (var block in allCensusBlocks)
            {
                if (block.Shape.Contains(parcel.Shape))
                {
                    return block as ICensusBlock;
                }
            }

            return null;
        }

        private int getParity(decimal input)
        {
            return Convert.ToInt32(input) % 2;
        }

        private IEnumerable<StreetSegment> getPossibleSegments(string street, int? parity = null)
        {
            if (!segmentsMap.ContainsKey(street))
            {
                var possibleSegments =
                    allCenterlines.Where(c => c.FULLNAME.Equals(street, StringComparison.OrdinalIgnoreCase))
                                  .SelectMany(validRanges)
                                  .OrderBy(s => s.Start).ThenBy(s => s.End)
                                  .ToArray();

                segmentsMap.Add(street, possibleSegments);
            }

            var segments = segmentsMap[street];

            if (parity.HasValue)
                segments = segments.Where(s => s.Parity == parity.Value);

            return segments;
        }

        private IEnumerable<StreetSegment> validRanges(EF.Centerlines centerline)
        {
            var leftSegment = new StreetSegment()
            {
                Name = centerline.FULLNAME,
                Start = centerline.ADLF.HasValue ? centerline.ADLF.Value : 0,
                End = centerline.ADLT.HasValue ? centerline.ADLT.Value : 0,
                Shape = centerline.Shape
            };

            if (startsAndEnds(leftSegment) && equalParity(leftSegment.Start, leftSegment.End))
            {
                leftSegment.Parity = getParity(leftSegment.Start);
                leftSegment.Centroid = ProjectionConverter.Convert(leftSegment.Shape.Buffer(1).Centroid);
                leftSegment.Shape = ProjectionConverter.Convert(leftSegment.Shape);
                yield return leftSegment;
            }

            var rightSegment = new StreetSegment()
            {
                Name = centerline.FULLNAME,
                Start = centerline.ADRF.HasValue ? centerline.ADRF.Value : 0,
                End = centerline.ADRT.HasValue ? centerline.ADRT.Value : 0,
                Shape = centerline.Shape
            };

            if (startsAndEnds(rightSegment) && equalParity(rightSegment.Start, rightSegment.End))
            {
                rightSegment.Parity = getParity(rightSegment.Start);
                rightSegment.Centroid = ProjectionConverter.Convert(rightSegment.Shape.Buffer(1).Centroid);
                rightSegment.Shape = ProjectionConverter.Convert(rightSegment.Shape);
                yield return rightSegment;
            }
        }

        private bool startsAndEnds(StreetSegment segment)
        {
            return !(segment.Start == 0 && segment.End == 0);
        }

        private bool equalParity(decimal left, decimal right)
        {
            return getParity(left) == getParity(right);
        }
    }
}
