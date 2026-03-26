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
	public class ClosingsViewModel : INotifyPropertyChanged
	{
		#region Public Events

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion// Public Events


		#region Properties

		bool designTime = System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject());

		public LK_Chiusura_Dal dalClosings = new LK_Chiusura_Dal();

		private List<LK_Chiusura> srcClosings = null;
		public List<LK_Chiusura> SrcClosings
		{
            get 
			{
				if (srcClosings == null)
					srcClosings = new List<LK_Chiusura>() { new LK_Chiusura() };					
				return srcClosings; 			
			}
		}

		private LK_Chiusura objClosings = null;
		public LK_Chiusura ObjClosings
		{
			get
			{
				if (objClosings == null)
					objClosings = new LK_Chiusura();
				return objClosings;
			}
			set { objClosings = value; OnPropertyChanged(this, "ObjClosings"); }
		}

		#endregion // Properties


		#region Constructors

		public ClosingsViewModel(int detailID)
		{
			if (detailID > 0)
			{
				srcClosings = dalClosings.GetSingleItem(detailID);
			}
			else
			{
				LK_Chiusura u = new LK_Chiusura();
				u.Date = DateTime.Now;
				srcClosings = new List<LK_Chiusura>() { u };
			}

			ObjClosings = srcClosings[0];
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

        public List<LK_Chiusura> GetClosingsSingleItem(int id)
        {
            return dalClosings.GetSingleItem(id);
        }

		#endregion // Main Methods
	}
}
