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


namespace Presentation.Pages
{
	/// <summary>
	/// Interaction logic for pgWarehouseMovementsToolBar.xaml
	/// </summary>
	public partial class pgPrivateSellMovementsToolBar : BaseToolBarPage
	{
		public pgPrivateSellMovementsToolBar()
		{
			InitializeComponent();
			rib.CollapseThresholdSize = new Size(1, 1);
			btnNew.Click += new RoutedEventHandler(btnNew_Click);
			//btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
			btnPrint.Click += new RoutedEventHandler(btnPrint_Click);
			btnPrintAll.Click += new RoutedEventHandler(btnPrintAll_Click);
			btnPrintList.Click += new RoutedEventHandler(btnPrintList_Click);
			btnChangeMovementPaymentType.Click += new RoutedEventHandler(btnChangeMovementPaymentType_Click);

			//btnDelete.IsEnabled = false;
			btnPrint.IsEnabled = false;
			btnChangeMovementPaymentType.IsEnabled = false;
		}

		void btnNew_Click(object sender, RoutedEventArgs e)
		{
			ContentPageCommandEventArgs args = new ContentPageCommandEventArgs();

			args.CommandType = ContentPageCommandType.New;
			args.PageTemplateName = "pgPrivateSellMovements.xaml";
			MenuCommand(args);
		}

		void btnDelete_Click(object sender, RoutedEventArgs e)
		{
			ContentPageCommandEventArgs args = new ContentPageCommandEventArgs();

			args.CommandType = ContentPageCommandType.Delete;
			args.PageTemplateName = "pgPrivateSellMovements.xaml";
			MenuCommand(args);
		}

		void btnPrint_Click(object sender, RoutedEventArgs e)
		{
			ContentPageCommandEventArgs args = new ContentPageCommandEventArgs();
			args.CommandArgument = "print";

			args.CommandType = ContentPageCommandType.Print;
			args.PageTemplateName = "pgPrivateSellMovements.xaml";
			MenuCommand(args);
		}

		void btnPrintAll_Click(object sender, RoutedEventArgs e)
		{
			ContentPageCommandEventArgs args = new ContentPageCommandEventArgs();
			args.CommandArgument = "printall";

			args.CommandType = ContentPageCommandType.Print;
			args.PageTemplateName = "pgPrivateSellMovements.xaml";
			MenuCommand(args);
		}

		void btnPrintList_Click(object sender, RoutedEventArgs e)
		{
			ContentPageCommandEventArgs args = new ContentPageCommandEventArgs();
			args.CommandArgument = "printlist";

			args.CommandType = ContentPageCommandType.Print;
			args.PageTemplateName = "pgPrivateSellMovements.xaml";
			MenuCommand(args);
		}

		void btnChangeMovementPaymentType_Click(object sender, RoutedEventArgs e)
		{
			ContentPageCommandEventArgs args = new ContentPageCommandEventArgs();
			args.CommandArgument = "changemovementpaymenttype";

			args.CommandType = ContentPageCommandType.Update;
			args.PageTemplateName = "pgWarehouseMovements.xaml";
			MenuCommand(args);
		}

		void btnPrintDeliveredBills_Click(object sender, RoutedEventArgs e)
		{
			ContentPageCommandEventArgs args = new ContentPageCommandEventArgs();
			args.CommandArgument = "printdeliveredbills";

			args.CommandType = ContentPageCommandType.Print;
			args.PageTemplateName = "pgPrivateSellMovements.xaml";
			MenuCommand(args);
		}

		void btnPrintInvoices_Click(object sender, RoutedEventArgs e)
		{
			ContentPageCommandEventArgs args = new ContentPageCommandEventArgs();
			args.CommandArgument = "printinvoices";

			args.CommandType = ContentPageCommandType.Print;
			args.PageTemplateName = "pgPrivateSellMovements.xaml";
			MenuCommand(args);
		}

		protected override void MenuCommand(ContentPageCommandEventArgs e)
		{
			if (OnMenuCommand != null)
				OnMenuCommand(this, e);
		}

		public override void SetButtons(ContentPageCommandEventArgs e)
		{
			//btnDelete.IsEnabled = e.SelectedIDs.Count > 0;
			btnPrint.IsEnabled = e.SelectedIDs.Count > 0;

			Movimento_Dal dal = new Movimento_Dal();
			bool isSelectable = true;
			foreach (int i in e.SelectedIDs)
				if (!dal.GetItem(i).IsMoney)
					isSelectable = false;
			btnChangeMovementPaymentType.IsEnabled = false;
			if (isSelectable)
				btnChangeMovementPaymentType.IsEnabled = e.SelectedIDs.Count > 0;		
		}
	}
}
