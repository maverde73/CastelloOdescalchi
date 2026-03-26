using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scv_Model
{
	public class GuidePayRollPrintItem
	{

		public string Date { get; set; }
		public string Guide { get; set; }
		public int AttendancesNumber { get; set; }
		public decimal UnitCost { get; set; }
		public decimal Total { get; set; }
		public int Pieces50Number { get; set; }
		public decimal Pieces50Total { get; set; }
		public int Pieces20Number { get; set; }
		public decimal Pieces20Total { get; set; }
		public int Pieces10Number { get; set; }
		public decimal Pieces10Total { get; set; }
		public int EnvelopNumber { get; set; }

	}
}
