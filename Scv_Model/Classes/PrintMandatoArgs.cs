using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Scv_Model
{
	public class PrintMandatoArgs
	{
		#region Private Fields


		#endregion// Private Fields


		#region Public Properties

		public int NrProgressivo { get; set; }
        public string DtMandato { get; set; }
        public int IdMandato { get; set; }
        private bool onlinePaymentEnabled = false;
        public bool OnlinePaymentEnabled
        {
            get { return onlinePaymentEnabled; }
            set { onlinePaymentEnabled = value; }
        }

		public string LblInteri { get; set; }
		public string LblRidotti { get; set; }

		public int NrVisite { get; set; }
		public int NrVisitatori { get; set; }
		public int NrPrenotatiNonVenuti { get; set; }
		public int NrBigliettiTotale { get; set; }
		public int NrBigliettiInteriTotale { get; set; }
		public int NrBigliettiRidottiTotale { get; set; }
		public int NrBigliettiOmaggioTotale { get; set; }
        public int NrBigliettiScontatiTotale { get; set; }
        public int NrBigliettiCumulativiTotale { get; set; }
		public decimal BigliettiTotale { get; set; }
		public decimal BigliettiInteriTotale { get; set; }
		public decimal BigliettiRidottiTotale { get; set; }
        public decimal BigliettiScontatiTotale { get; set; }
        public decimal BigliettiCumulativiTotale { get; set; }
		public int NrBigliettiDa { get; set; }
		public int NrBigliettiA { get; set; }

		public decimal BigliettiIncassatiTotale { get; set; }
		public decimal BigliettiIncassatiTotaleContanti { get; set; }
		public decimal BigliettiIncassatiTotalePos { get; set; }
		public decimal BigliettiIncassatiTotaleAssegno { get; set; }

		public decimal VenditaArticoliTotale { get; set; }
		public decimal VenditaArticoliTotaleContanti { get; set; }
		public decimal VenditaArticoliTotalePos { get; set; }
		public decimal VenditaArticoliTotaleAssegno { get; set; }

		public decimal AltroTotaleIn { get; set; }
		public decimal AltroTotaleContantiIn { get; set; }
		public decimal AltroTotalePosIn { get; set; }
		public decimal AltroTotaleAssegnoIn { get; set; }

		public decimal AltroTotaleOut { get; set; }
		public decimal AltroTotaleContantiOut { get; set; }
		public decimal AltroTotalePosOut { get; set; }
		public decimal AltroTotaleAssegnoOut { get; set; }

		public decimal Rimborso1Valore { get; set; }
		public string Rimborso1Nota { get; set; }
		public decimal Rimborso2Valore { get; set; }
		public string Rimborso2Nota { get; set; }
		public decimal Rimborso3Valore { get; set; }
		public string Rimborso3Nota { get; set; }

		public decimal DepositoAnticipiTotale { get; set; }
		public decimal DepositoAnticipiTotaleContanti { get; set; }
		public decimal DepositoAnticipiTotalePos { get; set; }
		public decimal DepositoAnticipiTotaleAssegno { get; set; }
		public decimal DepositoAnticipiTotaleOnline { get; set; }
		public decimal DepositoAnticipiTotaleIOR { get; set; }

		public decimal BigliettiPrelevatiTotale { get; set; }
		public decimal BigliettiPrelevatiTotaleContanti { get; set; }
		public decimal BigliettiPrelevatiTotalePos { get; set; }
		public decimal BigliettiPrelevatiTotaleAssegno { get; set; }
		public decimal BigliettiPrelevatiTotaleOnline { get; set; }
		public decimal BigliettiPrelevatiTotaleIOR { get; set; }

		public int BigliettiRinunciatiNrTotale { get; set; }
		public int BigliettiRinunciatiNrTotaleInteri { get; set; }
		public int BigliettiRinunciatiNrTotaleRidotti { get; set; }
        public int BigliettiRinunciatiNrTotaleScontati { get; set; }
        public int BigliettiRinunciatiNrTotaleCumulativi { get; set; }
		public decimal BigliettiRinunciatiTotale { get; set; }
		public decimal BigliettiRinunciatiTotaleInteri { get; set; }
		public decimal BigliettiRinunciatiTotaleRidotti { get; set; }
        public decimal BigliettiRinunciatiTotaleScontati { get; set; }
        public decimal BigliettiRinunciatiTotaleCumulativi { get; set; }


		public int taglio500Num { get; set; }
		public int taglio200Num { get; set; }
		public int taglio100Num { get; set; }
		public int taglio50Num { get; set; }
		public int taglio20Num { get; set; }
		public int taglio10Num { get; set; }
		public int taglio5Num { get; set; }
		public int taglio2Num { get; set; }
		public int taglio1Num { get; set; }
		public int taglio050Num { get; set; }
		public int taglio020Num { get; set; }
		public int taglio010Num { get; set; }
		public int taglio005Num { get; set; }
		public int taglioAssegnoNum { get; set; }

		public decimal taglio500Val { get; set; }
		public decimal taglio200Val { get; set; }
		public decimal taglio100Val { get; set; }
		public decimal taglio50Val { get; set; }
		public decimal taglio20Val { get; set; }
		public decimal taglio10Val { get; set; }
		public decimal taglio5Val { get; set; }
		public decimal taglio2Val { get; set; }
		public decimal taglio1Val { get; set; }
		public decimal taglio050Val { get; set; }
		public decimal taglio020Val { get; set; }
		public decimal taglio010Val { get; set; }
		public decimal taglio005Val { get; set; }
		public decimal taglioAssegnoVal { get; set; }

		public int taglioNumTotale { get; set; }
		public decimal taglioValTotale { get; set; }

		public decimal RimanenzaTotale { get; set; }

		public decimal TotaleCassa
		{
			get
			{
				return
				BigliettiIncassatiTotaleContanti
				+
				BigliettiIncassatiTotaleAssegno
				+
				VenditaArticoliTotaleContanti
				+
				VenditaArticoliTotaleAssegno
				+
				(AltroTotaleContantiIn/* - AltroTotaleContantiOut*/)//Mah!
				+
				(AltroTotaleAssegnoIn/* - AltroTotaleAssegnoOut*/)
				+
				DepositoAnticipiTotaleContanti
				+
				DepositoAnticipiTotaleAssegno
				-
				(Rimborso1Valore + Rimborso2Valore + Rimborso3Valore)
				;
			}
		}

		public decimal SintesiContanti
		{
			get { return BigliettiIncassatiTotaleContanti + DepositoAnticipiTotaleContanti + VenditaArticoliTotaleContanti + AltroTotaleContantiIn - (Rimborso1Valore + Rimborso2Valore + Rimborso3Valore); }
		}

		public decimal SintesiAssegno
		{
			get { return BigliettiIncassatiTotaleAssegno + DepositoAnticipiTotaleAssegno + VenditaArticoliTotaleAssegno + AltroTotaleAssegnoIn; }
		}

		public decimal SintesiPos
		{
			get { return BigliettiIncassatiTotalePos + DepositoAnticipiTotalePos + VenditaArticoliTotalePos + AltroTotalePosIn; }
		}

		public decimal SintesiIOR
		{
			get { return DepositoAnticipiTotaleIOR; }
		}

		public decimal SintesiOnline
		{
			get { return DepositoAnticipiTotaleOnline; }
		}

		public decimal SintesiTotale
		{
			get { return SintesiContanti + SintesiAssegno + SintesiPos + SintesiIOR + SintesiOnline; }
		}

		public string Alert
		{
			get { return TotaleCassa != taglioValTotale ? "Totale cassa (contanti e assegni) euro " + taglioValTotale.ToString() + " diverso da Sintesi incassi euro " + TotaleCassa.ToString() + "." : string.Empty; }
		}

		#endregion// Public Properties
	}
}
