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
	/// Interaction logic for pgGuidesPayRollToolBar.xaml
	/// </summary>
	public partial class pgGuidesPayRollToolBar : BaseToolBarPage
	{
		public pgGuidesPayRollToolBar()
		{
			InitializeComponent();
			rib.CollapseThresholdSize = new Size(1, 1);
			btnPrintPayrollSynthesis.Click += new RoutedEventHandler(btnPrintPayrollSynthesis_Click);
			btnPrintPayrollSynthesisForSigns.Click += new RoutedEventHandler(btnPrintPayrollSynthesisForSigns_Click);
			btnPrintAllPayroll.Click += new RoutedEventHandler(btnPrintAllPayroll_Click);
		}

		void btnPrintPayrollSynthesis_Click(object sender, RoutedEventArgs e)
		{
			ContentPageCommandEventArgs args = new ContentPageCommandEventArgs();
			args.CommandArgument = "printpayrollsynthesis";

			args.CommandType = ContentPageCommandType.Print;
			args.PageTemplateName = "pgGuidesPayRoll.xaml";
			MenuCommand(args);
		}

		void btnPrintPayrollSynthesisForSigns_Click(object sender, RoutedEventArgs e)
		{
			ContentPageCommandEventArgs args = new ContentPageCommandEventArgs();
			args.CommandArgument = "printpayrollsynthesisforsigns";

			args.CommandType = ContentPageCommandType.Print;
			args.PageTemplateName = "pgGuidesPayRoll.xaml";
			MenuCommand(args);
		}

		void btnPrintAllPayroll_Click(object sender, RoutedEventArgs e)
		{
			ContentPageCommandEventArgs args = new ContentPageCommandEventArgs();
			args.CommandArgument = "printallpayroll";

			args.CommandType = ContentPageCommandType.Print;
			args.PageTemplateName = "pgGuidesPayRoll.xaml";
			MenuCommand(args);
		}




		protected override void MenuCommand(ContentPageCommandEventArgs e)
		{
			if (OnMenuCommand != null)
				OnMenuCommand(this, e);
		}

		public override void SetButtons(ContentPageCommandEventArgs e)
		{
		}
	}
}
