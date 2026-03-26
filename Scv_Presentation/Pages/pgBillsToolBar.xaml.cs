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

namespace Presentation.Pages
{
	/// <summary>
	/// Interaction logic for pgBillsToolBar.xaml
	/// </summary>
	public partial class pgBillsToolBar : BaseToolBarPage
	{
		public pgBillsToolBar()
		{
			InitializeComponent();
			rib.CollapseThresholdSize = new Size(1, 1);
			btnPrintAll.Click += new RoutedEventHandler(btnPrintAll_Click);
			btnPrintSelected.Click += new RoutedEventHandler(btnPrintSelected_Click);
			btnPrintSelected.IsEnabled = false;
		}

		void btnPrintAll_Click(object sender, RoutedEventArgs e)
		{
			ContentPageCommandEventArgs args = new ContentPageCommandEventArgs();
			args.CommandArgument = "printAll";

			args.CommandType = ContentPageCommandType.Print;
			args.PageTemplateName = "pgBills.xaml";
			MenuCommand(args);
		}

		void btnPrintSelected_Click(object sender, RoutedEventArgs e)
		{
			ContentPageCommandEventArgs args = new ContentPageCommandEventArgs();
			args.CommandArgument = "printSelected";

			args.CommandType = ContentPageCommandType.Print;
			args.PageTemplateName = "pgBills.xaml";
			MenuCommand(args);
		}

		protected override void MenuCommand(ContentPageCommandEventArgs e)
		{
			if (OnMenuCommand != null)
				OnMenuCommand(this, e);
		}

		public override void SetButtons(ContentPageCommandEventArgs e)
		{
			btnPrintSelected.IsEnabled = e.SelectedIDs.Count > 0;
		}
	}
}
