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
	/// Interaction logic for pgBookingToolBar.xaml
	/// </summary>
	public partial class pgBookingToolBar : BaseToolBarPage
	{
		public pgBookingToolBar()
		{
			InitializeComponent();
			rib.CollapseThresholdSize = new Size(1, 1);
			btnNew.Click += new RoutedEventHandler(btnNew_Click);
			btnOpen.Click += new RoutedEventHandler(btnOpen_Click);
            btnSchedule.Click += new RoutedEventHandler(btnSchedule_Click);
			//btnExtractPayments.Click += new RoutedEventHandler(btnExtractPayments_Click);
			btnOpen.IsEnabled = false;
            btnSchedule.IsEnabled = false;
		}

		void btnNew_Click(object sender, RoutedEventArgs e)
		{
			ContentPageCommandEventArgs args = new ContentPageCommandEventArgs();

			args.CommandType = ContentPageCommandType.New;
			args.PageTemplateName = "pgBooking.xaml";
			MenuCommand(args);
		}

		void btnOpen_Click(object sender, RoutedEventArgs e)
		{
			ContentPageCommandEventArgs args = new ContentPageCommandEventArgs();

			args.CommandType = ContentPageCommandType.Open;
			args.PageTemplateName = "pgBooking.xaml";
			MenuCommand(args);
		}

        void btnSchedule_Click(object sender, RoutedEventArgs e)
        {
            ContentPageCommandEventArgs args = new ContentPageCommandEventArgs();

            args.CommandType = ContentPageCommandType.Schedule;
            args.PageTemplateName = "pgBooking.xaml";
            MenuCommand(args);
        }

		void btnDelete_Click(object sender, RoutedEventArgs e)
		{
			ContentPageCommandEventArgs args = new ContentPageCommandEventArgs();

			args.CommandType = ContentPageCommandType.Delete;
			args.PageTemplateName = "pgBooking.xaml";
			MenuCommand(args);
		}

		void btnExtractPayments_Click(object sender, RoutedEventArgs e)
		{
			ContentPageCommandEventArgs args = new ContentPageCommandEventArgs();
			args.CommandArgument = "extractpaymentsdata";
			args.CommandType = ContentPageCommandType.Other;
			args.PageTemplateName = "pgBooking.xaml";
			MenuCommand(args);
		}

		private void MenuCommand(ContentPageCommandEventArgs e)
		{
			if (OnMenuCommand != null)
				OnMenuCommand(this, e);
		}

		public override void SetButtons(ContentPageCommandEventArgs e)
		{
			btnOpen.IsEnabled = e.SelectedIDs.Count > 0;
            btnSchedule.IsEnabled = e.SelectedIDs.Count > 0;
		}
	}
}
