using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows;

namespace Scv_Model
{
	public class ClosingDateResult :INotifyPropertyChanged
	{
		#region Public Events

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion// Public Events




		#region Private Fields

		private bool closedAM = false;
		private bool closedPM = false;
		private string message = string.Empty;

		#endregion// Private Fields




		#region Public Properties

		public bool ClosedAM
		{
			get { return closedAM; }
			set { closedAM = value; OnNotifyPropertyChanged(this, "ClosedAM"); }
		}

		public bool ClosedPM
		{
			get { return closedPM; }
			set { closedPM = value; OnNotifyPropertyChanged(this, "ClosedPM"); }
		}

		public bool Closed
		{
			get { return closedAM || closedPM; }
		}

		public Visibility ClosedMessageVisibility
		{
			get { return Closed ? Visibility.Visible : Visibility.Collapsed; }
		}

		public string Message
		{
			get
			{
				string message = string.Empty;
				if (ClosedAM && !ClosedPM)
					message = "Il castello resterà chiuso la mattina";
				if (ClosedPM && !ClosedAM)
                    message = "Il castello resterà chiuso il pomeriggio";
				if (ClosedPM && ClosedAM)
                    message = "Il castello resterà chiuso tutto il giorno";

				return message;
			}
		}
		#endregion// Public Properties




		#region Event Handling

		private void OnNotifyPropertyChanged(Object sender, string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(sender, new PropertyChangedEventArgs(propertyName));
		}

		#endregion// Event Handling
	}

}
