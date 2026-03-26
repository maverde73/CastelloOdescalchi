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
using System.Windows.Shapes;
using Scv_Dal;
using Telerik.Windows.Controls;
using Scv_Entities;
using Scv_Model;
using System.Configuration;

namespace Presentation
{
	/// <summary>
	/// Interaction logic for wndLogin.xaml
	/// </summary>
	public partial class wndLogin : Telerik.Windows.Controls.RadWindow
	{
        LK_User_Dal dal = new LK_User_Dal();
		public wndLogin()
		{
			InitializeComponent();
			this.DialogResult = false;
			//BindTables();
            SetApplicationTheme();
			btnLogin.Click += new RoutedEventHandler(btnLogin_Click);
			//cmbTheme.SelectionChanged += new SelectionChangedEventHandler(cmbTheme_SelectionChanged);
		}

		void cmbTheme_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			switch(cmbTheme.SelectedValue.ToString())
			{
				case "VistaTheme":
					StyleManager.ApplicationTheme = new VistaTheme();
					break;
				case "Expression_DarkTheme":
					StyleManager.ApplicationTheme = new Expression_DarkTheme();
					break;
				case "MetroTheme":
					//StyleManager.ApplicationTheme = new MetroTheme();
					break;
				case "ModernTheme":
					StyleManager.ApplicationTheme = new ModernTheme();
					break;
				case "Office_BlackTheme":
					StyleManager.ApplicationTheme = new Office_BlackTheme();
					break;
				case "Office_BlueTheme":
					StyleManager.ApplicationTheme = new Office_BlueTheme();
					break;
				case "Office_SilverTheme":
					StyleManager.ApplicationTheme = new Office_SilverTheme();
					break;
				case "SummerTheme":
					StyleManager.ApplicationTheme = new SummerTheme();
					break;
				case "TransparentTheme":
					StyleManager.ApplicationTheme = new TransparentTheme();
					break;
				case "Windows7Theme":
					StyleManager.ApplicationTheme = new Windows7Theme();
					break;
				case "Windows8Theme":
					StyleManager.ApplicationTheme = new Windows8Theme();
					break;
				case "Windows8TouchTheme":
					StyleManager.ApplicationTheme = new Windows8TouchTheme();
					break;
			}

		}

		void btnLogin_Click(object sender, RoutedEventArgs e)
		{
			DoLogin();
		}

		private void DoLogin()
		{
			//LK_User user = dal.Login(txtUsername.Text, this.pwdPassword.Password);
			LK_User user = dal.Login(txtUsername.Text, this.pwdPassword.Password);
			if (user == null)
				lblError.Content = "Username o password errati, oppure Utente non attivo.";
			else
			{
                //(new MainWindow()).Show();

                ApplicationState.SetValue("currentUser", user);

				this.PromptResult = user.Id_User.ToString();
				this.DialogResult = true;
                this.Close();
			}
		}

		private void BindTables()
		{
			List<ThemeItem> Themes = Themes_Dal.GetThemes();
            //Themes.FirstOrDefault(tx => tx.Name == 
			cmbTheme.ItemsSource = Themes;
			cmbTheme.DisplayMemberPath = "Name";
			cmbTheme.SelectedValuePath = "Value";
			if (cmbTheme.Items.Count > 0)
				cmbTheme.SelectedIndex = 0;
		}

        private void SetApplicationTheme()
        {
            string configuredTheme = ConfigurationManager.AppSettings["applicationTheme"];
            
            switch (configuredTheme)
            {
                case "VistaTheme":
                    StyleManager.ApplicationTheme = new VistaTheme();
                    break;
                case "Expression_DarkTheme":
                    StyleManager.ApplicationTheme = new Expression_DarkTheme();
                    break;
                case "MetroTheme":
                    //StyleManager.ApplicationTheme = new MetroTheme();
                    break;
                case "ModernTheme":
                    StyleManager.ApplicationTheme = new ModernTheme();
                    break;
                case "Office_BlackTheme":
                    StyleManager.ApplicationTheme = new Office_BlackTheme();
                    break;
                case "Office_BlueTheme":
                    StyleManager.ApplicationTheme = new Office_BlueTheme();
                    break;
                case "Office_SilverTheme":
                    StyleManager.ApplicationTheme = new Office_SilverTheme();
                    break;
                case "SummerTheme":
                    StyleManager.ApplicationTheme = new SummerTheme();
                    break;
                case "TransparentTheme":
                    StyleManager.ApplicationTheme = new TransparentTheme();
                    break;
                case "Windows7Theme":
                    StyleManager.ApplicationTheme = new Windows7Theme();
                    break;
                case "Windows8Theme":
                    StyleManager.ApplicationTheme = new Windows8Theme();
                    break;
                case "Windows8TouchTheme":
                    StyleManager.ApplicationTheme = new Windows8TouchTheme();
                    break;
            }
        }
	}
}
