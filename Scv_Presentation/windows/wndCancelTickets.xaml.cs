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
using Scv_Model;
using Scv_Dal;
using System.ComponentModel;
using Scv_Entities;

namespace Presentation
{
    /// <summary>
    /// Interaction logic for wndYearMonth.xaml
    /// </summary>
    public partial class wndCancelTickets : BaseDetailPage
    {
        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion// Events

        #region Variables
        CancelTicketViewModels vM = null;

        #endregion

        #region Public Properties



        #endregion// Public Properties

        #region Constructors

        public wndCancelTickets(int idPrenotazione, int idVisitaProg)
        {
            InitializeComponent();
            vM = new CancelTicketViewModels(idPrenotazione, idVisitaProg);
            cmbBiglietti.DataContext = vM;

        }

        #endregion// Constructors

        #region Events



        #endregion// Events

        #region Private Methods



        #endregion// Private Methods

        #region Event Handlers

        private void OnPropertyChanged(object sender, string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(sender, new PropertyChangedEventArgs(propertyName));
        }

        #endregion// Event Handlers

        private void cmbBiglietti_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!vM.SelectedBiglietto.Annullato)
            {
                btnCancelTicket.Content = "Annulla Biglietto";
            }
            else
            {
                btnCancelTicket.Content = "Ripristina";
            }

        }

        protected override void SetLayout()
        {
        }

        private void btnCancelTicket_Click(object sender, RoutedEventArgs e)
        {
            if(btnCancelTicket.Content == "Annulla Biglietto")
            {
                this.vM.SelectedBiglietto.Annullato = true;
                this.vM.SelectedBiglietto.DataOraAnnullamento = DateTime.Now;
            }
            else
            {
                this.vM.SelectedBiglietto.Annullato = false;
                this.vM.SelectedBiglietto.DataOraAnnullamento = null;
            }

            vM.Update();

            this.Close();
         }
    }
}
