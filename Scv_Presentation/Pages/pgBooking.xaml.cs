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
using Scv_Model;
using Scv_Dal;
using Telerik.Windows.Controls;
using Scv_Entities;
using System.Data;
using Telerik.Windows;
using Scv_Model.Common;
using System.ComponentModel;
using Telerik.Windows.Data;
using System.Collections.ObjectModel;
using Telerik.Windows.Controls.GridView;

namespace Presentation.Pages
{

    /// <summary>
    /// Interaction logic for pgBooking.xaml
    /// </summary>
    public partial class pgBooking : BaseContentPage, INotifyPropertyChanged
    {

        DateTime start = DateTime.Now;

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion// Events


        #region Private Fields

        private int MasterID;
        private Prenotazione_Dal dal = new Prenotazione_Dal();
        private int selectedStatusTypeID = 0;
        private List<LK_TipoRisposta> availableStatusTypes = null;
        private List<V_Prenotazione> masterTable = null;
        private V_Prenotazione selectedItem = null;
        private List<IGroup> expandedGroups = new List<IGroup>();
        private int currentPageIndex = 0;
        BaseDetailPage frm = null;

        #endregion// Private Fields



        #region Public Properties

        public int SelectedStatusTypeID
        {
            get
            {
                return selectedStatusTypeID;
            }
            set { selectedStatusTypeID = value; OnPropertyChanged(this, "SelectedStatusTypeID"); }
        }

        public List<LK_TipoRisposta> AvailableStatusTypes
        {
            get
            {
                if (availableStatusTypes == null)
                {
                    LK_TipoRisposta_Dal dal = new LK_TipoRisposta_Dal();
                    availableStatusTypes = dal.GetList(9);
                    LK_TipoRisposta obj = new LK_TipoRisposta();
                    obj.Id_TipoRisposta = 0;
                    obj.Descrizione = "Tutte";
                    availableStatusTypes.Insert(0, obj);
                }
                return availableStatusTypes;
            }
        }

        public List<V_Prenotazione> MasterTable
        {
            get
            {
                if (masterTable == null)
                    masterTable = new List<V_Prenotazione>();
                return masterTable;
            }
            set { masterTable = value; OnPropertyChanged(this, "MasterTable"); }
        }

        public V_Prenotazione SelectedItem
        {
            get { return selectedItem; }
            set { selectedItem = value; OnPropertyChanged(this, "SelectedItem"); }
        }

        #endregion// Public Properties



        #region Constructors

        public pgBooking()
        {
            InitializeComponent();
            this.DataContext = this;
            Application.Current.MainWindow.SizeChanged += new SizeChangedEventHandler(MainWindow_SizeChanged);

            CommandEvent += new CommonEvents.ContentPageCommandEventHandler(pgPrenotations_CommandEvent);

            grdMaster.SelectionUnit = Telerik.Windows.Controls.GridView.GridViewSelectionUnit.FullRow;
            grdMaster.IsReadOnly = true;
            grdMaster.SelectionChanged += new EventHandler<Telerik.Windows.Controls.SelectionChangeEventArgs>(grdMaster_SelectionChanged);
            //grdMaster.MouseDoubleClick += new MouseButtonEventHandler(grdMaster_MouseDoubleClick);
            grdMaster.RowActivated += new EventHandler<RowEventArgs>(grdMaster_RowActivated);

            btnFilter.Click += new RoutedEventHandler(btnFilter_Click);
            btnCancel.Click += new RoutedEventHandler(btnCancel_Click);

            btnCancelReference.Click += new RoutedEventHandler(btnCancelReference_Click);
            btnCancelNumeroOrdine.Click += new RoutedEventHandler(btnCancelNumeroOrdine_Click);
            btnCancelDate1.Click += new RoutedEventHandler(btnCancelDate1_Click);
            btnCancelDate2.Click += new RoutedEventHandler(btnCancelDate2_Click);

            dtpDate1.SelectionChanged += new SelectionChangedEventHandler(dtpDate1_SelectionChanged);
            dtpDate2.SelectionChanged += new SelectionChangedEventHandler(dtpDate2_SelectionChanged);

            DateTime startDate = DateTime.Now.Date.Subtract(new TimeSpan(15, 0, 0, 0));
            DateTime endDate = DateTime.Now.Date.Add(new TimeSpan(15, 0, 0, 0));

            dtpDate1.SelectedDate = DateTime.Now.Date;

            DoFilter();

            cmbTipoMovimento.DataContext = this;

            Filter.AddSortField("Dt_Prenotazione");
            Filter.SortDirection = SortDirection.DESC;
            gridPager.PageSize = Helper_Dal.GetOptimalGridRows(true);
            BindMaster();
        }

        #endregion// Constructors



        #region Events

        void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            gridPager.PageSize = Helper_Dal.GetOptimalGridRows(true);
            BindMaster();
        }

        private void pgPrenotations_CommandEvent(ContentPageCommandEventArgs e)
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
                    if (MessageBox.Show("Eliminando le prenotazioni selezionate, tutte le visite ad esse associate andranno perdute e l'operazione non potrà essere annullata. Continuare?", "Eliminazione record", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                    {
                        DoDeleteItems();
                        BindMaster();
                        NotifySelection();
                    }
                    break;

                case ContentPageCommandType.Schedule:
                    DoScheduleToursMultiple();
                    break;

                case ContentPageCommandType.Filter:
                    Filter.Args = e.Filter.Args;
                    BindMaster();
                    NotifySelection();
                    break;

                case ContentPageCommandType.Other:
                    switch (e.CommandArgument)
                    {
                        case "extractpaymentsdata":
                            DoExtractPaymentsData();
                            break;
                    }
                    break;
            }
        }

        private void grdMaster_SelectionChanged(object sender, Telerik.Windows.Controls.SelectionChangeEventArgs e)
        {
            RadGridView grd = sender as RadGridView;
            if (grd != null)
            {
                if (grd.SelectedItems.Count > 0)
                    MasterID = (int)((V_Prenotazione)grd.SelectedItems[0]).Id_Prenotazione;

                NotifySelection();
            }
        }

        void grdMaster_RowActivated(object sender, RowEventArgs e)
        {
            DoScheduleToursMultiple();
        }

        private void grdMaster_FilterOperatorsLoading(object sender, Telerik.Windows.Controls.GridView.FilterOperatorsLoadingEventArgs e)
        {
            if (
                e.Column.UniqueName == "cognome"
                ||
                e.Column.UniqueName == "nome"
                ||
                e.Column.UniqueName == "visitatore"
            )
                e.DefaultOperator1 = Telerik.Windows.Data.FilterOperator.Contains;
        }

        private void gridPager_PageIndexChanged(object sender, PageIndexChangedEventArgs e)
        {
            grdMaster.CurrentItem = null;
        }

        private void grdMaster_GroupRowIsExpandedChanged(object sender, Telerik.Windows.Controls.GridView.GroupRowEventArgs e)
        {
            if (e.Row.IsExpanded)
                expandedGroups.Add(e.Row.Group);
            else
                if (expandedGroups.Contains(e.Row.Group))
                    expandedGroups.Remove(e.Row.Group);
        }

        private void btnFilter_Click(object sender, RoutedEventArgs e)
        {
            if (dtpDate1.SelectedDate != null && dtpDate2.SelectedDate != null)
            {
                if (dtpDate1.SelectedDate > dtpDate2.SelectedDate)
                {
                    MessageBox.Show("La data 'Dal' non può essere maggiore della data 'Al'.");
                    return;
                }
            }
            DoFilter();
            BindMaster();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DoCancelFilter();
            DoFilter();
            BindMaster();
        }

        private void btnCancelReference_Click(object sender, RoutedEventArgs e)
        {
            txtReference.Text = string.Empty;
            DoFilter();
            BindMaster();
        }

        private void btnCancelNumeroOrdine_Click(object sender, RoutedEventArgs e)
        {
            txtNumeroOrdine.Text = string.Empty;
            DoFilter();
            BindMaster();
        }

        private void btnCancelDate1_Click(object sender, RoutedEventArgs e)
        {
            dtpDate1.SelectedDate = null;
            DoFilter();
            BindMaster();
        }

        private void btnCancelDate2_Click(object sender, RoutedEventArgs e)
        {
            dtpDate2.SelectedDate = null;
            DoFilter();
            BindMaster();
        }

        void dtpDate1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtpDate1.SelectedValue != null && (dtpDate2.SelectedValue == null || dtpDate2.SelectedValue.Value.Date < dtpDate1.SelectedValue.Value.Date))
                dtpDate2.SelectedValue = dtpDate1.SelectedValue;
        }

        void dtpDate2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtpDate2.SelectedValue != null && (dtpDate1.SelectedValue == null || dtpDate1.SelectedValue.Value.Date > dtpDate2.SelectedValue.Value.Date))
                dtpDate1.SelectedValue = dtpDate2.SelectedValue;
        }

        #endregion// Events



        #region Context Menu

        private void contextMenu_Opening(object sender, RoutedEventArgs e)
        {
            int selectedItems = grdMaster.SelectedItems.Count;
            bool printable = false;

            mnuOpen.IsEnabled = false;
            mnuDispatch.IsEnabled = false;

            foreach (V_Prenotazione mm in grdMaster.SelectedItems)
                if (mm.Id_Prenotazione > 0)
                    printable = true;

            if (printable)
            {
                mnuOpen.IsEnabled = true;
                mnuDispatch.IsEnabled = true;
            }
        }

        private void contextMenu_Click(object sender, RadRoutedEventArgs e)
        {
            switch (((RadMenuItem)(e.Source)).CommandParameter.ToString())
            {
                case "open":
                    DoOpenItems();
                    break;

                case "dispatch":
                    DoScheduleToursMultiple();
                    break;

                case "delete":
                    if (MessageBox.Show("Eliminando le prenotazioni selezionate, tutte le visite ad esse associate andranno perdute e l'operazione non potrà essere annullata. Continuare?", "Eliminazione record", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
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
            MasterTable = dal.GetFilteredList_V_Prenotazione(Filter.Args, Filter.Sort, Filter.SortDirection.ToString(), Filter.PageSize, Filter.PageNumber, out count);
            grdMaster.ItemsSource = MasterTable;
            grdMaster.Rebind();
        }

        private void NotifySelection()
        {
            ContentPageCommandEventArgs args = new ContentPageCommandEventArgs();

            args.CommandType = ContentPageCommandType.SelectionChanged;

            for (int i = 0; i < grdMaster.SelectedItems.Count; i++)
                if ((int)((V_Prenotazione)grdMaster.SelectedItems[i]).Id_Prenotazione > 0)
                    args.SelectedIDs.Add((int)((V_Prenotazione)grdMaster.SelectedItems[i]).Id_Prenotazione);

            OnItemSelectionEvent(args);
        }

        private void DoNewItem()
        {
            frm = new wndPrenotationDetail(0);
            frm.User = User;
            frm.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            frm.DetailWindowClosing += new CommonDelegates.ClosingDetailWindowEventHandler(frm_DetailWindowClosing);
            frm.ShowDialog();
        }

        private void DoOpenItems()
        {
            if (grdMaster.SelectedItems.Count > 0)
                for (int i = 0; i < grdMaster.SelectedItems.Count; i++)
                    DoOpenItem(((V_Prenotazione)grdMaster.SelectedItems[i]), WindowStartupLocation.Manual);
        }

        private void DoOpenItem(V_Prenotazione obj, WindowStartupLocation startupLocation)
        {
            bool open = true;
            string windowName = string.Format("Prenotation_{0}", obj.Id_Prenotazione.ToString());
            frm = null;
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
                frm = new wndPrenotationDetail(obj.Id_Prenotazione);
                frm.User = User;
                frm.Name = windowName;
                frm.WindowStartupLocation = startupLocation;
                frm.DetailWindowClosing += new CommonDelegates.ClosingDetailWindowEventHandler(frm_DetailWindowClosing);
                frm.ShowDialog();
            }
        }

        void frm_DetailWindowClosing(object sender, ClosingDetailWindowEventArgs e)
        {
            //if (frm.SavedObj != null)
            //    RebindGrid();
            //else
            //    BindMaster();

            if (e.ProcessImmediately)
            {
                int count = 0;
                DoScheduleTours((V_Prenotazione)frm.SavedObj, WindowStartupLocation.CenterScreen);
                FillEmptyRows(gridPager.PageIndex, gridPager.PageSize);
            }
            else
            {
                NotifySelection();
            }
        }

        private void DoScheduleTours(V_Prenotazione obj, WindowStartupLocation startupLocation)
        {
            currentPageIndex = gridPager.PageIndex;
            string windowName = string.Format("Prenotation_{0}", obj.Id_Prenotazione.ToString());

            foreach (Window wnd in Application.Current.Windows)
            {
                if (wnd.Name == windowName && wnd.IsVisible)
                {
                    frm = (BaseDetailPage)wnd;
                    frm.Focus();
                }
            }
            frm = new wndScheduleTours(obj.Id_Prenotazione, User);
            frm.Name = windowName;
            frm.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            frm.CloseOnSave = true;
            if (frm.Openable)
            {
                frm.ShowDialog();
                if (frm.SavedObj != null)
                    RebindGrid();
                else
                    BindMaster();
            }
        }

        private void DoScheduleToursMultiple()
        {
            if (grdMaster.SelectedItems.Count > 0)
                for (int i = 0; i < grdMaster.SelectedItems.Count; i++)
                    DoScheduleTours(((V_Prenotazione)grdMaster.SelectedItems[i]), WindowStartupLocation.Manual);
        }

        private void RebindGrid()
        {
            bool found = false;
            for (int i = 0; i < MasterTable.Count; i++)
            {
                if (((V_Prenotazione)frm.SavedObj).Id_Prenotazione == MasterTable[i].Id_Prenotazione)
                {
                    MasterTable[i] = (V_Prenotazione)frm.SavedObj;
                    found = true;
                }
            }

            if (!found)
                MasterTable.Insert(0, (V_Prenotazione)frm.SavedObj);
            int count = 0;
            MasterTable = dal.GetFilteredList_V_Prenotazione(MasterTable, Filter.Args, Filter.Sort, Filter.SortDirection.ToString(), Filter.PageSize, Filter.PageNumber, out count); ;
            grdMaster.ItemsSource = MasterTable;
            grdMaster.Rebind();
            SelectedItem = (V_Prenotazione)frm.SavedObj;
            gridPager.PageIndex = currentPageIndex;
        }

        private void DoDeleteItems()
        {
            if (grdMaster.SelectedItems.Count > 0)
                for (int i = 0; i < grdMaster.SelectedItems.Count; i++)
                    DoDeleteItem(((V_Prenotazione)grdMaster.SelectedItems[i]));
        }

        private void DoDeleteItem(V_Prenotazione obj)
        {
            Prenotazione_Dal dal = new Prenotazione_Dal();
            try
            {
                Prenotazione g = dal.GetSingleItem(obj.Id_Prenotazione)[0];
                if (g != null)
                    dal.DeleteObject(g);
            }
            catch (UpdateException e)
            {
                MessageBox.Show("Impossibile eliminare: fra i record selezionati, alcuni hanno visite prenotate associate a una o più visite programmate.\nEliminare le visite programmate alle visite delle prenotazioni selezionate, quindi riprovare", "Errore eliminazione", MessageBoxButton.OK);
            }
        }

        private void DoFilter()
        {
            Filter.RemoveFilter("Protocollo");
            if (txtReference.Text.Length > 0)
                Filter.AddFilter("Protocollo", Utilities.ValueType.String, txtReference.Text.PadLeft(8, '0'));

            Filter.RemoveFilter("NumeroOrdine");
            if (txtNumeroOrdine.Text.Length > 0)
                Filter.AddFilter("NumeroOrdine", Utilities.ValueType.String, txtNumeroOrdine.Text);

            //Filtro su date
            List<object> dateValues = new List<object>();
            if (dtpDate1.SelectedDate != null)
                dateValues.Add(dtpDate1.SelectedDate.Value.Date);
            if (dtpDate2.SelectedDate != null)
                dateValues.Add(dtpDate2.SelectedDate.Value.Date);

            Filter.RemoveFilter("Dt_Prenotazione");

            if (dtpDate1.SelectedDate != null && dtpDate2.SelectedDate == null)
                Filter.SetFilter("Dt_Prenotazione", Utilities.ValueType.DateTime, dtpDate1.SelectedDate.Value.Date, Utilities.SQLOperator.GreaterThanEqual);

            if (dtpDate1.SelectedDate == null && dtpDate2.SelectedDate != null)
                Filter.SetFilter("Dt_Prenotazione", Utilities.ValueType.DateTime, dtpDate2.SelectedDate.Value.Date, Utilities.SQLOperator.LessThanEqual);

            if (dtpDate1.SelectedDate != null && dtpDate2.SelectedDate != null)
            {
                if (dtpDate1.SelectedDate != dtpDate2.SelectedDate)
                    Filter.SetFilter("Dt_Prenotazione", Utilities.ValueType.DateTime, dateValues, Utilities.SQLOperator.Between);
                else
                    Filter.SetFilter("Dt_Prenotazione", Utilities.ValueType.DateTime, dtpDate1.SelectedDate.Value.Date, Utilities.SQLOperator.Equal);
            }

            //Filtro su tipo movimento
            if (SelectedStatusTypeID != 0)
                Filter.SetFilter("Id_TipoRisposta", Utilities.ValueType.Int, SelectedStatusTypeID);
            else
                Filter.RemoveFilter("Id_TipoRisposta");
        }

        private void DoCancelFilter()
        {
            txtReference.Text = string.Empty;
            txtNumeroOrdine.Text = string.Empty;
            dtpDate1.SelectedDate = null;
            dtpDate2.SelectedDate = null;
            SelectedStatusTypeID = 0;
        }

        private void DoExtractPaymentsData()
        {
            frm = new wndExtractPaymentData();
            frm.User = User;
            frm.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            frm.DetailWindowClosing += new CommonDelegates.ClosingDetailWindowEventHandler(frm_DetailWindowClosing);
            frm.ShowDialog();
        }

        private void OnPropertyChanged(object sender, string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(sender, new PropertyChangedEventArgs(propertyName));
        }

        private void FillEmptyRows(int pageIndex, int pageSize)
        {
            int populatedRows = MasterTable.Count - (pageIndex * pageSize);
            for (int i = populatedRows; i < pageSize; i++)
                ((List<V_Prenotazione>)grdMaster.ItemsSource).Add(new V_Prenotazione());
        }

        #endregion// Private Methods


    }
}
