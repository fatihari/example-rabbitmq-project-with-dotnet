using System;
using System.Collections.Generic;
using System.Collections;

namespace CreateFileWorkerService.Models
{
    public partial class Product
    {
        public int Productid { get; set; }
        public string Name { get; set; } = null!;
        public string Productnumber { get; set; } = null!;
        public BitArray Makeflag { get; set; } = null!;
        public BitArray Finishedgoodsflag { get; set; } = null!;
        public string? Color { get; set; }
        public short Safetystocklevel { get; set; }
        public short Reorderpoint { get; set; }
        public decimal Standardcost { get; set; }
        public decimal Listprice { get; set; }
        public string? Size { get; set; }
        public string? Sizeunitmeasurecode { get; set; }
        public string? Weightunitmeasurecode { get; set; }
        public decimal? Weight { get; set; }
        public int Daystomanufacture { get; set; }
        public string? Productline { get; set; }
        public string? Class { get; set; }
        public string? Style { get; set; }
        public int? Productsubcategoryid { get; set; }
        public int? Productmodelid { get; set; }
        public DateTime Sellstartdate { get; set; }
        public DateTime? Sellenddate { get; set; }
        public DateTime? Discontinueddate { get; set; }
        public Guid Rowguid { get; set; }
        public DateTime Modifieddate { get; set; }
    }
}
