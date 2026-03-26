using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows;
using Scv_Entities;
using System.Collections.ObjectModel;
using Scv_Dal;

namespace Presentation
{
	public class GoodiesViewModel : INotifyPropertyChanged
	{
		#region Public Events

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion// Public Events


		#region Properties

		bool designTime = System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject());

		public Articolo_Dal dalGoodies = new Articolo_Dal();
		public LK_TipoArticolo_Dal dalGoodiesTypes = new LK_TipoArticolo_Dal();

		private List<Articolo> srcGoodies = null;
		public List<Articolo> SrcGoodies
		{
            get 
			{
				if (srcGoodies == null)
					srcGoodies = new List<Articolo>() { new Articolo() };					
				return srcGoodies; 			
			}
			set { srcGoodies = value; OnPropertyChanged(this, "SrcGoodies"); }
		}

		private Articolo objGoodies = null;
		public Articolo ObjGoodies
		{
			get
			{
				if (objGoodies == null)
					objGoodies = new Articolo();
				return objGoodies;
			}
			set { objGoodies = value; }
		}

		#endregion // Properties


		#region Constructors

		public GoodiesViewModel(int detailID)
		{
			LoadAvailableGoodiesTypes();
			
			if (detailID > 0)
			{
				srcGoodies = dalGoodies.GetSingleItem(detailID);

				ObjGoodies = SrcGoodies[0];
			}
			else
			{
				Articolo gd = new Articolo();
				gd.Id_TipoArticolo = 1;
				srcGoodies = new List<Articolo>() { gd };
				ObjGoodies = SrcGoodies[0];
			}
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

        public List<Articolo> GetGoodiesSingleItem(int id)
        {
            return dalGoodies.GetSingleItem(id);
        }

		#endregion // Main Methods


        #region Goodies Types

        private ObservableCollection<LK_TipoArticolo> availableGoodiesTypes = null;
        public ObservableCollection<LK_TipoArticolo> AvailableGoodiesTypes
        {
            get
            {
                if (availableGoodiesTypes == null)
                    availableGoodiesTypes = new ObservableCollection<LK_TipoArticolo>();
                return availableGoodiesTypes;
            }
            set { availableGoodiesTypes = value; }
        }

        private void LoadAvailableGoodiesTypes()
        {
			AvailableGoodiesTypes = new ObservableCollection<LK_TipoArticolo>( dalGoodiesTypes.GetItems());
        }
        
        #endregion// Goodies TYpes
	}
}
