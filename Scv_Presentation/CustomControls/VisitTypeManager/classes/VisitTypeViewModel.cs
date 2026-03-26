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
	public class VisitTypeViewModel : INotifyPropertyChanged
	{
		#region Events

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion // Events


		#region Properties

		private bool designTime = System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject());
		
		private  ObservableCollection<LK_TipoVisita> availables = null;
		public ObservableCollection<LK_TipoVisita> Availables
		{
			get
			{
				if (availables == null)
					availables = new ObservableCollection<LK_TipoVisita>();
				return availables;
			}
			set { availables = value; }
		}

		private LK_TipoVisita selectedItem = null;
		public LK_TipoVisita SelectedItem
		{
			get	
			{
				if (selectedItem == null)
					selectedItem = new LK_TipoVisita();
				return selectedItem; 			
			}
			set { selectedItem = value; OnPropertyChanged(this, "SelectedItem"); }
		}

		private LK_TipoVisita autoCompleteBoxSelectedItem = null;
		public LK_TipoVisita AutoCompleteBoxSelectedItem
		{
			get { return autoCompleteBoxSelectedItem; }
			set { autoCompleteBoxSelectedItem = value; OnPropertyChanged(this, "AutoCompleteBoxSelectedItem"); }
		}

		#endregion // Properties


		#region Contstructors

		public VisitTypeViewModel()
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

				LK_TipoVisita r = null;

				try
				{
					r = (LK_TipoVisita)_context.LK_TipiVisita.First(x => x.Id_TipoVisita == itemID);
				}
				catch (Exception e)
				{

				}
				SelectedItem = r;
				//AutoCompleteBoxSelectedItem = r;
			}
		}

		private ObservableCollection<LK_TipoVisita> LoadAvailables()
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				return new ObservableCollection<LK_TipoVisita>(_context.LK_TipiVisita);
			}
		}

		private LK_TipoVisita Clone(LK_TipoVisita source)
		{
			LK_TipoVisita destination = new LK_TipoVisita();			
			((LK_TipoVisita)destination).Descrizione = ((LK_TipoVisita)source).Descrizione;
			((LK_TipoVisita)destination).Nota = ((LK_TipoVisita)source).Nota;

			return destination;
		}

		public LK_TipoVisita GetItemByText(string text)
		{
			return Availables.FirstOrDefault(x => x.Descrizione.ToUpper().Trim().Equals(text.ToUpper().Trim()));
		}

		#endregion
	}
}