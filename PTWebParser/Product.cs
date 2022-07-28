using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTWebParser
{
    public class Product : IProduct
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public string CompCode { get; set; }
        public string VendorCode { get; set; }
        public string OthName { get; set; }
        public double OthPrice { get; set; }
        public double PriceDiff { get; set; }
        public bool IsPriceLess { get; set; }

        public Product()
        {
            ID = 0;
            Name = String.Empty;
            Price = 0;
            CompCode = String.Empty;
            VendorCode = String.Empty;
            OthName = String.Empty;
            OthPrice = 0;
            PriceDiff = 0;
            IsPriceLess = false;
        }
    }
}
