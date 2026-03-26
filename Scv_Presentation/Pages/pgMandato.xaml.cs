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
using System.Collections.ObjectModel;
using System.Configuration;

namespace Presentation.Pages
{
	/// <summary>
	/// Interaction logic for pgMandato.xaml
	/// </summary>
	public partial class pgMandato : BaseContentPage
	{
		#region Private Fields

		MandatoViewModel vm = null;

		#endregion// Private Fields

		#region Public Properties

		#endregion // Properties




		#region Constructors

		public pgMandato()
		{
			InitializeComponent();
			vm = new MandatoViewModel(DateTime.Now.Date);
            this.DataContext = vm;

 			CommandEvent += new CommonEvents.ContentPageCommandEventHandler(pgStores_CommandEvent);
			btnUdate.Click += new RoutedEventHandler(btnUdate_Click);
            btnRefreshOP.Click += new RoutedEventHandler(btnRefreshOP_Click);
			dtpDate.SelectionChanged += new SelectionChangedEventHandler(dtpDate_SelectionChanged);
		}


		#endregion




		#region Events

		void pgStores_CommandEvent(ContentPageCommandEventArgs e)
		{
			switch (e.CommandType)
			{
				case ContentPageCommandType.Print:
					DoPrint(e.CommandArgument);
					break;
			}
		}

		void frm_Closed(object sender, EventArgs e)
		{
			NotifySelection();
		}

		void grdMaster_SelectionChanged(object sender, Telerik.Windows.Controls.SelectionChangeEventArgs e)
		{
			RadGridView grd = sender as RadGridView;
			if (grd != null)
			{
				NotifySelection();
			}
		}

		private void grdMaster_RowLoaded(object sender, Telerik.Windows.Controls.GridView.RowLoadedEventArgs e)
		{
			MandatoDettaglio o = e.DataElement as MandatoDettaglio;
			if (o != null)
			{
				if (o.IsNumberReadOnly)
					e.Row.Cells[1].IsEnabled = false;
				if (o.IsValueReadOnly)
					e.Row.Cells[2].IsEnabled = false;
			}
		}

		private void grdDetails_BeginningEdit(object sender, GridViewBeginningEditRoutedEventArgs e)
		{

		}

		void dtpDate_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			vm.LoadData((DateTime)e.AddedItems[0]);
 		}

		void btnUdate_Click(object sender, RoutedEventArgs e)
		{
			vm.LoadArgs();
			decimal totale = vm.ObjMandatoDettaglio.Sum(x => x.Valore);
			if (vm.Args.TotaleCassa != totale)
				if (MessageBox.Show("Il totale dei tagli (" + totale.ToString("C") + ") non corrisponde al totale cassa (" + vm.Args.TotaleCassa.ToString("C") + ").\nSalvare ugualmente?", "Errore conteggio", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
					return;

			Mandato_Dal dalMandato = new Mandato_Dal();
			dalMandato.InsertOrUpdate(vm.ObjMandato, vm.ObjMandatoDettaglio.ToList(), dtpDate.SelectedDate.Value.Date);
			vm.LoadData(dtpDate.SelectedDate.Value);
		}

        void btnRefreshOP_Click(object sender, RoutedEventArgs e)
        {
            vm.LoadArgs();
            Mandato_Dal dalMandato = new Mandato_Dal();
            dalMandato.InsertOrUpdate(vm.ObjMandato, vm.ObjMandatoDettaglio.ToList(), dtpDate.SelectedDate.Value.Date,vm.OnlinePaymentEnabled,true);
            vm.LoadData(dtpDate.SelectedDate.Value);
        }
        



		#endregion// Events




		#region Context Menu

		private void contextMenu_Opening(object sender, RoutedEventArgs e)
		{
			int selectedItems = grdMaster.SelectedItems.Count;
			//mnuOpen.IsEnabled = false;
			//mnuDelete.IsEnabled = false;
			//if (selectedItems > 0)
			//{
			//    mnuOpen.IsEnabled = true;
			//    mnuDelete.IsEnabled = true;
			//}
		}

		private void contextMenu_Click(object sender, RadRoutedEventArgs e)
		{
		}

		#endregion// Context Menu



		#region Private Methods


		private void NotifySelection()
		{
			ContentPageCommandEventArgs args = new ContentPageCommandEventArgs();

			args.CommandType = ContentPageCommandType.SelectionChanged;

			for (int i = 0; i < grdMaster.SelectedItems.Count; i++)
				args.SelectedIDs.Add((int)((MandatoDettaglio)grdMaster.SelectedItems[i]).Id_MandatoDettagli);

			OnItemSelectionEvent(args);
		}

		private void DoPrint(string argument)
		{
			string windowName = "Mandato";
			BasePrintPage frm = null;
			vm.LoadArgs();

			switch (argument)
			{
				case "printufficioscavi":
					frm = new wndPrintMandatoUfficioScavi();
					break;

				case "printamministrazione":
					frm = new wndPrintMandatoAmministrazione();
					break;
			}

			frm.Name = windowName;
			frm.DsPrintMandato = new List<PrintMandatoArgs>() { vm.Args };
            frm.DsPrintTipiVisitaImporti = vm.DsPrintTipiVisitaImporti;
			frm.WindowStartupLocation = WindowStartupLocation.CenterOwner;
			frm.Closed += new EventHandler(frm_Closed);
			frm.ShowDialog();
		}

		#endregion// Private Methods

	}
}
