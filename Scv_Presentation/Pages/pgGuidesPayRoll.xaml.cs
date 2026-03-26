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
using Telerik.Windows.Data;
using Scv_Model.Common;

namespace Presentation.Pages
{
	/// <summary>
	/// Interaction logic for pgGuidesPayRoll.xaml
	/// </summary>
	public partial class pgGuidesPayRoll : BaseContentPage, INotifyPropertyChanged
	{
		#region Variables

		MandatoGuida_Dal dal = new MandatoGuida_Dal();

		private List<YearItem> years = null;

		private List<MonthItem> months = null;

		private YearItem selectedYear = null;

		private MonthItem selectedMonth = null;

		private bool groupByColleges = true;

		private int selectedGuideID = 0;

		private List<V_Guide> availableGuides = null;

		private List<PayRollPrintType> availablePayRollPrintTypes = null;

		private int selectedPayRollPrintTypeID = 0;

		private List<V_MandatoGuida> masterTable = null;

		private List<MoneyCut> moneyCuts = new List<MoneyCut>();

		private decimal total = 0;

		#endregion



		#region Public Properties

		public List<V_MandatoGuida> MasterTable
		{
			get
			{
				if (masterTable == null)
					masterTable = new List<V_MandatoGuida>();
				return masterTable;
			}
			set { masterTable = value; }
		}

		public List<YearItem> Years
		{
			get
			{
				if (years == null)
					years = Helper_Dal.GetYears(2000, DateTime.Now.Year);
				return years;
			}
		}

		public List<MonthItem> Months
		{
			get
			{
				if (months == null)
					months = Helper_Dal.GetMonths();
				return months;
			}
		}

		public bool GroupByColleges
		{
			get { return groupByColleges; }
			set { groupByColleges = value; OnPropertyChanged(this, "GroupByColleges"); }
		}

		public YearItem SelectedYear
		{
			get
			{
				if (selectedYear == null)
					selectedYear = new YearItem();
				return selectedYear;
			}
			set { selectedYear = value; OnPropertyChanged(this, "SelectedYear"); }
		}

		public MonthItem SelectedMonth
		{
			get
			{
				if (selectedMonth == null)
					selectedMonth = new MonthItem();
				return selectedMonth;
			}
			set { selectedMonth = value; OnPropertyChanged(this, "SelectedMonth"); }
		}

		public int SelectedGuideID
		{
			get
			{
				return selectedGuideID;
			}
			set { selectedGuideID = value; OnPropertyChanged(this, "SelectedGuideID"); }
		}

		public int SelectedPayRollPrintTypeID
		{
			get
			{
				return selectedPayRollPrintTypeID;
			}
			set { selectedPayRollPrintTypeID = value; OnPropertyChanged(this, "SelectedPayRollPrintTypeID"); }
		}

		public List<V_Guide> AvailableGuides
		{
			get
			{
				if (availableGuides == null)
				{
					Guida_Dal dal = new Guida_Dal();
					availableGuides = dal.GetList().Where(x => x.Fl_Attivo == true).ToList();
					V_Guide obj = new V_Guide();
					obj.Id_Guida = 0;
					obj.Cognome = "Tutte";
					availableGuides.Insert(0, obj);
				}
				return availableGuides;
			}
		}

		public List<PayRollPrintType> AvailablePayRollPrintTypes
		{
			get
			{
				if (availablePayRollPrintTypes == null)
				{
					availablePayRollPrintTypes = new List<PayRollPrintType>();
					PayRollPrintType obj = null;

					obj = new PayRollPrintType();
					obj.ID = 1;
					obj.Description = "Scontrini";
					availablePayRollPrintTypes.Add(obj);

					obj = new PayRollPrintType();
					obj.ID = 2;
					obj.Description = "Buste";
					availablePayRollPrintTypes.Add(obj);

				}
				return availablePayRollPrintTypes;
			}
		}

		public List<MoneyCut> MoneyCuts
		{
			get
			{
				if (moneyCuts == null)
					moneyCuts = new List<MoneyCut>();
				return moneyCuts;
			}
			set { moneyCuts = value; }
		}

		public decimal Total
		{
			get { return total; }
			set { total = value; OnPropertyChanged(this, "Total"); }
		}

		#endregion// Public Properties



		#region Constructors

		public pgGuidesPayRoll()
		{
			DataContext = this;
			InitializeComponent();

			MoneyCuts.Add(new MoneyCut(50));
			MoneyCuts.Add(new MoneyCut(20));
			MoneyCuts.Add(new MoneyCut(10));

			SelectedYear.PropertyChanged += new PropertyChangedEventHandler(SelectedYear_PropertyChanged);
			SelectedMonth.PropertyChanged += new PropertyChangedEventHandler(SelectedMonth_PropertyChanged);

			btnFilter.Click += new RoutedEventHandler(btnFilter_Click);
			btnCancel.Click += new RoutedEventHandler(btnCancel_Click);

			SelectedYear.Number = DateTime.Now.Year;
			SelectedMonth.Number = DateTime.Now.Month;

			cmbYear.DataContext = this;
			cmbMonth.DataContext = this;
			cmbGuide.DataContext = this;
			cmbPrintWhat.DataContext = this;

			Filter.AddSortField("Cognome");
			Filter.AddSortField("Nome");

			BindYears();
			BindMonths();

			//SelectedGuideID = 0;
			SelectedPayRollPrintTypeID = 1;

			CommandEvent += new CommonEvents.ContentPageCommandEventHandler(pgGuides_CommandEvent);

			grdMaster.SelectionUnit = Telerik.Windows.Controls.GridView.GridViewSelectionUnit.FullRow;
			grdMaster.IsReadOnly = true;
			grdMaster.SelectionChanged += new EventHandler<Telerik.Windows.Controls.SelectionChangeEventArgs>(grdMaster_SelectionChanged);
			grdMaster.MouseDoubleClick += new MouseButtonEventHandler(grdMaster_MouseDoubleClick);
			grdMaster.Grouped += new EventHandler<GridViewGroupedEventArgs>(grdMaster_Grouped);

		}

		#endregion// Constructors



		#region Events

		void pgGuides_CommandEvent(ContentPageCommandEventArgs e)
		{
			switch (e.CommandType)
			{
				case ContentPageCommandType.Open:
					DoOpenItems();
					break;

				case ContentPageCommandType.Print:
					switch (e.CommandArgument)
					{

						case "printpayrollsynthesis":
							DoPrintPayrollSynthesis();
							break;

						case "printpayrollsynthesisforsigns":
							DoPrintPayrollSynthesisForSigns();
							break;

						case "printallpayroll":
							DoPrintAllPayroll();
							break;
					}
					break;
			}
		}

		void frm_Closed(object sender, EventArgs e)
		{
			NotifySelection();
		}

		void grdMaster_SelectionChanged(object sender, Telerik.Windows.Controls.SelectionChangeEventArgs e)
		{
			//RadGridView grd = sender as RadGridView;
			//if (grd != null)
			//{
			//    if (grd.SelectedItems.Count > 0)
			//        MasterID = (int)((V_MandatoGuida)grd.SelectedItems[0]).Id_Guida;

			//    NotifySelection();
			//}
		}

		void grdMaster_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			//RadGridView grd = sender as RadGridView;
			//if (grd != null)
			//    if (grd.SelectedItems.Count > 0)
			//        DoOpenItem(((V_MandatoGuida)grd.SelectedItems[0]), WindowStartupLocation.CenterScreen);
		}

		void grdMaster_Grouped(object sender, GridViewGroupedEventArgs e)
		{
			grdMaster.CollapseAllGroups();
		}

		void SelectedYear_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			OnPropertyChanged(sender, "SelectedYear");
		}

		void SelectedMonth_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			OnPropertyChanged(sender, "SelectedMonth");
		}

		void btnFilter_Click(object sender, RoutedEventArgs e)
		{
			DoFilter();
		}

		void btnCancel_Click(object sender, RoutedEventArgs e)
		{
			DoCancelFilter();
			DoFilter();
		}

		#endregion// Events



		#region Private Methods

		private void BindYears()
		{
			//cmbYear.ItemsSource = DalHelper.GetYears(2000, DateTime.Now.Year);
			//cmbYear.DisplayMemberPath = "Description";
			//cmbYear.SelectedValuePath = "Number";
		}

		private void BindMonths()
		{
			//cmbMonth.ItemsSource = DalHelper.GetMonths();
			//cmbMonth.DisplayMemberPath = "Description";
			//cmbMonth.SelectedValuePath = "Number";
		}

		private void BindMaster(int year, int month, bool groupByColleges)
		{
			MoneyCuts.ForEach(x => x.Pieces = 0);
			MasterTable = dal.GetFilteredList_V_MandatoGuida(Filter, year, month, ref moneyCuts, groupByColleges);
			grdMaster.DataContext = MasterTable;
			Total = moneyCuts.Sum(x => x.Total);
			OnPropertyChanged(this, "MoneyCuts");
		}

		private void NotifySelection()
		{
			ContentPageCommandEventArgs args = new ContentPageCommandEventArgs();

			args.CommandType = ContentPageCommandType.SelectionChanged;

			for (int i = 0; i < grdMaster.SelectedItems.Count; i++)
				args.SelectedIDs.Add((int)((V_MandatoGuida)grdMaster.SelectedItems[i]).Id_Guida);

			OnItemSelectionEvent(args);
		}

		private void DoOpenItems()
		{
			if (grdMaster.SelectedItems.Count > 0)
				for (int i = 0; i < grdMaster.SelectedItems.Count; i++)
					DoOpenItem(((V_MandatoGuida)grdMaster.SelectedItems[i]), WindowStartupLocation.Manual);
		}

		private void DoOpenItem(V_MandatoGuida obj, WindowStartupLocation startupLocation)
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
					DoDeleteItem(((V_MandatoGuida)grdMaster.SelectedItems[i]));
		}

		private void DoDeleteItem(V_MandatoGuida obj)
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

		private void DoPrintPayrollSynthesis()
		{
			bool open = true;
			string windowName = "Mandati";
			BasePrintPage frm = null;
			foreach (Window wnd in Application.Current.Windows)
			{
				if (wnd.Name == windowName && wnd.IsVisible)
				{

					frm = (BasePrintPage)wnd;
					frm.Focus();
					open = false;
				}
			}
			if (open)
			{
				string monthName = string.Empty;

				switch (SelectedMonth.Number)
				{
					case 1:
						monthName = "Gennaio";
						break;
					case 2:
						monthName = "Febbraio";
						break;
					case 3:
						monthName = "Marzo";
						break;
					case 4:
						monthName = "Aprile";
						break;
					case 5:
						monthName = "Maggio";
						break;
					case 6:
						monthName = "Giugno";
						break;
					case 7:
						monthName = "Luglio";
						break;
					case 8:
						monthName = "Agosto";
						break;
					case 9:
						monthName = "Settembre";
						break;
					case 10:
						monthName = "Ottobre";
						break;
					case 11:
						monthName = "Novembre";
						break;
					case 12:
						monthName = "Dicembre";
						break;
				}

				frm = new wndPrintGuidesPayroll();
				frm.Name = windowName;
				frm.DsGuidesPayroll = MasterTable;
				PrintGuidePayrollArgs args = new PrintGuidePayrollArgs();
				args.Cut1 = MoneyCuts[0].Cut;
				args.Cut2 = MoneyCuts[1].Cut;
				args.Cut3 = MoneyCuts[2].Cut;
				args.Cut1Pieces = MoneyCuts[0].Pieces;
				args.Cut2Pieces = MoneyCuts[1].Pieces;
				args.Cut3Pieces = MoneyCuts[2].Pieces;
				args.Cut1Total = MoneyCuts[0].Total;
				args.Cut2Total = MoneyCuts[1].Total;
				args.Cut3Total = MoneyCuts[2].Total;
				args.MainTotal = Total;
				args.MonthName = SelectedMonth.Description;
				args.YearNumber = SelectedYear.Number.ToString();
				frm.PrintGuidePayrollArgs = args;
				frm.PrintGuidePayrollArgs.MonthName = monthName;
				frm.PrintGuidePayrollArgs.YearNumber = selectedYear.Number.ToString();
				frm.WindowStartupLocation = WindowStartupLocation.CenterOwner;
				frm.Closed += new EventHandler(frm_Closed);
				frm.ShowDialog();
			}
		}

		private void DoPrintPayrollSynthesisForSigns()
		{
			bool open = true;
			string windowName = "Mandati";
			BasePrintPage frm = null;
			foreach (Window wnd in Application.Current.Windows)
			{
				if (wnd.Name == windowName && wnd.IsVisible)
				{
					frm = (BasePrintPage)wnd;
					frm.Focus();
					open = false;
				}
			}
			if (open)
			{
				string monthName = string.Empty;

				switch (SelectedMonth.Number)
				{
					case 1:
						monthName = "Gennaio";
						break;
					case 2:
						monthName = "Febbraio";
						break;
					case 3:
						monthName = "Marzo";
						break;
					case 4:
						monthName = "Aprile";
						break;
					case 5:
						monthName = "Maggio";
						break;
					case 6:
						monthName = "Giugno";
						break;
					case 7:
						monthName = "Luglio";
						break;
					case 8:
						monthName = "Agosto";
						break;
					case 9:
						monthName = "Settembre";
						break;
					case 10:
						monthName = "Ottobre";
						break;
					case 11:
						monthName = "Novembre";
						break;
					case 12:
						monthName = "Dicembre";
						break;
				}

				frm = new wndPrintGuidesPayrollForSigns();
				frm.Name = windowName;
				frm.DsGuidesPayroll = MasterTable;
				PrintGuidePayrollArgs args = new PrintGuidePayrollArgs();
				args.Cut1 = MoneyCuts[0].Cut;
				args.Cut2 = MoneyCuts[1].Cut;
				args.Cut3 = MoneyCuts[2].Cut;
				args.Cut1Pieces = MoneyCuts[0].Pieces;
				args.Cut2Pieces = MoneyCuts[1].Pieces;
				args.Cut3Pieces = MoneyCuts[2].Pieces;
				args.Cut1Total = MoneyCuts[0].Total;
				args.Cut2Total = MoneyCuts[1].Total;
				args.Cut3Total = MoneyCuts[2].Total;
				args.MainTotal = Total;
				args.MonthName = SelectedMonth.Description;
				args.YearNumber = SelectedYear.Number.ToString();
				frm.PrintGuidePayrollArgs = args;
				frm.PrintGuidePayrollArgs.MonthName = monthName;
				frm.PrintGuidePayrollArgs.YearNumber = SelectedYear.Number.ToString();
				frm.WindowStartupLocation = WindowStartupLocation.CenterOwner;
				frm.Closed += new EventHandler(frm_Closed);
				frm.ShowDialog();
			}
		}

		private void DoPrintAllPayroll()
		{
			bool open = true;
			string windowName = "Mandati";
			BasePrintPage frm = null;
			foreach (Window wnd in Application.Current.Windows)
			{
				if (wnd.Name == windowName && wnd.IsVisible)
				{

					frm = (BasePrintPage)wnd;
					frm.Focus();
					open = false;
				}
			}
			if (open)
			{
				string monthName = string.Empty;

				switch (SelectedMonth.Number)
				{
					case 1:
						monthName = "Gennaio";
						break;
					case 2:
						monthName = "Febbraio";
						break;
					case 3:
						monthName = "Marzo";
						break;
					case 4:
						monthName = "Aprile";
						break;
					case 5:
						monthName = "Maggio";
						break;
					case 6:
						monthName = "Giugno";
						break;
					case 7:
						monthName = "Luglio";
						break;
					case 8:
						monthName = "Agosto";
						break;
					case 9:
						monthName = "Settembre";
						break;
					case 10:
						monthName = "Ottobre";
						break;
					case 11:
						monthName = "Novembre";
						break;
					case 12:
						monthName = "Dicembre";
						break;
				}

				if (SelectedPayRollPrintTypeID == 1)
					frm = new wndPrintGuidesPayRollReceipts();
				else
					frm = new wndPrintGuidesPayRollEnvelopes();

				frm.Name = windowName;
				GuidePayRollPrintItem item = null;
				int totaleNecropoliNum = 0;
				int totaleAltroNum = 0;
				decimal totaleNecropoli = 0;
				decimal totaleAltro = 0;
				decimal totaleCompplessivo = 0;
				decimal totaleSaldo = 0;
				decimal unitCost = 0;
				decimal.TryParse(new Parametri_Dal().GetItem("compenso_guida").Valore, out unitCost);

				List<MoneyCut> mCuts = new List<MoneyCut>();
				mCuts.Add(new MoneyCut(50));
				mCuts.Add(new MoneyCut(20));
				mCuts.Add(new MoneyCut(10));

				List<GuidePayRollPrintItem> list = new List<GuidePayRollPrintItem>();

				foreach (V_MandatoGuida obj in masterTable)
				{
					if (obj.Saldo > 0)
					{
						totaleNecropoliNum = obj.NrNecropoli != null ? (int)obj.NrNecropoli : 0;
						totaleNecropoli = obj.TotaleComplessivo != null ? (decimal)obj.TotaleComplessivo : 0;
						totaleAltroNum = obj.NrAltro != null ? (int)obj.NrAltro : 0;
						totaleAltro = obj.TotaleAltro != null ? (decimal)obj.TotaleAltro : 0;
						totaleCompplessivo = obj.TotaleComplessivo;
						totaleSaldo = obj.Saldo;
						mCuts.ForEach(x => x.Pieces = 0);
						mCuts = MoneyCut_Dal.GetCuts(mCuts, (int)obj.Saldo);

						item = new GuidePayRollPrintItem();
						item.AttendancesNumber = totaleNecropoliNum + totaleAltroNum;
						item.Date = SelectedMonth.Description + "/" + SelectedYear.Description;
						item.Guide = obj.Nominativo;
						item.Pieces50Number = mCuts[0].Pieces;
						item.Pieces50Total = mCuts[0].Total;
						item.Pieces20Number = mCuts[1].Pieces;
						item.Pieces20Total = mCuts[1].Total;
						item.Pieces10Number = mCuts[2].Pieces;
						item.Pieces10Total = mCuts[2].Total;
						item.UnitCost = unitCost;
						item.Total = totaleSaldo;

						list.Add(item);
					}
				}

				if (list.Count == 0)
				{
					MessageBox.Show("Nessuna busta paga trovata con i criteri di ricerca specificati.");
					return;
				}

				frm.DsPayRollPrintItems = list;
				PrintGuidePayrollArgs args = new PrintGuidePayrollArgs();
			
				frm.WindowStartupLocation = WindowStartupLocation.CenterOwner;
				frm.Closed += new EventHandler(frm_Closed);
				frm.ShowDialog();
			}
		}



		#endregion// Private Methods

		private void DoFilter()
		{
			//Filtro sulla guida
			if (SelectedGuideID != 0)
				Filter.SetFilter("Id_Guida", Utilities.ValueType.Int, SelectedGuideID);
			else
				Filter.RemoveFilter("Id_Guida");

			BindMaster(SelectedYear.Number, SelectedMonth.Number, GroupByColleges);

		}

		private void DoCancelFilter()
		{
			SelectedGuideID = 0;
		}


		public event PropertyChangedEventHandler PropertyChanged;
		private void OnPropertyChanged(object sender, string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));

			switch (propertyName)
			{
				case "SelectedYear":
				case "SelectedMonth":
				case "GroupByColleges":
					BindMaster(SelectedYear.Number, SelectedMonth.Number, GroupByColleges);
					break;
			}
		}
	}
}
