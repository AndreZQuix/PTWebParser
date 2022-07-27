﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTWebParser
{
    public interface IProduct
    {
        string Name { get; set; }
        double Price { get; set; }
        string CompCode { get; set; }
        string VendorCode { get; set; }
        string OthName { get; set; }
        double OthPrice { get; set; }
        double PriceDiff { get; set; }
        bool IsPriceLess { get; set; }
    }
}
