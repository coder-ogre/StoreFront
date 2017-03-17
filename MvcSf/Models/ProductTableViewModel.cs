using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcSf.Models
{
    public class ProductTableViewModel
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public Nullable<int> Quantity { get; set; }
        public Nullable<decimal> Price { get; set; }
        public string ImageFile { get; set; }
        public byte[] ProductImage { get; set; }
        //public int ResultCount { get; set; }
        public int AlreadyInCart { get; set; }
    }
}