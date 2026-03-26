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
	public class StoreViewModel : INotifyPropertyChanged
	{
		#region Public Events

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion// Public Events


		#region Properties

		bool designTime = System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject());

		public EsercizioVendita_Dal dalStore = new EsercizioVendita_Dal();
		public LK_Citta_Dal dalCity = new LK_Citta_Dal();

		private List<EsercizioVendita> srcStore = null;
		public List<EsercizioVendita> SrcStore
		{
            get 
			{
				if (srcStore == null)
					srcStore = new List<EsercizioVendita>() { new EsercizioVendita() };					
				return srcStore; 			
			}
			//set { srcStore = value; OnPropertyChanged(this, "SrcStore"); }
		}

		private EsercizioVendita objStore = null;
		public EsercizioVendita ObjStore
		{
			get
			{
				if (objStore == null)
					objStore = new EsercizioVendita();
				return objStore;
			}
			set { objStore = value; }
		}

		#endregion // Properties


		#region Constructors

		public StoreViewModel(int detailID)
		{
			if (detailID > 0)
				SrcStore[0] =  dalStore.GetItem(detailID);
			else
				SrcStore[0] = new EsercizioVendita();

			ObjStore = SrcStore[0];
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

        public List<EsercizioVendita> GetStoreSingleItem(int id)
        {
            return dalStore.GetSingleItem(id);
        }

		#endregion // Main Methods
	}
}
