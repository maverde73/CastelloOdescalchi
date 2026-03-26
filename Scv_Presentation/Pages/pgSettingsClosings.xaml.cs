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

namespace Presentation.Pages
{
	/// <summary>
	/// Interaction logic for pgSettingsClosings.xaml
	/// </summary>
	public partial class pgSettingsClosings : BaseContentPage, INotifyPropertyChanged
	{
		#region Events

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion// Events



		#region Private Fields

		private int MasterID;

		private LK_Chiusura_Dal dal = new LK_Chiusura_Dal();

		private List<YearItem> availableSourceYears = null;

		private YearItem selectedSourceYear = null;

		private List<YearItem> availableDestinationYears = null;

		private YearItem selectedDestinationYear = null;

		#endregion// Private Fields



		#region Public Properties

		public List<YearItem> AvailableSourceYears
		{
			get
			{
				if (availableSourceYears == null)
					availableSourceYears = LoadSourceYears();
				return availableSourceYears;
			}
		}

		public List<YearItem> AvailableDestinationYears
		{
			get
			{
				if (availableDestinationYears == null)
					availableDestinationYears = LoadDestinationYears(new List<int>() { SelectedSourceYear.Number });
				return availableDestinationYears;
			}
			set { availableDestinationYears = value; OnPropertyChanged(this, "AvailableDestinationYears"); }
		}

		public YearItem SelectedSourceYear
		{
			get
			{
				if (selectedSourceYear == null)
					selectedSourceYear = new YearItem();
				return selectedSourceYear;
			}
			set { selectedSourceYear = value; OnPropertyChanged(this, "SelectedSourceYear"); }
		}

		public YearItem SelectedDestinationYear
		{
			get
			{
				if (selectedDestinationYear == null)
					selectedDestinationYear = new YearItem();
				return selectedDestinationYear;
			}
			set { selectedDestinationYear = value; OnPropertyChanged(this, "SelectedDestinationYear"); }
		}

		#endregion// Public Properties



		#region Constructors

		public pgSettingsClosings()
		{
			InitializeComponent();

			DataContext = this;

			CommandEvent += new CommonEvents.ContentPageCommandEventHandler(pgUsers_CommandEvent);

			btnDuplicate.Click += new RoutedEventHandler(btnDuplicate_Click);

			cmbSourceYear.SelectionChanged += new SelectionChangedEventHandler(cmbSourceYear_SelectionChanged);

			grdMaster.SelectionUnit = Telerik.Windows.Controls.GridView.GridViewSelectionUnit.FullRow;
			grdMaster.IsReadOnly = true;
			grdMaster.SelectionChanged += new EventHandler<Telerik.Windows.Controls.SelectionChangeEventArgs>(grdMaster_SelectionChanged);
			grdMaster.MouseDoubleClick += new MouseButtonEventHandler(grdMaster_MouseDoubleClick);
			BindMaster();
		}


		#endregion// Constructors



		#region Events

		private void pgUsers_CommandEvent(ContentPageCommandEventArgs e)
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
					if (MessageBox.Show("Eliminare le chiusure selezionate? L'operazione non potrà essere annullata!", "Eliminazione record", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
					{
						DoDeleteItems();
						BindMaster();
						NotifySelection();
					}
					break;
			}
		}

		private void frm_Closed(object sender, EventArgs e)
		{
			BindMaster();
			NotifySelection();
		}

		private void BindMaster()
		{
			BaseFilter filter = new BaseFilter();
			filter.SortDirection = SortDirection.ASC;
			int count = 0;
			List<LK_Chiusura> MasterTable = dal.GetFilteredList(filter.Args, filter.Sort, filter.SortDirection.ToString(), filter.PageSize, filter.PageNumber, out count);
			grdMaster.DataContext = MasterTable;
		}

		private void grdMaster_SelectionChanged(object sender, Telerik.Windows.Controls.SelectionChangeEventArgs e)
		{
			RadGridView grd = sender as RadGridView;
			if (grd != null)
			{
				if (grd.SelectedItems.Count > 0)
					if(((LK_Chiusura)grd.SelectedItems[0]).Mese > 0)
						MasterID = (int)((LK_Chiusura)grd.SelectedItems[0]).Id_Chiusura;

				NotifySelection();
			}
		}

		private void grdMaster_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			RadGridView grd = sender as RadGridView;
			if (grd != null)
				if (grd.SelectedItems.Count > 0)
					if (((LK_Chiusura)grd.SelectedItems[0]).Mese > 0)
						DoOpenItem(((LK_Chiusura)grd.SelectedItems[0]), WindowStartupLocation.CenterScreen);
		}

		private void cmbSourceYear_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			AvailableDestinationYears = LoadDestinationYears(new List<int>() { SelectedSourceYear.Number });
		}

		private void btnDuplicate_Click(object sender, RoutedEventArgs e)
		{
			if (dal.YearExists(SelectedDestinationYear.Number))
				if (MessageBox.Show("ATTENZIONE: l'anno " + SelectedDestinationYear.Description + " esiste già in archivio. Se si procede verrà eliminato e sostituito con l'anno di partenza. Continuare?", "Avvertimento", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
					return;

			DoDuplicateYear(SelectedSourceYear.Number, SelectedDestinationYear.Number);
		}

		private void OnPropertyChanged(object sender, string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(sender, new PropertyChangedEventArgs(propertyName));
		}

		#endregion// Events



		#region Context Menu

		private void contextMenu_Opening(object sender, RoutedEventArgs e)
		{
			int selectedItems = grdMaster.SelectedItems.Count;
			mnuOpen.IsEnabled = false;
			mnuDelete.IsEnabled = false;
			if (selectedItems > 0 && ((LK_Chiusura)grdMaster.SelectedItems[0]).Mese > 0)
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
					if (MessageBox.Show("Eliminare le chiusure selezionate? L'operazione non potrà essere annullata!", "Eliminazione record", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
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

		private void NotifySelection()
		{
			ContentPageCommandEventArgs args = new ContentPageCommandEventArgs();

			args.CommandType = ContentPageCommandType.SelectionChanged;

			for (int i = 0; i < grdMaster.SelectedItems.Count; i++)
				args.SelectedIDs.Add((int)((LK_Chiusura)grdMaster.SelectedItems[i]).Id_Chiusura);

			OnItemSelectionEvent(args);
		}

		private void DoNewItem()
		{
			BaseDetailPage frm = new wndClosingsDetail(0);
			frm.WindowStartupLocation = WindowStartupLocation.CenterScreen;
			frm.ShowDialog();
			BindMaster();
		}

		private void DoOpenItems()
		{
			if (grdMaster.SelectedItems.Count > 0)
				for (int i = 0; i < grdMaster.SelectedItems.Count; i++)
					DoOpenItem(((LK_Chiusura)grdMaster.SelectedItems[i]), WindowStartupLocation.Manual);
		}

		private void DoOpenItem(LK_Chiusura obj, WindowStartupLocation startupLocation)
		{
			bool open = true;
			string windowName = string.Format("Guide_{0}", obj.Id_Chiusura.ToString());
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
				frm = new wndClosingsDetail(obj.Id_Chiusura);
				frm.User = User;
				frm.Name = windowName;
				frm.WindowStartupLocation = startupLocation;
				frm.Closed += new EventHandler(frm_Closed);
				frm.ShowDialog();
			}
		}

		private void DoDeleteItems()
		{
			if (grdMaster.SelectedItems.Count > 0)
				for (int i = 0; i < grdMaster.SelectedItems.Count; i++)
					DoDeleteItem(((LK_Chiusura)grdMaster.SelectedItems[i]));
		}

		private void DoDeleteItem(LK_Chiusura obj)
		{
			LK_Chiusura_Dal dal = new LK_Chiusura_Dal();
			try
			{
				LK_Chiusura g = dal.GetItem(obj.Id_Chiusura);
				if (g != null)
					dal.DeleteObject(g);
			}
			catch (UpdateException e)
			{
				MessageBox.Show(e.Message, "Errore eliminazione", MessageBoxButton.OK);
			}
		}

		private List<YearItem> LoadSourceYears()
		{
			return dal.GetExistingYears(new List<int>());
		}

		private List<YearItem> LoadDestinationYears(List<int> exclude)
		{
			return Helper_Dal.GetYears(2000 + 1, 2050, exclude);
		}

		private void DoDuplicateYear(int sourceYear, int destinationYear)
		{
			dal.DuplicateYear(sourceYear, destinationYear);
			BindMaster();
		}

		#endregion// Private Methods

	}
}
