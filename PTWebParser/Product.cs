using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTWebParser
{
    class Product : IProduct
    {
        public string Name { get; set; }
        public double Price { get; set; }
        public string CompCode { get; set; }
        public string VendorCode { get; set; }
        public string OthName { get; set; }
        public double OthPrice { get; set; }
        public double PriceDiff { get; set; }
        public bool IsPriceLess { get; }
    }
}
