using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scv_Entities
{
	public partial class Invoice : V_Movimento
	{
		#region Private Fields

		private string storeName = string.Empty;
		private string storeAddress1 = string.Empty;
		private string storeAddress2 = string.Empty;
		private int? invoiceNumber = (int?)null;
		private DateTime? invoiceDate = null;
		private DateTime? paymentDate = null;
		private bool isReadOnly = false;
		private bool isEditable = true;
		private decimal? invoiceTotal = (decimal?)null;
		private List<V_InvoiceDetails> details = null;

		#endregion// Private Fields



		#region Public Properties

		public string StoreName
		{
			get { return storeName; }
			set { storeName = value; }
		}

		public string StoreAddress1
		{
			get { return storeAddress1; }
			set { storeAddress1 = value; }
		}

		public string StoreAddress2
		{
			get { return storeAddress2; }
			set { storeAddress2 = value; }
		}

		public int? InvoiceNumber
		{
			get { return invoiceNumber; }
			set { invoiceNumber = value; }
		}

		public DateTime? InvoiceDate
		{
			get { return invoiceDate; }
			set { invoiceDate = value; }
		}

		public DateTime? PaymentDate
		{
			get { return paymentDate; }
			set { paymentDate = value; }
		}

		public bool IsReadOnly
		{
			get { return PaymentDate != null; }
		}

		public bool IsEditable
		{
			get { return PaymentDate == null; }
		}

		public decimal? InvoiceTotal
		{
			get { return invoiceTotal; }
			set { invoiceTotal = value; OnPropertyChanged("InvoiceTotal"); }
		}

		public decimal OriginalPrice
		{
			get { return ((PrezzoVendita != null ? (decimal)PrezzoVendita : 0) * 100) / (100 - (Sconto != null ? (decimal)Sconto : 0)); }
		}

		public List<V_InvoiceDetails> Details
		{
			get
			{
				if (details == null)
					details = new List<V_InvoiceDetails>();
				return details;
			}
			set { details = value; OnPropertyChanged("Details"); }
		}

		#endregion// Public Properties
	}
}
