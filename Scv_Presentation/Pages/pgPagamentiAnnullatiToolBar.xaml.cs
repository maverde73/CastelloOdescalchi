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
	/// Interaction logic for pgPagamentiAnnullatiToolBar.xaml
	/// </summary>
	public partial class pgPagamentiAnnullatiToolBar : BaseToolBarPage
	{
		public pgPagamentiAnnullatiToolBar()
		{
			InitializeComponent();
			rib.CollapseThresholdSize = new Size(1, 1);
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
