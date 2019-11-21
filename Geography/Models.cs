using System.Data.Entity.Spatial;

namespace CSM.WaterUsage.Geography
{
    public interface ICensusBlock
    {
        int OBJECTID { get; set; }
        string GEOID10 { get; set; }
        DbGeometry Shape { get; set; }
    }

    public interface ICenterline
    {
        int OBJECTID { get; set; }
        string FULLNAME { get; set; }
        decimal? ADLF { get; set; }
        decimal? ADLT { get; set; }
        decimal? ADRF { get; set; }
        decimal? ADRT { get; set; }
        DbGeometry Shape { get; set; }
    }

    public interface IParcelCentroid
    {
        int OBJECTID { get; set; }
        string SitusHouseNo { get; set; }
        string SitusStreet { get; set; }
        DbGeometry Shape { get; set; }
    }

    public class StreetSegment
    {
        public string Name { get; set; }
        public decimal Start { get; set; }
        public decimal End { get; set; }
        public int Parity { get; set; }
        public string Side
        {
            get
            {
                return Parity == 0 ? "even" : "odd";
            }
        }
        public DbGeometry Shape { get; set; }
        public DbGeometry Centroid { get; set; }
    }
}
