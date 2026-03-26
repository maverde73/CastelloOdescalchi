using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.ScheduleView;
using Scv_Dal;
using Scv_Entities;
using Scv_Model;
using Presentation.Classes;

namespace Presentation
{
    public class CancelTicketViewModels : INotifyPropertyChanged
    {
        private Biglietto_Dal dalBiglietto = new Biglietto_Dal();
        public CancelTicketViewModels(int idPrenotazione, int idVisitaProg)
        {
            LoadSrcBiglietti(idVisitaProg);
        }

        public void Update()
        {
            dalBiglietto.InsertOrUpdate(SelectedBiglietto);
        }


        public void LoadSrcBiglietti(int idVisitaProg)
        {
            SrcBiglietti = new ObservableCollection<Biglietto>(dalBiglietto.Get_List_ByIdVisitaProgrammata(idVisitaProg));
            if (SrcBiglietti.Count > 0)
                SelectedBiglietto = SrcBiglietti[0];
        }

        private ObservableCollection<Biglietto> srcBiglietti = null;
        public ObservableCollection<Biglietto> SrcBiglietti
        {
            get
            {
                if (srcBiglietti == null)
                    srcBiglietti = new ObservableCollection<Biglietto>();
                return srcBiglietti;
            }
            set
            {
                srcBiglietti = value;
                OnPropertyChanged(this, "SrcBiglietti");
            }
        }

        Biglietto selectedBiglietto = new Biglietto();
        public Biglietto SelectedBiglietto
        {
            get
            {
                return selectedBiglietto;
            }
            set
            {
                selectedBiglietto = value;
                OnPropertyChanged(this, "SelectedBiglietto");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(object sender, string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(sender, new PropertyChangedEventArgs(propertyName));
            
            //switch (propertyName)
            //{
            //    case "Id_TipoMovimento":

            //        break;
            //}

          
        }
    }
}
