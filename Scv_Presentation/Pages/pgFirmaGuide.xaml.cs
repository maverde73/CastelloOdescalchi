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
	/// Interaction logic for pgFirmaGuide.xaml
	/// </summary>
	public partial class pgFirmaGuide : BaseContentPage, INotifyPropertyChanged
	{
		#region Public Events

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion// Public Events



		#region Private Fields

		Guida_Dal dal = new Guida_Dal();

		private List<V_GuideIncaricate> masterTable = null;

		private DateTime visitsDate;

		#endregion// Private Fields



		#region Public Properties

		public List<V_GuideIncaricate> MasterTable
		{
			get
			{
				if (masterTable == null)
					masterTable = new List<V_GuideIncaricate>();
				return masterTable;
			}
			set { masterTable = value; OnPropertyChanged(this, "MasterTable"); }
		}

		public DateTime VisitsDate
		{
			get { return visitsDate; }
			set { visitsDate = value; OnPropertyChanged(this, "VisitsDate"); }
		}
		#endregion// Public Properties



		#region Constructors

		public pgFirmaGuide()
		{
			InitializeComponent();

			DataContext = this;

			VisitsDate = DateTime.Now.Date;

			CommandEvent += new CommonEvents.ContentPageCommandEventHandler(pgGuides_CommandEvent);

			grdMaster.SelectionUnit = Telerik.Windows.Controls.GridView.GridViewSelectionUnit.FullRow;
			grdMaster.IsReadOnly = true;

			//BindMaster();
		}

		#endregion



		#region Event Handlers

		void pgGuides_CommandEvent(ContentPageCommandEventArgs e)
		{
			switch (e.CommandType)
			{
				case ContentPageCommandType.Print:
					DoPrint();
					break;
			}
		}

		void frm_Closed(object sender, EventArgs e)
		{
			BindMaster();
			NotifySelection();
		}

		void OnPropertyChanged(object sender, string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(sender, new PropertyChangedEventArgs(propertyName));

			switch (propertyName)
			{
				case "VisitsDate":
					BindMaster();
					break;
			}
		}

		#endregion// Event Handlers




		#region Private Methods

		private void BindMaster()
		{
			MasterTable = dal.GetGuideIncaricate(VisitsDate);

			grdMaster.DataContext = MasterTable;
		}

		private void NotifySelection()
		{
			ContentPageCommandEventArgs args = new ContentPageCommandEventArgs();

			args.CommandType = ContentPageCommandType.SelectionChanged;

			for (int i = 0; i < grdMaster.SelectedItems.Count; i++)
				args.SelectedIDs.Add((int)((V_Guide)grdMaster.SelectedItems[i]).Id_Guida);

			OnItemSelectionEvent(args);
		}

		private void DoPrint()
		{
			string windowName = "Firma_guide";
			BasePrintPage frm = null;

			FirmaGuideArgs args = new FirmaGuideArgs();
			args.VisitsDate = VisitsDate.ToLongDateString();

			frm = new wndPrintFirmaGuide();
			frm.Name = windowName;
			frm.DsGuideIncaricate = MasterTable;
			frm.FirmaGuideArgs = args;
			frm.WindowStartupLocation = WindowStartupLocation.CenterOwner;
			frm.Closed += new EventHandler(frm_Closed);
			frm.ShowDialog();
		}

		#endregion// Private Methods
	}
}
