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
	/// Interaction logic for pgFirmaGuideToolBar.xaml
	/// </summary>
	public partial class pgFirmaGuideToolBar : BaseToolBarPage
	{
		public pgFirmaGuideToolBar()
		{
			InitializeComponent();
			rib.CollapseThresholdSize = new Size(1, 1);
			btnPrint.Click += new RoutedEventHandler(btnPrint_Click);
		}

		void btnPrint_Click(object sender, RoutedEventArgs e)
		{
			ContentPageCommandEventArgs args = new ContentPageCommandEventArgs();

			args.CommandType = ContentPageCommandType.Print;
			args.PageTemplateName = "pgFirmaGuide.xaml";
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
