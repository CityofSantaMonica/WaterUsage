//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CSM.WaterUsage.Geography.EF
{
    using System;
    using System.Collections.Generic;
    
    internal partial class ParcelCentroids
    {
        public int OBJECTID { get; set; }
        public string SitusHouse { get; set; }
        public string SitusStree { get; set; }
        public System.Data.Entity.Spatial.DbGeometry Shape { get; set; }
    }
}
