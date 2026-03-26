using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using Scv_Model;
using System.ComponentModel;
using Scv_Entities;

namespace Presentation
{
	public class BaseContentPage : Page
	{
		#region Events

		public event Scv_Model.CommonEvents.ContentPageCommandEventHandler CommandEvent;
		public event Scv_Model.CommonEvents.ContentPageItemSelectionEventHandler ItemSelectionEvent;
		public event PropertyChangedEventHandler BasePropertyChanged;

		#endregion //Events


		#region Private Fields

		protected LK_User user = null;
		
		private int detailID;

		private BaseFilter filter = null;

		#endregion// Private Fields


		#region Public Properties

		public LK_User User
		{
			get
			{
				if (user == null)
					user = new LK_User();
				return user;
			}
			set { user = value; OnBasePropertyChanged(this, "User"); }
		}

		public int DetailID
		{
			get { return detailID; }
			set { detailID = value; }
		}

		public BaseFilter Filter
		{
			get
			{
				if (filter == null)
					filter = new BaseFilter();
				return filter;
			}
			set { filter = value; }
		}

		#endregion// Public Properties


		#region Constructors


		#endregion// Constructors



		#region Public Methods

		public void Command(ContentPageCommandEventArgs args )
		{
			OnCommandEvent(args);
		}

		public void Selection(ContentPageCommandEventArgs args)
		{

		}

		#endregion// Public Methods



		#region Event Handling

		protected void OnCommandEvent(ContentPageCommandEventArgs e)
		{
			if (CommandEvent != null)
				CommandEvent(e);
		}

		protected void OnItemSelectionEvent(ContentPageCommandEventArgs e)
		{
			if (ItemSelectionEvent != null)
				ItemSelectionEvent(e);
		}

		protected void OnBasePropertyChanged(object sender, string propertyName)
		{
			if (BasePropertyChanged != null)
				BasePropertyChanged(sender, new PropertyChangedEventArgs(propertyName));
		}
		#endregion// Event Handling

		
	}
}
