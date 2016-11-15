using CSM.WaterUsage.Geography.EF;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CSM.WaterUsage.Geography
{
    public class CensusBlockLocator
    {
        private readonly IEnumerable<CensusBlocks> allCensusBlocks;
        private readonly IEnumerable<ParcelCentroids> allParcelCentroids;

        public CensusBlockLocator()
        {
            var geography = new GeographyEntities();
            allCensusBlocks = geography.CensusBlocks.ToArray();
            allParcelCentroids = geography.ParcelCentroids.ToArray();
        }

        /// <summary>
        /// Find the census block and parcel containing this address
        /// </summary>
        public CensusBlock Locate(int number, string street)
        {
            var parcel = findParcelCentroid(number, street);
            var censusBlock = findCensusBlock(parcel);

            return new CensusBlock {
                Parcel = parcel,
                Census = censusBlock
            };
        }

        /// <summary>
        /// For each parcel on this street, find the first with a matching number
        /// </summary>
        private ParcelCentroids findParcelCentroid(int number, string street)
        {
            return allParcelCentroids.Where(p => p.SitusStree.Equals(street, StringComparison.OrdinalIgnoreCase))
                                     .FirstOrDefault(p => p.SitusHouse.Equals(number.ToString(), StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Find the census block containing this parcel
        /// </summary>
        private CensusBlocks findCensusBlock(ParcelCentroids parcel)
        {
            if (parcel == null || parcel.Shape == null)
                return null;

            foreach (var block in allCensusBlocks)
            {
                if (block.Shape.Contains(parcel.Shape))
                {
                    return block;
                }
            }

            return null;
        }
    }
}
