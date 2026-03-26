using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Scv_Entities;
using System.Windows;

namespace Presentation
{
	public class TitleViewModel : INotifyPropertyChanged
	{
		#region Events

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion // Events


		#region Properties

		private bool designTime = System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject());
		
		private  ObservableCollection<LK_Titolo> availables = null;
		public ObservableCollection<LK_Titolo> Availables
		{
			get
			{
				if (availables == null)
					availables = new ObservableCollection<LK_Titolo>();
				return availables;
			}
			set { availables = value; }
		}

		private LK_Titolo selectedItem = null;
		public LK_Titolo SelectedItem
		{
			get	
			{
				if (selectedItem == null)
					selectedItem = new LK_Titolo();
				return selectedItem; 			
			}
			set { selectedItem = value; OnPropertyChanged(this, "SelectedItem"); }
		}

		private LK_Titolo autoCompleteBoxSelectedItem = null;
		public LK_Titolo AutoCompleteBoxSelectedItem
		{
			get { return autoCompleteBoxSelectedItem; }
			set { autoCompleteBoxSelectedItem = value; OnPropertyChanged(this, "AutoCompleteBoxSelectedItem"); }
		}

		#endregion // Properties


		#region Contstructors

		public TitleViewModel()
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
				LK_Titolo r = null;

				try
				{
					r = (LK_Titolo)_context.LK_Titoli.First(x => x.Id_Titolo == itemID);
				}
				catch (Exception e)
				{

				}
				SelectedItem = r;
				AutoCompleteBoxSelectedItem = r;
			}
		}

		private ObservableCollection<LK_Titolo> LoadAvailables()
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				return new ObservableCollection<LK_Titolo>(_context.LK_Titoli);
			}
		}

		private LK_Titolo Clone(LK_Titolo source)
		{
			LK_Titolo destination = new LK_Titolo();			
			((LK_Titolo)destination).Sigla = ((LK_Titolo)source).Sigla;
			return destination;
		}

		public LK_Titolo GetItemByText(string text)
		{
			return Availables.FirstOrDefault(x => x.Sigla.ToUpper().Trim().Equals(text.ToUpper().Trim()));
		}

		#endregion
	}
}