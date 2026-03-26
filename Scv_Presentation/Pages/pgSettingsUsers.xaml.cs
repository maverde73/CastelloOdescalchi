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

namespace Presentation.Pages
{
	/// <summary>
	/// Interaction logic for pgSettingsGeneral.xaml
	/// </summary>
	public partial class pgSettingsUsers : BaseContentPage
	{
		#region Variables

		private int MasterID;
		LK_User_Dal dal = new LK_User_Dal();

		#endregion// Variables



		#region Constructors

		public pgSettingsUsers()
		{
			InitializeComponent();
			CommandEvent += new CommonEvents.ContentPageCommandEventHandler(pgUsers_CommandEvent);

			grdMaster.SelectionUnit = Telerik.Windows.Controls.GridView.GridViewSelectionUnit.FullRow;
			grdMaster.IsReadOnly = true;
			grdMaster.SelectionChanged += new EventHandler<Telerik.Windows.Controls.SelectionChangeEventArgs>(grdMaster_SelectionChanged);
			grdMaster.MouseDoubleClick += new MouseButtonEventHandler(grdMaster_MouseDoubleClick);
			grdMaster.Grouped += new EventHandler<GridViewGroupedEventArgs>(grdMaster_Grouped);
			BindMaster();
		}

		#endregion// Constructrors



		#region Events

		void pgUsers_CommandEvent(ContentPageCommandEventArgs e)
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
					if (MessageBox.Show("Eliminare gli utenti selezionati? L'operazione non potrà essere annullata!", "Eliminazione record", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
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
					MasterID = (int)((LK_User)grd.SelectedItems[0]).Id_User;

				NotifySelection();
			}
		}

		void grdMaster_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			RadGridView grd = sender as RadGridView;
			if (grd != null)
				if (grd.SelectedItems.Count > 0)
					DoOpenItem(((LK_User)grd.SelectedItems[0]), WindowStartupLocation.CenterScreen);
		}

		void grdMaster_Grouped(object sender, GridViewGroupedEventArgs e)
		{
			grdMaster.CollapseAllGroups();
		}

		#endregion// Events



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
					if (MessageBox.Show("Eliminare gli utenti selezionati? L'operazione non potrà essere annullata!", "Eliminazione record", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
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
			BaseFilter filter = new BaseFilter();
			filter.AddSortField("Nominativo");
			filter.SortDirection = SortDirection.ASC;
			int count = 0;
			List<LK_User> MasterTable = dal.GetFilteredList(filter.Args, filter.Sort, filter.SortDirection.ToString(), filter.PageSize, filter.PageNumber, out count);
			grdMaster.DataContext = MasterTable;
		}

		private void NotifySelection()
		{
			ContentPageCommandEventArgs args = new ContentPageCommandEventArgs();

			args.CommandType = ContentPageCommandType.SelectionChanged;

			for (int i = 0; i < grdMaster.SelectedItems.Count; i++)
				args.SelectedIDs.Add((int)((LK_User)grdMaster.SelectedItems[i]).Id_User);

			OnItemSelectionEvent(args);
		}

		private void DoNewItem()
		{
			BaseDetailPage frm = new wndUserDetail(0);
			frm.WindowStartupLocation = WindowStartupLocation.CenterScreen;
			frm.ShowDialog();
			BindMaster();
		}

		private void DoOpenItems()
		{
			if (grdMaster.SelectedItems.Count > 0)
				for (int i = 0; i < grdMaster.SelectedItems.Count; i++)
					DoOpenItem(((LK_User)grdMaster.SelectedItems[i]), WindowStartupLocation.Manual);
		}

		private void DoOpenItem(LK_User obj, WindowStartupLocation startupLocation)
		{
			bool open = true;
			string windowName = string.Format("Guide_{0}", obj.Id_User.ToString());
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
				frm = new wndUserDetail(obj.Id_User);
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
					DoDeleteItem(((LK_User)grdMaster.SelectedItems[i]));
		}

		private void DoDeleteItem(LK_User obj)
		{
			LK_User_Dal dal = new LK_User_Dal();
			try
			{
				LK_User g = dal.GetSingleItem(obj.Id_User)[0];
				if (g != null)
					dal.DeleteObject(g);
			}
			catch (Exception e)
			{
				MessageBox.Show(e.Message, "Errore eliminazione", MessageBoxButton.OK);
			}
		}


		#endregion// Private Methods

	}
}
