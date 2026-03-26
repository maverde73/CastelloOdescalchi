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
using Scv_Dal;
using System.Configuration;

namespace Presentation
{
    public abstract class BaseDetailPage : Window, INotifyPropertyChanged
	{
		#region Events

		public event PropertyChangedEventHandler PropertyChanged;
		public event Scv_Model.CommonDelegates.ClosingDetailWindowEventHandler DetailWindowClosing = null;

		#endregion //Events




        #region Private Fields

		private int detailID;

		private LK_User user = null;

		private EmailDestination emailDestination = EmailDestination.Send;

		private bool closeOnSave = false;

		private bool openable = true;

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
			set { user = value; OnPropertyChanged(this, "User"); }
		}

		public bool CloseOnSave
		{
			get { return closeOnSave; }
			set { closeOnSave = value; OnPropertyChanged(this, "CloseOnSave"); }
		}
		
		public EmailDestination EmailDestination
		{
			get { return emailDestination; }
			set { emailDestination = value; }
		}

		public bool LoadingFromDb = false;

		public bool Openable
		{
			get { return openable; }
			set { openable = value; }
		}

        public object savedObj = null;
        public object SavedObj
        {
            get { return savedObj; }
            set { savedObj = value; }
        }

		#endregion //Public Properties




		#region Constructors

		public BaseDetailPage(int detailID)
        {
            this.Topmost = Convert.ToBoolean(ConfigurationManager.AppSettings["windTopMost"]);
            this.DetailID = detailID;
        }

        public BaseDetailPage()
            : base()
        {
            this.Topmost = Convert.ToBoolean(ConfigurationManager.AppSettings["windTopMost"]);
        }

		#endregion // Constructors





		#region Event handlers

		public override void EndInit()
        {
            base.EndInit();
			SetLayout();
        }

        protected abstract void SetLayout();

		protected void OnPropertyChanged(object sender, string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(sender, new PropertyChangedEventArgs(propertyName));
		}

		protected void OnDetailWindowClosing(ClosingDetailWindowEventArgs args)
		{
			if (DetailWindowClosing != null)
				DetailWindowClosing(this, args);
		}

		#endregion // Event handlers
	}
}
