using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scv_Model
{
	public class PaymentCheckResult
	{
		public int DiffFullPrice { get; set; }
		public int DiffDiscount { get; set; }
		public bool Success
		{
			get { return (DiffFullPrice == 0 && DiffDiscount == 0) ? true : false; }
		}
		public string Message
		{
			get
			{
				string msg = string.Empty;

				if (DiffFullPrice > 0)
					msg += "\n- Aggiunti " + DiffFullPrice.ToString() + " biglietti interi";

				if (DiffFullPrice < 0)
					msg += "\n- RImossi " + DiffFullPrice.ToString() + " biglietti interi";

				if (DiffDiscount > 0)
					msg += "\n- Aggiunti " + DiffDiscount.ToString() + " biglietti ridotti";

				if (DiffDiscount < 0)
					msg += "\n- RImossi " + DiffDiscount.ToString() + " biglietti ridotti";

				if (msg.Length > 0)
				{
					msg = "Attenzione:\nDopo il pagamento è stata variato il numero di visitatori e/o di visite per questo protocollo.\nSono state riscontrate le seguenti variazioni:"
						+ msg
						+ "\nSi vuole adeguare il pagamento alle attuali quantità dei biglietti?"
						;
				}

				return msg;
			}
		}
	}
}
