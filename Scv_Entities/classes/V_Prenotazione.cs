using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects.DataClasses;
using System.ComponentModel;

namespace Scv_Entities
{
	public partial class V_Prenotazione : EntityObject, INotifyPropertyChanged
	{
		#region Events

		event PropertyChangedEventHandler PropertyChanged;

		#endregion// Events



		#region Private Fields

		//V_VisiteProgrammate firstPrenotationVisit = null;

		#endregion// Private Fields




		#region Public Properties

		//public V_VisiteProgrammate FirstPrenotationVisit
		//{
		//    get
		//    {
		//        if (firstPrenotationVisit == null)
		//            firstPrenotationVisit = GetPrenotationFirstScheduledVisit();
		//        return firstPrenotationVisit;
		//    }
		//}

		public DateTime? RequestDate
		{
			get
			{
				return Dt_Prenotazione != DateTime.MinValue ? Dt_Prenotazione : (DateTime?)null;
			}
		}

		//public string VisitsDate
		//{
		//    get 
		//    {
		//        return FirstPrenotationVisit != null ? FirstPrenotationVisit.Dt_Visita.ToShortDateString() : string.Empty; 
		//    }
		//}

		//public int? VisitorsNumber
		//{
		//    get
		//    {
		//        int? visitorsNumber = (int?)null;
		//        using (IN_VIAEntities _context = new IN_VIAEntities())
		//        {
		//            Totsal
		//            List<VisitaPrenotata> prenotatedVisits = new List<VisitaPrenotata>();
		//            List<V_VisiteProgrammate> scheduledVisits = new List<V_VisiteProgrammate>();
		//            prenotatedVisits.AddRange(_context.V_VisiteProgrammate.Where(x => x.Id_Prenotazione == this.Id_Prenotazione));
		//            if (prenotatedVisits.Count > 0)
		//                visitorsNumber = 0;
		//            foreach (VisitaPrenotata v in prenotatedVisits)
		//            {
		//                VisitaProgrammata vp = _context.VisiteProgrammate.FirstOrDefault(x => x.Id_VisitaPrenotata == v.Id_VisitaPrenotata);
		//                if(vp != null)
		//                    visitorsNumber += (vp.Nr_Interi != null ? (int)vp.Nr_Interi : 0) + (vp.Nr_Omaggio != null ? (int)vp.Nr_Omaggio : 0) + (vp.Nr_Ridotti != null ? (int)vp.Nr_Ridotti : 0);
		//            } 
		//        }

		//        return visitorsNumber;				
		//    }
		//}

		//public string VisitHour
		//{
		//    get
		//    {				
		//        return FirstPrenotationVisit != null ? FirstPrenotationVisit.Ora_Visita : string.Empty;
		//    }
		//}

		//public string VisitLanguage
		//{
		//    get
		//    {
		//        return FirstPrenotationVisit != null ? FirstPrenotationVisit.LinguaVisita : string.Empty;
		//    }
		//}

		//public int? Receipt
		//{
		//    get
		//    {
		//        int? receipt = (int?)null;
		//        using (IN_VIAEntities _context = new IN_VIAEntities())
		//        {
		//            Pagamento p = _context.Pagamenti.FirstOrDefault(x => x.Id_Prenotazione == this.Id_Prenotazione);
		//            if (p != null)
		//            {
		//                int tmp;
		//                receipt = (int.TryParse(p.Ricevuta, out tmp)) != null ? tmp : (int?)null;
		//            }
		//        }

		//        return receipt;
		//    }
		//}

		//public string ReceiptDate
		//{
		//    get
		//    {
		//        string receiptDate = string.Empty;
		//        using (IN_VIAEntities _context = new IN_VIAEntities())
		//        {
		//            Pagamento p = _context.Pagamenti.FirstOrDefault(x => x.Id_Prenotazione == this.Id_Prenotazione);
		//            if (p != null)
		//                receiptDate = p.Dt_Pagamento != null ? p.Dt_Pagamento.Value.Date.ToShortDateString() : string.Empty;
		//        }

		//        return receiptDate;
		//    }
		//}

		//public string ResponseDate
		//{
		//    get 
		//    {
		//        return FirstPrenotationVisit != null && FirstPrenotationVisit.Dt_InvioAvviso != null ? FirstPrenotationVisit.Dt_InvioAvviso.Value.ToShortDateString() : string.Empty; 
		//    }
		//}

		#endregion// Public Properties




		#region Event Handlers

		private void OnPropertyChanged(object sender, string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(sender, new PropertyChangedEventArgs(propertyName));
		}

		#endregion// Event Handlers




		#region Utils

		private V_VisiteProgrammate GetPrenotationFirstScheduledVisit()
		{
			V_VisiteProgrammate obj = null;
			List<V_VisiteProgrammate> lstObj = null;

			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				lstObj = _context.V_VisiteProgrammate
					.Where(x => x.Id_Prenotazione == this.Id_Prenotazione)
					.OrderBy(x => x.Ora_Visita).ToList();
				if(lstObj != null && lstObj.Count > 0)
					obj = lstObj.First();
			}

			return obj;
		}

		#endregion// Utils
	}
}
