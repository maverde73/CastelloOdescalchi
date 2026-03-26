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
	/// Interaction logic for pgEvidenzeGiornaliereToolBar.xaml
	/// </summary>
	public partial class pgEvidenzeGiornaliereToolBar : BaseToolBarPage
	{
		public pgEvidenzeGiornaliereToolBar()
		{
			InitializeComponent();
			rib.CollapseThresholdSize = new Size(1, 1);
			btnSchedule.Click += new RoutedEventHandler(btnSchedule_Click);
			btnPrintGuidesAssignments.Click += new RoutedEventHandler(btnPrintGuidesAssignments_Click);
			btnCancelPrenotation.Click += new RoutedEventHandler(btnCancelPrenotation_Click);

			btnSchedule.IsEnabled = false;
			btnCancelPrenotation.IsEnabled = false;
		}

		void btnSend_Click(object sender, RoutedEventArgs e)
		{
			ContentPageCommandEventArgs args = new ContentPageCommandEventArgs();

			args.CommandType = ContentPageCommandType.Send;
			args.PageTemplateName = "pgEvidenzeGiornaliere.xaml";
			MenuCommand(args);
		}

		void btnPrint_Click(object sender, RoutedEventArgs e)
		{
			ContentPageCommandEventArgs args = new ContentPageCommandEventArgs();

			args.CommandType = ContentPageCommandType.Open;
			args.PageTemplateName = "pgEvidenzeGiornaliere.xaml";
			MenuCommand(args);
		}

        void btnSchedule_Click(object sender, RoutedEventArgs e)
        {
            ContentPageCommandEventArgs args = new ContentPageCommandEventArgs();

            args.CommandType = ContentPageCommandType.Schedule;
			args.PageTemplateName = "pgEvidenzeGiornaliere.xaml";
            MenuCommand(args);
        }

		void btnPrintGuidesAssignments_Click(object sender, RoutedEventArgs e)
		{
			ContentPageCommandEventArgs args = new ContentPageCommandEventArgs();
			args.CommandArgument = "giornalieraguide";

			args.CommandType = ContentPageCommandType.Print;
			args.PageTemplateName = "pgEvidenzeGiornaliere.xaml";
			MenuCommand(args);
		}

		void btnCancelPrenotation_Click(object sender, RoutedEventArgs e)
		{
			ContentPageCommandEventArgs args = new ContentPageCommandEventArgs();
			args.CommandArgument = "annullaprenotazione";

			args.CommandType = ContentPageCommandType.Other;
			args.PageTemplateName = "pgEvidenzeGiornaliere.xaml";
			MenuCommand(args);
		}

		private void MenuCommand(ContentPageCommandEventArgs e)
		{
			if (OnMenuCommand != null)
				OnMenuCommand(this, e);
		}

		public override void SetButtons(ContentPageCommandEventArgs e)
		{
			btnSchedule.IsEnabled = e.SelectedIDs.Count > 0;
			btnCancelPrenotation.IsEnabled = e.SelectedIDs.Count > 0;
		}
	}
}
