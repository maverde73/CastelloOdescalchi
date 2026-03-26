using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows;
using System.Collections.ObjectModel;
using Scv_Entities;

namespace Presentation
{
	class OrganizationViewModel : INotifyPropertyChanged
	{
		#region Events

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion // Events


		#region Properties

		private bool designTime = System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject());
		
		private  ObservableCollection<LK_Organizzazione> availables = null;
		public ObservableCollection<LK_Organizzazione> Availables
		{
			get
			{
				if (availables == null)
					availables = new ObservableCollection<LK_Organizzazione>();
				return availables;
			}
			set { availables = value; }
		}

		private LK_Organizzazione selectedItem = null;
		public LK_Organizzazione SelectedItem
		{
			get	
			{
				if (selectedItem == null)
					selectedItem = new LK_Organizzazione();
				return selectedItem; 			
			}
			set { selectedItem = value; OnPropertyChanged(this, "SelectedItem"); }
		}

		private LK_Organizzazione autoCompleteBoxSelectedItem = null;
		public LK_Organizzazione AutoCompleteBoxSelectedItem
		{
			get { return autoCompleteBoxSelectedItem; }
			set { autoCompleteBoxSelectedItem = value; OnPropertyChanged(this, "AutoCompleteBoxSelectedItem"); }
		}

		#endregion // Properties


		#region Contstructors

		public OrganizationViewModel()
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

				LK_Organizzazione r = null;

				try
				{
					r = (LK_Organizzazione)_context.LK_Organizzazioni.First(x => x.Id_Organizzazione == itemID);
				}
				catch (Exception e)
				{

				}
				SelectedItem = r;
				AutoCompleteBoxSelectedItem = r;
			}
		}

		private ObservableCollection<LK_Organizzazione> LoadAvailables()
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				return new ObservableCollection<LK_Organizzazione>(_context.LK_Organizzazioni);
			}
		}

		private LK_Organizzazione Clone(LK_Organizzazione source)
		{
			LK_Organizzazione destination = new LK_Organizzazione();			
			((LK_Organizzazione)destination).Descrizione = ((LK_Organizzazione)source).Descrizione;
			((LK_Organizzazione)destination).Nota = ((LK_Organizzazione)source).Nota;

			return destination;
		}

		public LK_Organizzazione GetItemByText(string text)
		{
			return Availables.FirstOrDefault(x => x.Descrizione.ToUpper().Trim().Equals(text.ToUpper().Trim()));
		}

		#endregion
	}
}