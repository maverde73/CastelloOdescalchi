using System;
using System.Collections.Generic;
using System.Text;

namespace Thera.Biglietteria.Cassa.Commons.Utils
{
    public class TicketToPrintXML
    {
        /// <summary>
        /// BarCode.
        /// </summary>
        public string BarCode = null;
        /// <summary>
        /// Pax
        /// </summary>
        public string Pax = null;
        /// <summary>
        /// TicketType
        /// </summary>
        public string TicketType = null;
        /// <summary>
        /// Date
        /// </summary>
        public DateTime Date = DateTime.MinValue;
        /// <summary>
        ///Price
        /// </summary>
        public string Price = "0";

        /// <summary>
        /// Override for ToString().
        /// </summary>
        /// <returns>Dump string</returns>
        public override string ToString()
        {
            string s = string.Empty;
            s += DumpHelper.DumpField("BarCode", BarCode);
            s += " ";
            s += DumpHelper.DumpField("Pax", Pax);
            s += " ";
            s += DumpHelper.DumpField("TicketType", TicketType);
            s += " ";
            s += DumpHelper.DumpField("Date", Date);
            s += " ";
            s += DumpHelper.DumpField("Price", Price);
            return s;
        }
    }
}
