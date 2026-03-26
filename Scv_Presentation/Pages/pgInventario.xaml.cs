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
using Scv_Entities;
using Telerik.Windows.Controls;
using System.Data;
using Telerik.Windows;

namespace Presentation.Pages
{
	/// <summary>
	/// Interaction logic for pgInventario.xaml
	/// </summary>
	public partial class pgInventario : BaseContentPage
	{
		#region Private Fields

		Movimento_Dal dal = new Movimento_Dal();

		private List<V_Articoli> masterTable = null;

		#endregion// Private Fields




		#region Public Properties

		public List<V_Articoli> MasterTable
		{
			get
			{
				if (masterTable == null)
					masterTable = new List<V_Articoli>();
				return masterTable;
			}
			set { masterTable = value; }
		}

		#endregion// Public properties



		#region Constructors

		public pgInventario()
		{
			InitializeComponent();
			CommandEvent += new CommonEvents.ContentPageCommandEventHandler(pgGuides_CommandEvent);

			grdMaster.SelectionUnit = Telerik.Windows.Controls.GridView.GridViewSelectionUnit.FullRow;
			grdMaster.IsReadOnly = true;
			BindMaster();
		}

		#endregion// Constructors



		#region Events

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



		#endregion// Events



		#region Private Methods

		private void BindMaster()
		{
			MasterTable = dal.GetInventario();
			grdMaster.DataContext = MasterTable;
		}

		private void NotifySelection()
		{
			ContentPageCommandEventArgs args = new ContentPageCommandEventArgs();

			args.CommandType = ContentPageCommandType.SelectionChanged;

			for (int i = 0; i < grdMaster.SelectedItems.Count; i++)
				args.SelectedIDs.Add((int)((V_Articoli)grdMaster.SelectedItems[i]).Id_Articolo);

			OnItemSelectionEvent(args);
		}

		private void DoPrint()
		{
			string windowName = "Movimenti";
			BasePrintPage frm = null;
			frm = new wndPrintInventario();
			frm.DsInventario = MasterTable;
			frm.Name = windowName;
			frm.WindowStartupLocation = WindowStartupLocation.CenterOwner;
			frm.Closed += new EventHandler(frm_Closed);
			frm.ShowDialog();
		}



		#endregion// Private Methods

	}
}
