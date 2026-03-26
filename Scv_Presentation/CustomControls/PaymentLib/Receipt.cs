using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;


namespace Presentation.CustomControls.PaymentLib
{
	class Receipt : INotifyPropertyChanged
	{
		#region Public Events

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion// Public Events




		#region Private Fields

		private int ID = 0;

		private DateTime Date;


		#endregion// Private Fields





		#region Public Properties



		#endregion// Public Properties


		#region Event Handling

		private void OnPropertyChanged(object sender, string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(sender, new PropertyChangedEventArgs(propertyName));
		}

		#endregion// Event Handling

	}
}
