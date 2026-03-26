using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects.DataClasses;
using System.ComponentModel;

namespace Scv_Entities
{
	public partial class MandatoDettaglio : EntityObject, INotifyPropertyChanged
	{
		#region Events

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion// Events

		#region Private Fields

		private bool isNumberReadOnly = false;

		private bool isValueReadOnly = false;

		private bool validateValue = false;

		#endregion// Private Fields




		#region Public Properties

		public bool IsNumberReadOnly
		{
			get { return isNumberReadOnly; }
			set { isNumberReadOnly = value; OnPropertyChanged(this, "IsNumberReadOnly"); }
		}

		public bool IsValueReadOnly
		{
			get { return isValueReadOnly; }
			set { isValueReadOnly = value; OnPropertyChanged(this, "IsValueReadOnly"); }
		}

		public bool ValidateValue
		{
			get { return validateValue; }
			set { validateValue = value; OnPropertyChanged(this, "ValidateValue"); }
		}

		#endregion// Public properties




		#region Event Handlers

		partial void OnNumeroChanged()
		{
			OnPropertyChanged(this, "Numero");
		}

		partial void OnValoreChanged()
		{
			OnPropertyChanged(this, "Valore");
		}

		private void OnPropertyChanged(object sender, string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(sender, new PropertyChangedEventArgs(propertyName));

			switch (propertyName)
			{
				case "Numero":
					decimal taglio = 0;
					decimal.TryParse(Taglio.Replace(".", ","), out taglio);
					if (IsValueReadOnly && taglio > 0)
						Valore = Numero * taglio;
					break;
			}


		}

		#endregion// Event Handlers
	}
}
