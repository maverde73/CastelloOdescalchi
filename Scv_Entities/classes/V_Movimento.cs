using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects.DataClasses;
using System.ComponentModel;

namespace Scv_Entities
{
	public partial class V_Movimento : EntityObject, INotifyPropertyChanged
	{
		private List<V_Movimento> details = null;
		public List<V_Movimento> Details
		{
			get
			{
				if (details == null)
					details = new List<V_Movimento>();
				return details;
			}
			set { details = value; OnPropertyChanged(this, "Details"); }
		}

		private decimal? totaleBolla = 0;
		public decimal? TotaleBolla
		{
			get { return totaleBolla; }
			set { totaleBolla = value; OnPropertyChanged(this, "TotaleBolla"); }
		}

		public decimal Totale
		{
			get 
			{
				bool Calc =
					Id_TipoMovimento == 3
					||
					Id_TipoMovimento == 7
					||
					Id_TipoMovimento == 8
					||
					Id_TipoMovimento == 9
					||
					Id_TipoMovimento == 11
					||
					Id_TipoMovimento == 12
					||
					Id_TipoMovimento == 14
					;
				return  Calc ? (PrezzoVendita != null ? (decimal)PrezzoVendita : 0) * (Nr_Pezzi != null ? (short)Nr_Pezzi : 0) : 0; 
			
			}
		}

		private int anno = 0;
		public int Anno
		{
			get { return this.Dt_Movimento.Year; }
		}

		private bool invoice = false;
		public bool Invoice
		{
			get { return invoice; }
			set { invoice = value; OnPropertyChanged("Invoice"); }
		}

		public bool IsFatturato
		{
			get { return Id_Fattura != null; }
		}

		public bool IsMoney
		{
			get
			{
				return (
					Id_TipoMovimento == 3
					||
					Id_TipoMovimento == 8
					||
					Id_TipoMovimento == 9
					||
					Id_TipoMovimento == 11
					||
					Id_TipoMovimento == 12
				);
			}
		}

		public string Status
		{
			get { return IsFatturato ? "Fatturata" : "Non fatturata"; }
		}

		public override string ToString()
		{
			return this.Identificativo.ToString();
		}

		public event PropertyChangedEventHandler PropertyChanged;
		private void OnPropertyChanged(object sender, string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(sender, new PropertyChangedEventArgs(propertyName));
		}

	}
}
