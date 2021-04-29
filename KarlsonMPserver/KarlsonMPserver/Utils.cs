using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KarlsonMPserver
{
    class Utils
    {
        public static string RemoveRichText(string input)
        {
            return input.Replace("<", "<<i></i>");
        }
    }
}
