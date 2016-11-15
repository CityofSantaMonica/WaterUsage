using CSM.WaterUsage.Geography.EF;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CSM.WaterUsage.Geography
{
    public class StreetSegmentLocator
    {
        private readonly Centerlines[] allCenterlines;
        private Dictionary<string, IEnumerable<StreetSegment>> segmentsMap;

        public StreetSegmentLocator()
        {
            allCenterlines = new GeographyEntities().Centerlines.ToArray();
            segmentsMap = new Dictionary<string, IEnumerable<StreetSegment>>();
        }

        public StreetSegment Locate(int number, string street)
        {
            var parity = getParity(number);
            var possibleSegments = getPossibleSegments(street, parity);
            return possibleSegments.FirstOrDefault(s => s.Start <= number && s.End >= number);
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

            if(parity.HasValue)
                segments = segments.Where(s => s.Parity == parity.Value);

            return segments;
        }

        private IEnumerable<StreetSegment> validRanges(Centerlines centerline)
        {
            var leftSegment = new StreetSegment() {
                Name = centerline.FULLNAME,
                Start = centerline.ADLF.HasValue ? centerline.ADLF.Value : 0,
                End = centerline.ADLT.HasValue ? centerline.ADLT.Value : 0,
                Shape = centerline.Shape
            };

            if (startsAndEnds(leftSegment) && equalParity(leftSegment.Start, leftSegment.End))
            {
                leftSegment.Parity = getParity(leftSegment.Start);
                leftSegment.BufferedShape = leftSegment.Shape.Buffer(1);
                yield return leftSegment;
            }

            var rightSegment = new StreetSegment() {
                Name = centerline.FULLNAME,
                Start = centerline.ADRF.HasValue ? centerline.ADRF.Value : 0,
                End = centerline.ADRT.HasValue ? centerline.ADRT.Value : 0,
                Shape = centerline.Shape
            };

            if (startsAndEnds(rightSegment) && equalParity(rightSegment.Start, rightSegment.End))
            {
                rightSegment.Parity = getParity(rightSegment.Start);
                rightSegment.BufferedShape = rightSegment.Shape.Buffer(1);
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
