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
using Scv_Entities;
using Scv_Model;
using Scv_Dal;
using Telerik.Windows.Controls;
using Telerik.Windows;
using System.Data;
using System.ComponentModel;
using Telerik.Windows.Controls.GridView;

namespace Presentation.Pages
{

	public partial class pgGuides : BaseContentPage, INotifyPropertyChanged
	{
		#region Public Events

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion// Public Events



		#region Private Fields

		private int MasterID;

		Guida_Dal dal = new Guida_Dal();

		private List<V_Guide> masterTable = null;

		private string guideName = string.Empty;

		private string selectedLanguage = string.Empty;

		private List<LK_Lingua> availableLanguages = null;

		#endregion// Private Fields



		#region Public Properties

		public List<V_Guide> MasterTable
		{
			get
			{
				if (masterTable == null)
					masterTable = new List<V_Guide>();
				return masterTable;
			}
			set { masterTable = value; OnPropertyChanged(this, "MasterTable"); }
		}

		public string GuideName
		{
			get { return guideName; }
			set { guideName = value; OnPropertyChanged(this, "GuideName"); }
		}

		public string SelectedLanguage
		{
			get { return selectedLanguage; }
			set { selectedLanguage = value; OnPropertyChanged(this, "SelectedLanguage"); }
		}

		public List<LK_Lingua> AvailableLanguages
		{
			get
			{
				LK_Lingua_Dal dal = new LK_Lingua_Dal();
				if (availableLanguages == null)
				{
					availableLanguages = dal.GetList();
					LK_Lingua o = new LK_Lingua();
					o.Id_Lingua = 0;
					o.Descrizione = string.Empty;
					availableLanguages.Insert(0, o);
				}
				return availableLanguages;
			}
		}

		#endregion// Public Properties



		#region Constructors

		public pgGuides()
		{
			InitializeComponent();

			DataContext = this;

			CommandEvent += new CommonEvents.ContentPageCommandEventHandler(pgGuides_CommandEvent);

			grdMaster.SelectionUnit = Telerik.Windows.Controls.GridView.GridViewSelectionUnit.FullRow;
			grdMaster.IsReadOnly = true;
			grdMaster.SelectionChanged += new EventHandler<Telerik.Windows.Controls.SelectionChangeEventArgs>(grdMaster_SelectionChanged);
			//grdMaster.MouseDoubleClick += new MouseButtonEventHandler(grdMaster_MouseDoubleClick);
			grdMaster.RowActivated += new EventHandler<RowEventArgs>(grdMaster_RowActivated);			
			grdMaster.Grouped += new EventHandler<GridViewGroupedEventArgs>(grdMaster_Grouped);

			btnFilter.Click += new RoutedEventHandler(btnFilter_Click);
			btnCancel.Click += new RoutedEventHandler(btnCancel_Click);

			Filter.AddSortField("Cognome");
			Filter.AddSortField("Nome");
			Filter.SortDirection = SortDirection.ASC;

			SelectedLanguage = AvailableLanguages[0].Descrizione;

			BindMaster();
		}

		#endregion



		#region Event Handlers

		void pgGuides_CommandEvent(ContentPageCommandEventArgs e)
		{
			switch (e.CommandType)
			{
				case ContentPageCommandType.New:
					DoNewItem();
					break;

				case ContentPageCommandType.Open:
					DoOpenItems();
					break;

				case ContentPageCommandType.Delete:
					if (MessageBox.Show("Eliminare le guide selezionate? L'operazione non potrà essere annullata!", "Eliminazione record", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
					{
						DoDeleteItems();
						BindMaster();
						NotifySelection();
					}
					break;
			}
		}

		void frm_Closed(object sender, EventArgs e)
		{
			BindMaster();
			NotifySelection();
		}

		void grdMaster_SelectionChanged(object sender, Telerik.Windows.Controls.SelectionChangeEventArgs e)
		{
			RadGridView grd = sender as RadGridView;
			if (grd != null)
			{
				if (grd.SelectedItems.Count > 0)
					MasterID = (int)((V_Guide)grd.SelectedItems[0]).Id_Guida;

				NotifySelection();
			}
		}

		void grdMaster_RowActivated(object sender, RowEventArgs e)
		{
			RadGridView grd = sender as RadGridView;
			if (grd != null)
				if (grd.SelectedItems.Count > 0)
					DoOpenItem(((V_Guide)grd.SelectedItems[0]), WindowStartupLocation.CenterScreen);
		}

		void btnFilter_Click(object sender, RoutedEventArgs e)
		{
			BindMaster();
		}

		void btnCancel_Click(object sender, RoutedEventArgs e)
		{
			DoCancelFilter();
			BindMaster();
		}

		void grdMaster_Grouped(object sender, GridViewGroupedEventArgs e)
		{
			DoCancelFilter();
			BindMaster();
		}

		void OnPropertyChanged(object sender, string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(sender, new PropertyChangedEventArgs(propertyName));
		}

		private void grdMaster_FilterOperatorsLoading(object sender, Telerik.Windows.Controls.GridView.FilterOperatorsLoadingEventArgs e)
		{
			if (
				e.Column.UniqueName == "titolo"
				||
				e.Column.UniqueName == "nominativo"
				||
				e.Column.UniqueName == "stato"
				||
				e.Column.UniqueName == "email"
				||
				e.Column.UniqueName == "lingua"
				||
				e.Column.UniqueName == "collegio"
				||
				e.Column.UniqueName == "capofila"
			)
				e.DefaultOperator1 = Telerik.Windows.Data.FilterOperator.Contains;
		}

		#endregion// Event Handlers



		#region Context Menu

		private void contextMenu_Opening(object sender, RoutedEventArgs e)
		{
			int selectedItems = grdMaster.SelectedItems.Count;
			mnuOpen.IsEnabled = false;
			mnuDelete.IsEnabled = false;
			if (selectedItems > 0)
			{
				mnuOpen.IsEnabled = true;
				mnuDelete.IsEnabled = true;
			}
		}

		private void contextMenu_Click(object sender, RadRoutedEventArgs e)
		{
			switch (((RadMenuItem)(e.Source)).CommandParameter.ToString())
			{
				case "open":
					DoOpenItems();
					break;
				case "delete":
					if (MessageBox.Show("Eliminare le guide selezionate? L'operazione non potrà essere annullata!", "Eliminazione record", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
					{
						DoDeleteItems();
						BindMaster();
						NotifySelection();
					}

					break;
			}
		}

		#endregion// Context Menu



		#region Private Methods

		private void BindMaster()
		{
			int count = 0;
			List<V_Guide> TmpTable = dal.GetFilteredList_V_Guide(Filter.Args, Filter.Sort, Filter.SortDirection.ToString(), Filter.PageSize, Filter.PageNumber, out count);

			List<V_Guide> FilteredTable = new List<V_Guide>();

			bool filterLanguage = true;
			bool filterName = true;

			MasterTable.Clear();

			foreach (V_Guide o in TmpTable)
			{
				if ((o.Lingue != null && o.Lingue.Contains(SelectedLanguage)) || o.Lingue == null)
					filterLanguage = true;
				else
					filterLanguage = false;

				if ((GuideName.Length > 0 && o.Nominativo.ToUpper().Contains(GuideName.ToUpper())) || GuideName.Length == 0)
					filterName = true;
				else
					filterName = false;

				if (filterLanguage && filterName)
					FilteredTable.Add(o);
			}

			MasterTable = FilteredTable;
			//grdMaster.DataContext = MasterTable;
		}

		private void NotifySelection()
		{
			ContentPageCommandEventArgs args = new ContentPageCommandEventArgs();

			args.CommandType = ContentPageCommandType.SelectionChanged;

			for (int i = 0; i < grdMaster.SelectedItems.Count; i++)
				args.SelectedIDs.Add((int)((V_Guide)grdMaster.SelectedItems[i]).Id_Guida);

			OnItemSelectionEvent(args);
		}

		private void DoNewItem()
		{
			BaseDetailPage frm = new wndGuideDetail(0);
			frm.WindowStartupLocation = WindowStartupLocation.CenterScreen;
			frm.ShowDialog();
			BindMaster();
		}

		private void DoOpenItems()
		{
			if (grdMaster.SelectedItems.Count > 0)
				for (int i = 0; i < grdMaster.SelectedItems.Count; i++)
					DoOpenItem(((V_Guide)grdMaster.SelectedItems[i]), WindowStartupLocation.Manual);
		}

		private void DoOpenItem(V_Guide obj, WindowStartupLocation startupLocation)
		{
			bool open = true;
			string windowName = string.Format("Guide_{0}", obj.Id_Guida.ToString());
			BaseDetailPage frm = null;
			foreach (Window wnd in Application.Current.Windows)
			{
				if (wnd.Name == windowName && wnd.IsVisible)
				{
					frm = (BaseDetailPage)wnd;
					frm.Focus();
					open = false;
				}
			}
			if (open)
			{
				frm = new wndGuideDetail(obj.Id_Guida);
				frm.User = User;
				frm.Name = windowName;
				frm.WindowStartupLocation = startupLocation;
				//frm.Closed += new EventHandler<Telerik.Windows.Controls.WindowClosedEventArgs>(frm_Closed);
				frm.Closed += new EventHandler(frm_Closed);
				frm.ShowDialog();
			}
		}

		private void DoDeleteItems()
		{
			if (grdMaster.SelectedItems.Count > 0)
				for (int i = 0; i < grdMaster.SelectedItems.Count; i++)
					DoDeleteItem(((V_Guide)grdMaster.SelectedItems[i]));
		}

		private void DoDeleteItem(V_Guide obj)
		{
			Guida_Dal dal = new Guida_Dal();
			try
			{
				Guida g = dal.GetItem(obj.Id_Guida);
				if (g != null)
					dal.DeleteObject(g);
			}
			catch (UpdateException e)
			{
				MessageBox.Show("Impossibile eliminare: fra i record selezionati, alcuni sono associati a una o più visite.\nEliminare le visite associate alle guide selezionate, quindi riprovare", "Errore eliminazione", MessageBoxButton.OK);
			}
		}

		private void DoCancelFilter()
		{
			GuideName = string.Empty;
			SelectedLanguage = AvailableLanguages[0].Descrizione;
		}

		#endregion// Private Methods

	}
}
