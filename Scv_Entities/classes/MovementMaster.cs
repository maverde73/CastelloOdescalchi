using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scv_Entities
{
	public class MovementMaster
	{
		private List<MovementDetail> details = null;

		public int ID { get; set; }
		public DateTime? Date { get; set; }
		public int MovementTypeID { get; set; }
		public string MovementType { get; set; }
		public int PaymentTypeID { get; set; }
		public string PaymentType { get; set; }
		public string PosNumber { get; set; }
		public int StoreID { get; set; }
		public string Store { get; set; }
		public decimal? StoreDiscount { get; set; }
		public int GuideID { get; set; }
		public string Guide { get; set; }
		public string Document { get; set; }
		public string Note { get; set; }
		public string MovementNote { get; set; }
		public int? BillID { get; set; }
		public decimal? Total
		{
			get
			{
				bool Calc =
					MovementTypeID == 3
					||
					MovementTypeID == 7
					||
					MovementTypeID == 8
					||
					MovementTypeID == 9
					||
					MovementTypeID == 11
					||
					MovementTypeID == 12
					||
					MovementTypeID == 14
					;

				decimal? t = Details.Count == 0 ? (decimal?)null : 0;
				foreach (MovementDetail d in Details)
					t += d.Total != null ? d.Total : 0;
				return t != null && Calc ? t.Value : (decimal?)null;
			}
		}
		public int? Year
		{
			get { return Date != null ? Date.Value.Year : (int?)null; }
		}
		public bool IsFatturato
		{
			get { return BillID == null ? false : BillID > 0 ?  true : false; }
		}
		public bool IsMoney
		{
			get
			{
				return (
					MovementTypeID == 3
					||
					MovementTypeID == 8
					||
					MovementTypeID == 9
					||
					MovementTypeID == 11
					||
					MovementTypeID == 12
				);
			}
		}
		public string Status
		{
			get { return ID > 0 ? (IsFatturato ?  "Fatturata" : "Non fatturata") : string.Empty; }
		}

		public bool CanBeInvoice
		{
			get { return !IsFatturato; }
		}

		public List<MovementDetail> Details
		{
			get
			{
				if (details == null)
					details = new List<MovementDetail>();
				return details;
			}
			set { details = value; }
		}

		public string ShowMovementType { get { return MovementType.Length > 0 ? "Visible" : "Collapsed"; } }
		public string ShowPaymentType { get { return PaymentType.Length > 0 ? "Visible" : "Collapsed"; } }
		public string ShowStore { get { return Store.Length > 0 ? "Visible" : "Collapsed"; } }
		public string ShowStoreDiscount { get { return StoreDiscount > 0 ? "Visible" : "Collapsed"; } }
		public string ShowGuide { get { return Guide.Length > 0 ? "Visible" : "Collapsed"; } }
		public string ShowDocument { get { return Document.Length > 0 ? "Visible" : "Collapsed"; } }

		public MovementMaster()
		{
			ID = 0;
			MovementType = string.Empty;
			PaymentType = string.Empty;
			PosNumber = string.Empty;
			Store = string.Empty;
			Guide = string.Empty;
			Document = string.Empty;

		}
	}
}
