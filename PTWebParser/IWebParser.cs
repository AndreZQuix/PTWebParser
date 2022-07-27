using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTWebParser
{
    interface IWebParser
    {
        public static string DocFolderPath = "../../../../Docs/";

        public void InitializeProperties();
        public bool IsFileCorrect();
        public void StartParsing();
    }
}
