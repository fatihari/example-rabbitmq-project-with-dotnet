using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace CreateFileWorkerService.Models
{
    public partial class AdventureworksContext : DbContext
    {
        public AdventureworksContext()
        {
        }

        public AdventureworksContext(DbContextOptions<AdventureworksContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Product> Products { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("product");

                entity.Property(e => e.Class)
                    .HasMaxLength(2)
                    .HasColumnName("class")
                    .IsFixedLength();

                entity.Property(e => e.Color)
                    .HasMaxLength(15)
                    .HasColumnName("color");

                entity.Property(e => e.Daystomanufacture).HasColumnName("daystomanufacture");

                entity.Property(e => e.Discontinueddate)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("discontinueddate");

                entity.Property(e => e.Finishedgoodsflag)
                    .HasColumnType("bit(1)")
                    .HasColumnName("finishedgoodsflag");

                entity.Property(e => e.Listprice).HasColumnName("listprice");

                entity.Property(e => e.Makeflag)
                    .HasColumnType("bit(1)")
                    .HasColumnName("makeflag");

                entity.Property(e => e.Modifieddate)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("modifieddate")
                    .HasDefaultValueSql("now()");

                entity.Property(e => e.Name)
                    .HasMaxLength(50)
                    .HasColumnName("name");

                entity.Property(e => e.Productid)
                    .ValueGeneratedOnAdd()
                    .HasColumnName("productid");

                entity.Property(e => e.Productline)
                    .HasMaxLength(2)
                    .HasColumnName("productline")
                    .IsFixedLength();

                entity.Property(e => e.Productmodelid).HasColumnName("productmodelid");

                entity.Property(e => e.Productnumber)
                    .HasMaxLength(25)
                    .HasColumnName("productnumber");

                entity.Property(e => e.Productsubcategoryid).HasColumnName("productsubcategoryid");

                entity.Property(e => e.Reorderpoint).HasColumnName("reorderpoint");

                entity.Property(e => e.Rowguid).HasColumnName("rowguid");

                entity.Property(e => e.Safetystocklevel).HasColumnName("safetystocklevel");

                entity.Property(e => e.Sellenddate)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("sellenddate");

                entity.Property(e => e.Sellstartdate)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("sellstartdate");

                entity.Property(e => e.Size)
                    .HasMaxLength(5)
                    .HasColumnName("size");

                entity.Property(e => e.Sizeunitmeasurecode)
                    .HasMaxLength(3)
                    .HasColumnName("sizeunitmeasurecode")
                    .IsFixedLength();

                entity.Property(e => e.Standardcost).HasColumnName("standardcost");

                entity.Property(e => e.Style)
                    .HasMaxLength(2)
                    .HasColumnName("style")
                    .IsFixedLength();

                entity.Property(e => e.Weight)
                    .HasPrecision(8, 2)
                    .HasColumnName("weight");

                entity.Property(e => e.Weightunitmeasurecode)
                    .HasMaxLength(3)
                    .HasColumnName("weightunitmeasurecode")
                    .IsFixedLength();
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
