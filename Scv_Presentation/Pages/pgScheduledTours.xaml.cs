using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Scv_Model;
using Scv_Dal;
using Scv_Entities;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;
using Telerik.Windows.Controls.ScheduleView;
using System.Net.Mail;
using VisitsSummaryManager;
using Presentation.CustomControls.PaymentLib;
using System.Threading;
using System.Globalization;
using System.Configuration;
using System.Windows.Threading;


namespace Presentation.Pages
{
    /// <summary>
    /// Interaction logic for pgScheduledTours.xaml
    /// </summary>
    public partial class pgScheduledTours : BaseContentPage, INotifyPropertyChanged
    {

        #region Variables
        ScheduledToursViewModel vm = null;
        bool alreadyOpen = false;
        #endregion

        public pgScheduledTours()
        {
            InitializeComponent();
            CommandEvent += new CommonEvents.ContentPageCommandEventHandler(pgScheduledTours_CommandEvent);
            vm = new ScheduledToursViewModel();
            swAppointments.DataContext = vm;
        }

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        private void pgScheduledTours_CommandEvent(ContentPageCommandEventArgs e)
        {
            switch (e.CommandType)
            {
                case ContentPageCommandType.New:
                    //vm.ResetAll();
                    break;
            }
        }

        private void RadScheduleView_ShowDialog(object sender, ShowDialogEventArgs e)
        {
            if (e.DialogViewModel is AppointmentDialogViewModel)
                e.Cancel = true;


        }


        protected void frm_DetailWindowClosing(object sender, ClosingDetailWindowEventArgs e)
        {
           
        }

        #endregion// Events

        #region Methods
        private void DoOpenItem(int idPrenotazione, int idVisitaProgrammata, WindowStartupLocation startupLocation)
        {
            bool open = true;
            string windowName = string.Format("Prenotation_{0}", idVisitaProgrammata.ToString());
            BaseDetailPage frm = null;
            foreach (Window wnd in Application.Current.Windows)
            {
                if (wnd.Name == windowName && wnd.IsVisible)
                {
                    frm = (BaseDetailPage)wnd;
                    frm.Focus();
                    open = false;
                }
            }
            if (open)
            {
                frm = new wndScheduleTours(idPrenotazione, User);
                frm.User = User;
                frm.Name = windowName;
                frm.WindowStartupLocation = startupLocation;
                frm.DetailWindowClosing += new CommonDelegates.ClosingDetailWindowEventHandler(frm_DetailWindowClosing);
                frm.CloseOnSave = true;
                frm.ShowDialog();
                vm.LoadAppointments();
            }
           
        }

       
        #endregion

        private void OnPropertyChanged(object sender, string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(sender, new PropertyChangedEventArgs(propertyName));
        }

        private void swAppointments_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Appointment selectedAppointment = (Appointment)swAppointments.SelectedAppointment;
            var appInfo = selectedAppointment.Body.Split(',');
            DoOpenItem(Convert.ToInt32(appInfo[0]), Convert.ToInt32(appInfo[0]), WindowStartupLocation.CenterScreen);
        }


    }
}
