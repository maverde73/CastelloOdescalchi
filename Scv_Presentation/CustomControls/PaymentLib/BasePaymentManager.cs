using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Scv_Model;
using System.Data.Objects.DataClasses;
using System.ComponentModel;
using Telerik.Windows.Controls;
using Scv_Entities;

namespace Presentation.CustomControls.PaymentLib
{
	public class BasePaymentManager : Window, INotifyPropertyChanged
	{
		#region Events

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion //Events


		#region Private Fields

		private int detailID = 0;

		private List<int> VisitsIDs = null;

		private LK_User user = null;

		#endregion// Private Fields




		#region Public Properties

		public int DetailID
		{
			get { return detailID; }
			set { detailID = value; OnPropertyChanged(this, "DetailID"); }
		}

		public LK_User User
		{
			get
			{
				if (user == null)
					user = new LK_User();
				return user;
			}
			set { user = value; }
		}


		#endregion// Public Properties




		#region Event handlers

		protected void OnPropertyChanged(object sender, string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(sender, new PropertyChangedEventArgs(propertyName));
		}

		#endregion // Event handlers

	}
}
