using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Threading;

namespace GestioneTornelloREA.Business
{
    public class Barcode
    {
        private const uint totallen = 41;

        public static void DecodeBarCode(ref uint pax, ref DateTime date, ref uint cont, ref uint tipo, ulong code)
        {
            ulong secs = getBits(code, 1, 29);
            pax = getBits(code, 30, 7);
            tipo = getBits(code, 37, 3);
            cont = getBits(code, 40, 2);
            date = (new DateTime(2014, 1, 1)).AddSeconds(Convert.ToDouble(secs));
        }

        private static uint getBits(ulong code, uint pos, uint len)
        {
            uint mask = 1;
            for (int i = 1; i < len; i++)
            {
                mask <<= 1;
                mask += 1;
            }
            uint sw = ((totallen - len) - (pos - 1));
            ulong val = (code >> Convert.ToInt32(sw)) & mask;
            return Convert.ToUInt32(val);
        }
    }
}
