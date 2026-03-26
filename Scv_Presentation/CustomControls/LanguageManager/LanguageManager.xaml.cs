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
	/// Interaction logic for LanguageManager.xaml
	/// </summary>
	public partial class LanguageManager : UserControl, INotifyPropertyChanged
	{
		#region Events

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion // Events


		#region Properties

		public LK_Lingua SelectedItem
		{
			get { return ((LanguageViewModel)DataContext).SelectedItem; }
			set { ((LanguageViewModel)DataContext).SelectedItem = value; }
		}

		protected string caption = string.Empty;
		public string Caption
		{
			get { return caption; }
			set { caption = value; }
		}

		private LK_Lingua_Dal Dal
		{
			get { return new LK_Lingua_Dal(); }
		}

		#endregion // Properties


		#region Constructors

		public LanguageManager()
		{
			DataContext = new LanguageViewModel();
			((LanguageViewModel)this.DataContext).PropertyChanged += new PropertyChangedEventHandler(OnViewModelPropertyChanged);
			
			InitializeComponent();
		}

		#endregion // Constructors


		#region Overrides

		public override void EndInit()
		{
			base.EndInit();
			lblSearch.Content = Caption;
		}

		#endregion// Overrides


		#region Event Handling

		private void cmb_SelectionChanged(object sender, EventArgs e)
		{
			RadComboBox c = sender as RadComboBox;
			if(c!= null)
			{			
				int id = 0;
				try
				{
					id = int.Parse(c.SelectedValue.ToString());
				}
				catch
				{

				}

				((LanguageViewModel)DataContext).SelectItem(id);
			}
		}

		private void cmb_SearchTextChanged(object sender, EventArgs e)
		{
			RadAutoCompleteBox rac = sender as RadAutoCompleteBox;

			if (rac != null)
			{
				if (rac.FilteredItems.GetType().Name == "WhereSelectEnumerableIterator`2")
				{
					LK_Lingua i = ((LanguageViewModel)DataContext).GetItemByText(rac.SearchText);
					if (i != null)
						SelectedItem = i;
					else
					{
						SelectedItem = new LK_Lingua();
						SelectedItem.Descrizione = rac.SearchText;
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
			((LanguageViewModel)DataContext).SelectItem(itemID);
			cmb.SelectedValue = ((LanguageViewModel)DataContext).SelectedItem.Id_Lingua;
		}

		#endregion
	}
}
