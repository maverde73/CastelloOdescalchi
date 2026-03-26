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
using Telerik.Windows.Controls;
using Scv_Entities;
using Scv_Dal;

namespace Presentation
{
	/// <summary>
	/// Interaction logic for VisitTypeManager.xaml
	/// </summary>
	public partial class VisitTypeManager : UserControl, INotifyPropertyChanged
	{
		#region Events

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion // Events


		#region Properties

		public LK_TipoVisita SelectedItem
		{
			get { return ((VisitTypeViewModel)DataContext).SelectedItem; }
			set { ((VisitTypeViewModel)DataContext).SelectedItem = value; }
		}

		protected string caption = string.Empty;
		public string Caption
		{
			get { return caption; }
			set { caption = value; }
		}

		private LK_TipoVisita_Dal Dal
		{
			get { return new LK_TipoVisita_Dal(); }
		}

		#endregion // Properties


		#region Constructors

		public VisitTypeManager()
		{
			DataContext = new VisitTypeViewModel();
			((VisitTypeViewModel)this.DataContext).PropertyChanged += new PropertyChangedEventHandler(OnViewModelPropertyChanged);
			
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

				((VisitTypeViewModel)DataContext).SelectItem(id);
			}
		}

		private void cmb_SearchTextChanged(object sender, EventArgs e)
		{
			RadAutoCompleteBox rac = sender as RadAutoCompleteBox;

			if (rac != null)
			{
				if (rac.FilteredItems.GetType().Name == "WhereSelectEnumerableIterator`2")
				{
					LK_TipoVisita i = ((VisitTypeViewModel)DataContext).GetItemByText(rac.SearchText);
					if (i != null)
						SelectedItem = i;
					else
					{
						SelectedItem = new LK_TipoVisita();
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
			((VisitTypeViewModel)DataContext).SelectItem(itemID);
			cmb.SelectedValue = ((VisitTypeViewModel)DataContext).SelectedItem.Id_TipoVisita;
		}

		#endregion
	}
}
