using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scv_Model
{
	public class PrintReceiptArgs
	{
		public DateTime Data { get; set; }
		public string Protocol { get; set; }
		public string TipoPagamento { get; set; }
		public decimal Importo { get; set; }
		public int NrInteri { get; set; }
		public decimal TotInteri { get; set; }
		public int NrRidotti { get; set; }
		public decimal TotRidotti { get; set; }
		public int NrOmaggio { get; set; }
		public string Avvisi { get; set; }
		public string Ricevuta { get; set; }
		public bool Promemoria { get; set; }
	}
}
