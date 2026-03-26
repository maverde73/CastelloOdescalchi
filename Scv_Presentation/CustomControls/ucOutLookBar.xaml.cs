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
using Scv_Model.Common;
using Scv_Dal;
using System.IO;
using Telerik.Windows.Controls;
using System.ComponentModel;
using Scv_Entities;

namespace Presentation.CustomControls
{
	/// <summary>
	/// Interaction logic for ucOutLookBar.xaml
	/// </summary>
	public partial class ucOutLookBar : BaseUserControl, INotifyPropertyChanged
	{
		#region fields

		List<PageTemplate> pageTemplates = null;

		#endregion

		#region Properties

		private List<PageTemplate> PageTemplates
		{
			get
			{
				if (pageTemplates == null)
					pageTemplates = PageTemplates_Dal.GetPageTemplates(ApplicationState.XmlPath + CommonKeyNames.PageTemplateFileName);
				return pageTemplates;
			}
		}

		#endregion

		#region Constructors

		public ucOutLookBar()
		{
			InitializeComponent();

			this.PropertyChanged += new PropertyChangedEventHandler(ucOutLookBar_PropertyChanged);

			outlookBar.SelectionChanged += new RadSelectionChangedEventHandler(outlookBar_SelectionChanged);
			Prenotazione_Dal pren_dal = new Prenotazione_Dal();
			var tipiconfermaCounts = pren_dal.GetTipiConfermaCounts();
		}


		#endregion// Constructors

		#region Events Handling

		public Scv_Model.CommonEvents.ContextChangedEventHandler ContextChanged;

		public Scv_Model.CommonEvents.ContentPageCommandEventHandler ContentCommand;

		void ucOutLookBar_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case "User":
					this.tvUsers.IsEnabled = User.Fl_Amministratore;
					break;
			}
		}

		private void outlookBar_SelectionChanged(object sender, RoutedEventArgs e)
		{
			ContextArgs args = new ContextArgs();
			RadOutlookBarItem selectedItem = (sender as RadOutlookBar).SelectedItem as RadOutlookBarItem;

			if (selectedItem != null)
			{
				PageTemplate pg = PageTemplate.GetPageTemplate(PageTemplates, selectedItem.Name);
				args.MainPageUrl = pg.ContentPageUrl;
				args.MenuPageUrl = pg.MenuPageUrl;
				args.ToolBarPageUrl = pg.ToolBarPageUrl;
				args.ContentPageTitle = pg.ContentPageTitle;
				OnContextChangedEvent(args);

				ClearTreeViewSelection(PrenotationTreeView);
				ClearTreeViewSelection(tvWarehouse);
				ClearTreeViewSelection(tvSettings);
				ClearTreeViewSelection(GuidesTreeView);
				ClearTreeViewSelection(tvMandato);				
			}
		}

		private void PrenotationsTreeView_SelectionChanged(object sender, RoutedEventArgs e)
		{
			RadTreeViewItem item = (RadTreeViewItem)((RadTreeView)(e.Source)).SelectedItem as RadTreeViewItem;
			if (item != null)
			{
				ChangePageByTreeView((RadTreeViewItem)((RadTreeView)(e.Source)).SelectedItem);
			}
		}

        private void TicketingTreeView_SelectionChanged(object sender, RoutedEventArgs e)
        {
            RadTreeViewItem item = (RadTreeViewItem)((RadTreeView)(e.Source)).SelectedItem as RadTreeViewItem;
            if (item != null)
            {
                ChangePageByTreeView((RadTreeViewItem)((RadTreeView)(e.Source)).SelectedItem);
            }
        }

		private void GuidesTreeView_SelectionChanged(object sender, RoutedEventArgs e)
		{
			RadTreeViewItem item = (RadTreeViewItem)((RadTreeView)(e.Source)).SelectedItem as RadTreeViewItem;
			if (item != null)
			{
				ChangePageByTreeView((RadTreeViewItem)((RadTreeView)(e.Source)).SelectedItem);
			}
		}

		private void tvSettings_SelectionChanged(object sender, RoutedEventArgs e)
		{
			ChangePageByTreeView((RadTreeViewItem)((RadTreeView)(e.Source)).SelectedItem);
		}

		private void tvWarehouse_SelectionChanged(object sender, RoutedEventArgs e)
		{
			ChangePageByTreeView((RadTreeViewItem)((RadTreeView)(e.Source)).SelectedItem);
		}

		private void tvMandato_SelectionChanged(object sender, RoutedEventArgs e)
		{
			ChangePageByTreeView((RadTreeViewItem)((RadTreeView)(e.Source)).SelectedItem);
		}

		
		private void ChangePageByTreeView(RadTreeViewItem item)
		{
			if (item != null && !string.IsNullOrEmpty(item.CommandParameter.ToString()))
			{
				ContextArgs args = new ContextArgs();
				PageTemplate pg = PageTemplate.GetPageTemplate(PageTemplates, item.CommandParameter.ToString());
				args.MainPageUrl = pg.ContentPageUrl;
				args.MenuPageUrl = pg.MenuPageUrl;
				args.ToolBarPageUrl = pg.ToolBarPageUrl;
				args.ContentPageTitle = pg.ContentPageTitle;
				OnContextChangedEvent(args);
			}
		}

		private void OnContextChangedEvent(ContextArgs args)
		{
			if (ContextChanged != null)
				ContextChanged(args);
		}

		private void OnContentCommand(ContentPageCommandEventArgs args)
		{
			if (ContentCommand != null)
				ContentCommand(args);
		}

		#endregion// Events Handling

		#region Private Methods

		#endregion// Private Methods

		#region Utils

		public static void ClearTreeViewSelection(RadTreeView tv)
		{
			if (tv != null)
				ClearTreeViewItemsControlSelection(tv.Items, tv.ItemContainerGenerator);
		}
		private static void ClearTreeViewItemsControlSelection(ItemCollection ic, ItemContainerGenerator icg)
		{
			if ((ic != null) && (icg != null))
				for (int i = 0; i < ic.Count; i++)
				{
					RadTreeViewItem tvi = icg.ContainerFromIndex(i) as RadTreeViewItem;
					if (tvi != null)
					{
						ClearTreeViewItemsControlSelection(tvi.Items, tvi.ItemContainerGenerator);
						tvi.IsSelected = false;
					}
				}
		}

		#endregion// Utils
	}
}
