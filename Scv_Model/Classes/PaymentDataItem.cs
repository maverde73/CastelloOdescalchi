using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scv_Model.Classes
{
	public class PaymentDataItem
	{

		public string Protocollo { get; set; }
		public string Richiedente { get; set; }
		public string Email { get; set; }
		public string Responsabile { get; set; }
		public string Visitatori { get; set; }
		public string DataRisposta { get; set; }

		private List<PaymentDataDetail> details = null;
		public List<PaymentDataDetail> Details
		{
			get
			{
				if (details == null)
					details = new List<PaymentDataDetail>();
				return details;
			}
			set { details = value; }
		}
	}

	public class PaymentDataDetail
	{
		public string Data { get; set; }
		public string Ora { get; set; }
		public string BigliettiInteri { get; set; }
		public string BigliettiRidotti { get; set; }
        public string BigliettiScontati { get; set; }
        public string BigliettiCumulativi { get; set; }
		public string Lingua { get; set; }
	}
}
