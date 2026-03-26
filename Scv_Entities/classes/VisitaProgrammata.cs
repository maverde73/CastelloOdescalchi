using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects.DataClasses;
using System.ComponentModel;

namespace Scv_Entities
{
	public partial class VisitaProgrammata : EntityObject, INotifyPropertyChanged
	{

		#region Events

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion// Events



		#region Private Fields

		private bool? fl_AvvisaGuida = false;

		private bool canDeleteScheduledVisit = false;

		#endregion// Private fields



		#region Public Properties

		public bool? Fl_AvvisaGuida
		{
			get { return fl_AvvisaGuida; }
			set { fl_AvvisaGuida = value; OnPropertyChanged(this, "Fl_AvvisaGuida"); }
		}

		public bool CanDeleteScheduledVisit
		{
			get { return canDeleteScheduledVisit; }
			set { canDeleteScheduledVisit = value; OnPropertyChanged(this, "CanDeleteScheduledVisit"); }
		}

		#endregion// Public Properties



		#region Event Handlers

		private void OnPropertyChanged(object sender, string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(sender, new PropertyChangedEventArgs(propertyName));
		}

		#endregion// Event Handlers
	}
}
