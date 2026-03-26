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
using Scv_Dal;
using Telerik.Windows.Controls;

namespace Presentation
{
	/// <summary>
	/// Interaction logic for TitleManager.xaml
	/// </summary>
	public partial class TitleManager : UserControl, INotifyPropertyChanged
	{
		#region Events

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion // Events


		#region Properties

		public LK_Titolo SelectedItem
		{
			get { return ((TitleViewModel)DataContext).SelectedItem; }
			set { ((TitleViewModel)DataContext).SelectedItem = value; }
		}

		private LK_Titolo_Dal Dal
		{
			get { return new LK_Titolo_Dal(); }
		}

		#endregion // Properties


		#region Constructors

		public TitleManager()
		{
			DataContext = new TitleViewModel();
			((TitleViewModel)this.DataContext).PropertyChanged += new PropertyChangedEventHandler(OnViewModelPropertyChanged);
			
			InitializeComponent();

		}

		#endregion // Constructors


		#region Event Handling

		private void cmb_SelectionChanged(object sender, EventArgs e)
		{
			SelectedItem = ((TitleViewModel)DataContext).AutoCompleteBoxSelectedItem;
		}

		private void cmb_SearchTextChanged(object sender, EventArgs e)
		{
			RadAutoCompleteBox rac = sender as RadAutoCompleteBox;

			if (rac != null)
			{
				if (rac.FilteredItems.GetType().Name == "WhereSelectEnumerableIterator`2")
				{
					LK_Titolo i = ((TitleViewModel)DataContext).GetItemByText(rac.SearchText);
					if (i != null)
						SelectedItem = i;
					else
					{
						SelectedItem = new LK_Titolo();
						SelectedItem.Sigla = rac.SearchText;
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
			((TitleViewModel)DataContext).SelectItem(itemID);
			cmb.SearchText = ((TitleViewModel)DataContext).SelectedItem.Sigla;
		}

		#endregion
	}
}
