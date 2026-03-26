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
	/// Interaction logic for pgGuidesAssignmentToolBar.xaml
	/// </summary>
	public partial class pgGuidesAssignmentToolBar : BaseToolBarPage
	{
		public pgGuidesAssignmentToolBar()
		{
			InitializeComponent();
			rib.CollapseThresholdSize = new Size(1, 1);
			btnSend.Click += new RoutedEventHandler(btnSend_Click);
			btnAllAvv.Click += new RoutedEventHandler(btnAllAvv_Click);
			btnAllAcc.Click += new RoutedEventHandler(btnAllAcc_Click);
			btnPrintAll.Click += new RoutedEventHandler(btnPrintAll_Click);
		}

		void btnSend_Click(object sender, RoutedEventArgs e)
		{
			ContentPageCommandEventArgs args = new ContentPageCommandEventArgs();

			args.CommandType = ContentPageCommandType.Send;
			args.PageTemplateName = "pgGuidesAssignment.xaml";
			MenuCommand(args);
		}

		void btnAllAvv_Click(object sender, RoutedEventArgs e)
		{
			ContentPageCommandEventArgs args = new ContentPageCommandEventArgs();

			args.CommandType = ContentPageCommandType.Other;
			args.CommandArgument = "avvall";
			args.PageTemplateName = "pgGuidesAssignment.xaml";
			MenuCommand(args);
		}

		void btnAllAcc_Click(object sender, RoutedEventArgs e)
		{
			ContentPageCommandEventArgs args = new ContentPageCommandEventArgs();

			args.CommandType = ContentPageCommandType.Other;
			args.CommandArgument = "accall";
			args.PageTemplateName = "pgGuidesAssignment.xaml";
			MenuCommand(args);
		}

		void btnPrintAll_Click(object sender, RoutedEventArgs e)
		{
			ContentPageCommandEventArgs args = new ContentPageCommandEventArgs();

			args.CommandType = ContentPageCommandType.Print;
			args.CommandArgument = "all";
			args.PageTemplateName = "pgGuidesAssignment.xaml";
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
