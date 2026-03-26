using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Threading;
#if (!WindowsCE)
using Microsoft.SqlServer.Server;
#endif
using System.Data.SqlTypes;


namespace Thera.Biglietteria.Boca
{
    public class BarCode
    {
        private const uint totallen = 41;
#if (!WindowsCE)
        [Microsoft.SqlServer.Server.SqlProcedure]
#endif
        public static void SqlGenerateBarCode(int pax, DateTime date, int cont, int tipo, out long code)
        {
            try
            {
                code = 0;
                uint upax; uint ucont; uint utipo; ulong ucode;
                upax = Convert.ToUInt32(pax);
                ucont = Convert.ToUInt32(cont);
                ucode = Convert.ToUInt32(code);
                utipo = Convert.ToUInt32(tipo);
                GenerateBarCode(upax, date, ucont, utipo, ref ucode);
                code = Convert.ToInt64(ucode);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        public static void GenerateBarCode(int pax, DateTime date, int cont, int tipo, out long code)
        {
            SqlGenerateBarCode(pax, date, cont, tipo, out code);
        }
        public static void GenerateBarCode(uint pax, DateTime date, uint cont, uint tipo, ref ulong code)
        {
            code = 0;
            //ulong mod = Convert.ToUInt64((new DateTime(2027, 1, 1).Subtract(new DateTime(2014, 1, 1))).TotalSeconds);
            var sec=Convert.ToUInt64(date.Subtract(new DateTime(2014, 1, 1).Date).TotalSeconds);
            setBits(ref code, 1, sec, 29);
            setBits(ref code, 30, pax, 7);
            setBits(ref code, 37, tipo, 3);
            setBits(ref code, 40, cont, 2);

            //DecodeBarCode(ref pax, ref date, ref cont, ref tipo, code);


        }
#if(!WindowsCE)
        [Microsoft.SqlServer.Server.SqlProcedure]
#endif
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
        private static void setBits(ref ulong code, uint pos, ulong val, uint len)
        {
            try
            {
                uint mask = 1;
                for (uint i = 1; i < len; i++)
                {
                    mask <<= 1;
                    mask += 1;
                }
                if ((mask & val) != val)
                {
                    throw new Exception("valore troppo esteso per la lunghezza fornita");
                }
                uint sw = ((totallen - len) - (pos - 1));
                Debug.WriteLine("val:\t" + val.ToString());
                Debug.WriteLine("sw:\t" + sw.ToString());
                code = code | (val << Convert.ToInt32(sw));
            }
            catch (Exception ex)
            {

            }
        }


    }

}
