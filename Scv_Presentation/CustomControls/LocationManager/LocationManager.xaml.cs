using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using Scv_Entities;
using Telerik.Windows.Controls;
using Scv_Dal;

namespace Presentation
{
	/// <summary>
	/// Interaction logic for LocationManager.xaml
	/// </summary>
	public partial class LocationManager : UserControl, INotifyPropertyChanged
	{
		#region Events

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion // Events


		#region Properties

		public LK_Citta SelectedItem
		{
			get { return ((LocationViewModel)DataContext).SelectedItem; }
			set { ((LocationViewModel)DataContext).SelectedItem = value; }
		}

		private LK_Citta_Dal Dal
		{
			get { return new LK_Citta_Dal(); }
		}

		#endregion // Properties


		#region Constructors

		public LocationManager()
		{
			DataContext = new LocationViewModel();
			((LocationViewModel)this.DataContext).PropertyChanged += new PropertyChangedEventHandler(OnViewModelPropertyChanged);
			
			InitializeComponent();

		}

		#endregion // Constructors


		#region Event Handling

		private void cmb_SelectionChanged(object sender, EventArgs e)
		{
			SelectedItem = ((LocationViewModel)DataContext).AutoCompleteBoxSelectedItem;
		}

		private void cmb_SearchTextChanged(object sender, EventArgs e)
		{
			RadAutoCompleteBox rac = sender as RadAutoCompleteBox;

			if (rac != null)
			{
				if (rac.FilteredItems.GetType().Name == "WhereSelectEnumerableIterator`2")
				{
					LK_Citta i = ((LocationViewModel)DataContext).GetItemByText(rac.SearchText);
					if (i != null)
						SelectedItem = i;
					else
					{
						SelectedItem = new LK_Citta();
						SelectedItem.Nome = rac.SearchText;
					}
				}
			}
		}

		private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case "SelectedItem":
					break;
			}

			if (PropertyChanged != null)
				PropertyChanged(sender, e);

		}

		#endregion // Event Handling


		#region Public Methods

		public void SelectItem(int itemID)
		{
			((LocationViewModel)DataContext).SelectItem(itemID);
			cmb.SearchText = ((LocationViewModel)DataContext).SelectedItem.Nome;
		}

		#endregion
	}
}
