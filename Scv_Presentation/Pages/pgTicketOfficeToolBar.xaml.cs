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
	/// Interaction logic for pgTicketOfficeToolBar.xaml
	/// </summary>
	public partial class pgTicketOfficeToolBar : BaseToolBarPage
	{
		public pgTicketOfficeToolBar()
		{
			InitializeComponent();
			rib.CollapseThresholdSize = new Size(1, 1);
			btnPrintTickets.Click += new RoutedEventHandler(btnPrintTickets_Click);
			btnPrintTickets.IsEnabled = false;
		}

		void btnPrintTickets_Click(object sender, RoutedEventArgs e)
		{
			ContentPageCommandEventArgs args = new ContentPageCommandEventArgs();
			args.CommandArgument = "printtickets";
			args.CommandType = ContentPageCommandType.Print;
			args.PageTemplateName = "pgTicketOffice.xaml";
			MenuCommand(args);
		}

		private void MenuCommand(ContentPageCommandEventArgs e)
		{
			if (OnMenuCommand != null)
				OnMenuCommand(this, e);
		}

		public override void SetButtons(ContentPageCommandEventArgs e)
		{
			btnPrintTickets.IsEnabled = e.SelectedIDs.Count > 0;
		}
	}
}
