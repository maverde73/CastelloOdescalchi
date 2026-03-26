using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects.DataClasses;
using System.ComponentModel;

namespace Scv_Entities
{
	public partial class V_Mandato : EntityObject, INotifyPropertyChanged
	{
		private string guide = string.Empty;
		public string Guide
		{
			get { return (CognomeGuida != null ? CognomeGuida : string.Empty) + " " + (NomeGuida != null ? NomeGuida : string.Empty); }
		}

		public string Group
		{
			get
			{
				return
					Identificativo.ToString().PadLeft(8, '0')
					+ " (Identificativo) - Data: "
					+ _Dt_Movimento.ToShortDateString()
					+ "  Tipo movimento: " + TipoMovimento
					;
			}
		}

		private decimal totale = 0;
		public decimal Totale
		{
			get { return totale; }
			set { totale = value; OnPropertyChanged(this, "Totale"); }
		}

		private bool printNote = false;
		public bool PrintNote
		{
			get { return Id_TipoMovimento != 9; } //compenso guide
		}

		private DateTime? movementDate = (DateTime?)null;
		public DateTime? MovementDate
		{
			get { return Dt_Movimento != DateTime.MinValue ? Dt_Movimento : (DateTime?)null; }
		}

		public event PropertyChangedEventHandler PropertyChanged;
		private void OnPropertyChanged(object sender, string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(sender, new PropertyChangedEventArgs(propertyName));
		}

	}
}
