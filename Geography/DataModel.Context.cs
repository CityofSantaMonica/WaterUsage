﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CSM.WaterUsage.Geography
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class GeographyEntities : DbContext
    {
        public GeographyEntities()
            : base("name=GeographyEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<CensusBlocks> CensusBlocks { get; set; }
        public virtual DbSet<Centerlines> Centerlines { get; set; }
        public virtual DbSet<ParcelCentroids> ParcelCentroids { get; set; }
    }
}
