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
	public class LocationViewModel : INotifyPropertyChanged
	{
		#region Events

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion // Events


		#region Properties

		private bool designTime = System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject());
		
		private  ObservableCollection<LK_Citta> availables = null;
		public ObservableCollection<LK_Citta> Availables
		{
			get
			{
				if (availables == null)
					availables = new ObservableCollection<LK_Citta>();
				return availables;
			}
			set { availables = value; }
		}

		private LK_Citta selectedItem = null;
		public LK_Citta SelectedItem
		{
			get	
			{
				if (selectedItem == null)
					selectedItem = new LK_Citta();
				return selectedItem; 			
			}
			set { selectedItem = value; OnPropertyChanged(this, "SelectedItem"); }
		}

		private LK_Citta autoCompleteBoxSelectedItem = null;
		public LK_Citta AutoCompleteBoxSelectedItem
		{
			get { return autoCompleteBoxSelectedItem; }
			set { autoCompleteBoxSelectedItem = value; OnPropertyChanged(this, "AutoCompleteBoxSelectedItem"); }
		}

		#endregion // Properties


		#region Contstructors

		public LocationViewModel()
		{
			if (!designTime)
				Availables = LoadAvailables();
		}

		#endregion // Constructors


		#region Event handling

		private void OnPropertyChanged(object sender, string propertyName)
		{
			switch (propertyName)
			{
				case "AutoCompleteBoxSelectedItem":
				if (AutoCompleteBoxSelectedItem != null)
					SelectedItem = Clone(autoCompleteBoxSelectedItem);
					break;
			}

			if (PropertyChanged != null)
				PropertyChanged(sender, new PropertyChangedEventArgs(propertyName));
		}

		#endregion // Event handling


		#region Main Methods

		public void SelectItem(int itemID)
		{

			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				LK_Citta r = null;

				try
				{
					r = (LK_Citta)_context.LK_Citta.First(x => x.Id_Citta == itemID);
				}
				catch (Exception e)
				{

				}
				SelectedItem = r;
				AutoCompleteBoxSelectedItem = r;
			}
		}

		private ObservableCollection<LK_Citta> LoadAvailables()
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				return new ObservableCollection<LK_Citta>(_context.LK_Citta);
			}
		}

		private LK_Citta Clone(LK_Citta source)
		{
			LK_Citta destination = new LK_Citta();			
			((LK_Citta)destination).Nome = ((LK_Citta)source).Nome;
			((LK_Citta)destination).CAP = ((LK_Citta)source).CAP;
			((LK_Citta)destination).Provincia = ((LK_Citta)source).Provincia;
			((LK_Citta)destination).Nazione = ((LK_Citta)source).Nazione;
			((LK_Citta)destination).Nota = ((LK_Citta)source).Nota;

			return destination;
		}

		public LK_Citta GetItemByText(string text)
		{
			return Availables.FirstOrDefault(x => x.Nome.ToUpper().Trim().Equals(text.ToUpper().Trim()));
		}

		#endregion
	}
}