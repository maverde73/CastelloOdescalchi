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
	/// Interaction logic for pgPersonnelToolBar.xaml
	/// </summary>
	public partial class pgGoodsToolBar : BaseToolBarPage
	{
		public pgGoodsToolBar()
		{
			InitializeComponent();
			rib.CollapseThresholdSize = new Size(1, 1);
			btnNew.Click += new RoutedEventHandler(btnNew_Click);
			btnOpen.Click += new RoutedEventHandler(btnOpen_Click);
			btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
			btnOpen.IsEnabled = false;
			btnDelete.IsEnabled = false;
		}

		void btnNew_Click(object sender, RoutedEventArgs e)
		{
			ContentPageCommandEventArgs args = new ContentPageCommandEventArgs();

			args.CommandType = ContentPageCommandType.New;
			args.PageTemplateName = "pgGoods.xaml";
			MenuCommand(args);
		}

		void btnOpen_Click(object sender, RoutedEventArgs e)
		{
			ContentPageCommandEventArgs args = new ContentPageCommandEventArgs();

			args.CommandType = ContentPageCommandType.Open;
			args.PageTemplateName = "pgGoods.xaml";
			MenuCommand(args);
		}

        void btnSchedule_Click(object sender, RoutedEventArgs e)
        {
            ContentPageCommandEventArgs args = new ContentPageCommandEventArgs();

            args.CommandType = ContentPageCommandType.Schedule;
			args.PageTemplateName = "pgGoods.xaml";
            MenuCommand(args);
        }

		void btnDelete_Click(object sender, RoutedEventArgs e)
		{
			ContentPageCommandEventArgs args = new ContentPageCommandEventArgs();

			args.CommandType = ContentPageCommandType.Delete;
			args.PageTemplateName = "pgGoods.xaml";
			MenuCommand(args);
		}

		protected override void MenuCommand(ContentPageCommandEventArgs e)
		{
			if (OnMenuCommand != null)
				OnMenuCommand(this, e);
		}

		public override void SetButtons(ContentPageCommandEventArgs e)
		{
			btnDelete.IsEnabled = e.SelectedIDs.Count > 0;
			btnOpen.IsEnabled = e.SelectedIDs.Count > 0;
		}
	}
}
