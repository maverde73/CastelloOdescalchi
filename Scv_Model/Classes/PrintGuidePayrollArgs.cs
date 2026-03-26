using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scv_Model
{
	public class PrintGuidePayrollArgs
	{

        public string MonthName { get; set; }
        public string YearNumber { get; set; }
		public decimal TotalNecropoli { get; set; }
		public decimal TotalAltro { get; set; }
		public decimal TotalAcconto { get; set; }
		public decimal MainTotal { get; set; }
		public decimal Cut1 { get; set; }
		public decimal Cut2 { get; set; }
		public decimal Cut3 { get; set; }
		public int Cut1Pieces { get; set; }
		public int Cut2Pieces { get; set; }
		public int Cut3Pieces { get; set; }
		public decimal Cut1Total { get; set; }
		public decimal Cut2Total { get; set; }
		public decimal Cut3Total { get; set; }
	}
}
