﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    internal partial class GeographyEntities : DbContext
    {
        public GeographyEntities()
            : base("name=GeographyEntities")
        {
            CensusBlocks = Set<CensusBlocks>();
            Centerlines = Set<Centerlines>();
            ParcelCentroids = Set<ParcelCentroids>();
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        internal virtual DbSet<CensusBlocks> CensusBlocks { get; set; }
        internal virtual DbSet<Centerlines> Centerlines { get; set; }
        internal virtual DbSet<ParcelCentroids> ParcelCentroids { get; set; }
    }
}
