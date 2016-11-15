using System.Data.Entity.Spatial;

namespace CSM.WaterUsage.Geography
{
    public class StreetSegment
    {
        public string Name { get; set; }
        public decimal Start { get; set; }
        public decimal End { get; set; }
        public int Parity { get; set; }
        public DbGeometry Shape { get; set; }
        public DbGeometry BufferedShape { get; set; }
    }
}
