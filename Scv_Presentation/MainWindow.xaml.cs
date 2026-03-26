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
using Scv_Dal;
using Presentation.CustomControls;
using Scv_Entities;
using System.Globalization;
using System.Threading;

namespace Presentation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        #region Constants
        const string currentCulture = "it-IT";
       
        #endregion

        #region Fields

        LK_User user = null;

        List<PageTemplate> pageTemplates = null;

		BaseUserControl currentMenuPage = null;

		BaseContentPage currentContentPage = null;

        BaseToolBarPage currentToolBarPage = null;

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

        public MainWindow()
        {

            var culture = CultureInfo.CreateSpecificCulture(currentCulture);
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            InitializeComponent();

			Application.Current.MainWindow.SizeChanged += new SizeChangedEventHandler(MainWindow_SizeChanged);

			Telerik.Windows.Controls.RadWindow wnd = new wndLogin();
			wnd.ShowDialog();
			if (wnd.DialogResult == false)
				this.Close();

			user = new LK_User_Dal().GetItem(int.Parse(wnd.PromptResult));

			frmHeader.NavigationService.LoadCompleted += new System.Windows.Navigation.LoadCompletedEventHandler(NavigationService_LoadCompleted);
            frmMenu.NavigationService.LoadCompleted += new System.Windows.Navigation.LoadCompletedEventHandler(NavigationService_LoadCompleted);
            frmContent.NavigationService.LoadCompleted += new System.Windows.Navigation.LoadCompletedEventHandler(NavigationService_LoadCompleted);
			frmContent.NavigationService.Navigating += new NavigatingCancelEventHandler(NavigationService_Navigating);

            frmMenu.Source = new Uri(ApplicationState.CustomControlsPath + PageTemplate.GetPageTemplate(PageTemplates, "outlookbar").ContentPageUrl, UriKind.Relative);
            frmFooter.Source = new Uri(ApplicationState.PagesPath + PageTemplate.GetPageTemplate(PageTemplates, "footer").ContentPageUrl, UriKind.Relative);
        }

		void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			SetWindowsSize();
		}

		public void SetWindowsSize()
		{
			System.Drawing.Size s = new System.Drawing.Size();
			s.Height = (int)Application.Current.MainWindow.ActualHeight;
			s.Width = (int)Application.Current.MainWindow.ActualWidth;
			Helper_Dal.MainWindowSize = s;

			s = new System.Drawing.Size();
			s.Height =  (int)frmContent.ActualHeight;
			s.Width =  (int)frmContent.ActualWidth;
			Helper_Dal.ContentFrameSize = s;

			Helper_Dal.ContentFrameMarginTop = (int)frmContent.Margin.Top;
			Helper_Dal.ContentFrameMarginBottom = (int)frmContent.Margin.Bottom;
		}

		void NavigationService_Navigating(object sender, NavigatingCancelEventArgs e)
		{
			NavigatingCancelEventArgs args = e as NavigatingCancelEventArgs;
			if (args != null)
			{
				Frame navigator = args.Navigator as Frame;
				if (navigator != null)
				{
					Uri uri = navigator.CurrentSource as Uri;
					if (uri != null)
					{
						if (uri.OriginalString.Contains("pgGuidesAssignment.xaml") && !navigator.Source.OriginalString.Contains("pgGuidesAssignment.xaml"))
						{
							if (MessageBox.Show("Uscire dall'assegnazione guide? Evantuali assegnazioni non confermate andranno perse!", "Assegnazione guide", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
							{
								navigator.StopLoading();
							}
						}
					}
				}
			}
		}

        void NavigationService_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            Frame f = sender as Frame;
            if (f != null)
            {
                switch (f.Name)
                {
                    case "frmMenu":
						currentMenuPage = (BaseUserControl)e.Content as BaseUserControl;
						currentMenuPage.User = user;
                        ((ucOutLookBar)e.Content).ContextChanged += new Scv_Model.CommonEvents.ContextChangedEventHandler(ChangeContext);
                        ((ucOutLookBar)e.Content).ContentCommand += new Scv_Model.CommonEvents.ContentPageCommandEventHandler(ContentCommand);
						break;

                    case "frmContent":
                        currentContentPage = (BaseContentPage)e.Content as BaseContentPage;
						currentContentPage.User = user;
                        if (currentContentPage != null)
                            currentContentPage.ItemSelectionEvent += new CommonEvents.ContentPageItemSelectionEventHandler(currentContentPage_ItemSelectionEvent);
                        break;

                    case "frmHeader":
                        currentToolBarPage = (BaseToolBarPage)e.Content as BaseToolBarPage;
                        if (currentToolBarPage != null)
                            currentToolBarPage.OnMenuCommand += new Scv_Model.CommonEvents.ToolBarCommandEventHandler(ToolBarCommand);
                        break;

                }
            }
        }

        void currentContentPage_ItemSelectionEvent(ContentPageCommandEventArgs e)
        {
            currentToolBarPage.SetButtons(e);
        }

        public void ChangeContext(ContextArgs args)
        {
            frmHeader.Source = new Uri(ApplicationState.PagesPath + args.ToolBarPageUrl, UriKind.Relative);
            frmContent.Source = new Uri(ApplicationState.PagesPath + args.MainPageUrl, UriKind.Relative);
			this.Title = "Gestione Visite - " + args.ContentPageTitle;
        }

        public void MenuCommand(object sender, MenuEventArgs e)
        {
            ContentPageCommandEventArgs args = new ContentPageCommandEventArgs();
            args.CommandArgument = "null";
            args.CommandType = ContentPageCommandType.Save;

            BaseContentPage pg = null;
            currentContentPage.Command(args);

        }

        public void ToolBarCommand(object sender, ContentPageCommandEventArgs e)
        {
			if (currentContentPage != null)
				currentContentPage.Command(e);
        }

		public void ContentCommand(ContentPageCommandEventArgs args)
		{
			if(currentContentPage != null)
				currentContentPage.Command(args);
		}

    }
}
