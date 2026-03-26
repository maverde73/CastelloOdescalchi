using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows;
using Scv_Entities;
using Scv_Dal;
using System.Collections.ObjectModel;

namespace Presentation
{
	public class UserViewModel : INotifyPropertyChanged
	{
		#region Public Events

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion// Public Events


		#region Properties

		bool designTime = System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject());

		public LK_User_Dal dalUser = new LK_User_Dal();

		private List<LK_User> srcUser = null;
		public List<LK_User> SrcUser
		{
            get 
			{
				if (srcUser == null)
					srcUser = new List<LK_User>() { new LK_User() };					
				return srcUser; 			
			}
			//set { srcUser = value; OnPropertyChanged(this, "srcUser"); }
		}

		private LK_User objUser = null;
		public LK_User ObjUser
		{
			get
			{
				if (objUser == null)
					objUser = new LK_User();
				return objUser;
			}
			set { objUser = value; }
		}

		#endregion // Properties


		#region Constructors

		public UserViewModel(int detailID)
		{
			if (detailID > 0)
			{
				srcUser = dalUser.GetSingleItem(detailID);
			}
			else
			{
				LK_User u = new LK_User();
				srcUser = new List<LK_User>() { u };
			}

			ObjUser = srcUser[0];
		}

		#endregion// Constructors


		#region Event Handling

		private void OnPropertyChanged(object sender, string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(sender, new PropertyChangedEventArgs(propertyName));
		}

		#endregion// Event Handling


		#region Main Methods

        public List<LK_User> GetGuideSingleItem(int id)
        {
            return dalUser.GetSingleItem(id);
        }

		#endregion // Main Methods
	}
}
