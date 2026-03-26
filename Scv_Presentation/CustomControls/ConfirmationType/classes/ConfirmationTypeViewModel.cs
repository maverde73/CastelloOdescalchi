using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows;
using Scv_Entities;
using System.ComponentModel;

namespace Presentation
{
	class ConfirmationTypeViewModel : INotifyPropertyChanged
	{
		#region Events

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion // Events


		#region Properties

		private bool designTime = System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject());
		
		private  ObservableCollection<LK_TipoConferma> availables = null;
		public ObservableCollection<LK_TipoConferma> Availables
		{
			get
			{
				if (availables == null)
					availables = new ObservableCollection<LK_TipoConferma>();
				return availables;
			}
			set { availables = value; }
		}

		private LK_TipoConferma selectedItem = null;
		public LK_TipoConferma SelectedItem
		{
			get	
			{
				if (selectedItem == null)
					selectedItem = new LK_TipoConferma();
				return selectedItem; 			
			}
			set { selectedItem = value; OnPropertyChanged(this, "SelectedItem"); }
		}

		private LK_TipoConferma autoCompleteBoxSelectedItem = null;
		public LK_TipoConferma AutoCompleteBoxSelectedItem
		{
			get { return autoCompleteBoxSelectedItem; }
			set { autoCompleteBoxSelectedItem = value; OnPropertyChanged(this, "AutoCompleteBoxSelectedItem"); }
		}

		#endregion // Properties


		#region Contstructors

		public ConfirmationTypeViewModel()
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

				LK_TipoConferma r = null;

				try
				{
					r = (LK_TipoConferma)_context.LK_TipiConferma.First(x => x.Id_TipoConferma == itemID);
				}
				catch (Exception e)
				{

				}
				SelectedItem = r;
				//AutoCompleteBoxSelectedItem = r;
			}
		}

		private ObservableCollection<LK_TipoConferma> LoadAvailables()
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				return new ObservableCollection<LK_TipoConferma>(_context.LK_TipiConferma);
			}
		}

		private LK_TipoConferma Clone(LK_TipoConferma source)
		{
			LK_TipoConferma destination = new LK_TipoConferma();			
			((LK_TipoConferma)destination).Descrizione = ((LK_TipoConferma)source).Descrizione;
			((LK_TipoConferma)destination).Nota = ((LK_TipoConferma)source).Nota;

			return destination;
		}

		public LK_TipoConferma GetItemByText(string text)
		{
			return Availables.FirstOrDefault(x => x.Descrizione.ToUpper().Trim().Equals(text.ToUpper().Trim()));
		}

		#endregion
	}
}