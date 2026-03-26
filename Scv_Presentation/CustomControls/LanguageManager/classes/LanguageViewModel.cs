using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows;
using Scv_Entities;

namespace Presentation
{
	public class LanguageViewModel : INotifyPropertyChanged
	{
		#region Events

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion // Events


		#region Properties

		private bool designTime = System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject());
		
		private  ObservableCollection<LK_Lingua> availables = null;
		public ObservableCollection<LK_Lingua> Availables
		{
			get
			{
				if (availables == null)
					availables = new ObservableCollection<LK_Lingua>();
				return availables;
			}
			set { availables = value; }
		}

		private LK_Lingua selectedItem = null;
		public LK_Lingua SelectedItem
		{
			get	
			{
				if (selectedItem == null)
					selectedItem = new LK_Lingua();
				return selectedItem; 			
			}
			set { selectedItem = value; OnPropertyChanged(this, "SelectedItem"); }
		}

		private LK_Lingua autoCompleteBoxSelectedItem = null;
		public LK_Lingua AutoCompleteBoxSelectedItem
		{
			get { return autoCompleteBoxSelectedItem; }
			set { autoCompleteBoxSelectedItem = value; OnPropertyChanged(this, "AutoCompleteBoxSelectedItem"); }
		}

		#endregion // Properties


		#region Contstructors

		public LanguageViewModel()
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

				LK_Lingua r = null;

				try
				{
					r = (LK_Lingua)_context.LK_Lingue.First(x => x.Id_Lingua == itemID);
				}
				catch (Exception e)
				{

				}
				SelectedItem = r;
				//AutoCompleteBoxSelectedItem = r;
			}
		}

		private ObservableCollection<LK_Lingua> LoadAvailables()
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				ObservableCollection<LK_Lingua> l = new ObservableCollection<LK_Lingua>(_context.LK_Lingue);
				l.Add(new LK_Lingua());
				return l;
			}
		}

		private LK_Lingua Clone(LK_Lingua source)
		{
			LK_Lingua destination = new LK_Lingua();			
			((LK_Lingua)destination).Descrizione = ((LK_Lingua)source).Descrizione;
			((LK_Lingua)destination).Nota = ((LK_Lingua)source).Nota;

			return destination;
		}

		public LK_Lingua GetItemByText(string text)
		{
			return Availables.FirstOrDefault(x => x.Descrizione.ToUpper().Trim().Equals(text.ToUpper().Trim()));
		}

		#endregion
	}
}