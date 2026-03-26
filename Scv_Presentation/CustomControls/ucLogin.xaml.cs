
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

namespace Presentation.CustomControls
{
    /// <summary>
    /// Interaction logic for ucLogin.xaml
    /// </summary>
    public partial class ucLogin : Window
    {
        LK_User_Dal dal = new LK_User_Dal();
        public ucLogin()
        {
            InitializeComponent();
        }

        private void RadButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DoLogin()
        {
            LK_User user = dal.Login(txtUserName.Text, pwdPassword.Password);
            if (user == null)
            {
                //lblError.Content = "Username o password errati.";
            }
            else
            {
                (new MainWindow()).Show();
                this.Close();
            }
        }
    }
}
