using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Scv_Entities;
using Scv_Model;
using Scv_Dal;

namespace Presentation.Classes
{
    public class Ticket : INotifyPropertyChanged
    {

        EasyTicketing_DAL easyTicketing_DAL = new EasyTicketing_DAL();

        #region Public Properties

        int id_Riga;
        public int Id_Riga
        {
            get { return id_Riga; }
            set { id_Riga = value; OnPropertyChanged(this, "Id_Riga"); }
        }

        int id_TipoVisita;
        public int Id_TipoVisita
        {
            get { return id_TipoVisita; }
            set { id_TipoVisita = value; OnPropertyChanged(this, "Id_TipoVisita"); }

        }

        string tipoVisita;
        public string TipoVisita
        {
            get { return tipoVisita; }
            set { tipoVisita = value; OnPropertyChanged(this, "TipoVisita"); }

        }

        string ora_Visita = "09:00";
        public string Ora_Visita
        {
            get { return ora_Visita; }
            set { ora_Visita = value; OnPropertyChanged(this, "Ora_Visita"); }

        }

        LK_TipoVisita objTipoVisita;
        public LK_TipoVisita ObjTipoVisita
        {
            get { return objTipoVisita; }
            set { objTipoVisita = value; OnPropertyChanged(this, "ObjTipoVisita"); }
        }

        int visitatori;
        public int Visitatori
        {
            get { return visitatori; }
            set { visitatori = value; OnPropertyChanged(this, "Visitatori"); }

        }

        int interi;
        public int Interi
        {
            get { return interi; }
            set { interi = value; OnPropertyChanged(this, "Interi"); }

        }

        int ridotti;
        public int Ridotti
        {
            get { return ridotti; }
            set { ridotti = value; OnPropertyChanged(this, "Ridotti"); }

        }

        int omaggio;
        public int Omaggio
        {
            get { return omaggio; }
            set { omaggio = value; OnPropertyChanged(this, "Omaggio"); }

        }

        int scontati;
        public int Scontati
        {
            get { return scontati; }
            set { scontati = value; OnPropertyChanged(this, "Scontati"); }

        }

        int cumulativi;
        public int Cumulativi
        {
            get { return cumulativi; }
            set { cumulativi = value; OnPropertyChanged(this, "Cumulativi"); }

        }

        bool interiEnabled;
        public bool InteriEnabled
        {
            get { return interiEnabled; }
            set { interiEnabled = value; OnPropertyChanged(this, "InteriEnabled"); }
        }

        bool ridottiEnabled;
        public bool RidottiEnabled
        {
            get { return ridottiEnabled; }
            set { ridottiEnabled = value; OnPropertyChanged(this, "RidottiEnabled"); }
        }

        bool scontatiEnabled;
        public bool ScontatiEnabled
        {
            get { return scontatiEnabled; }
            set { scontatiEnabled = value; OnPropertyChanged(this, "ScontatiEnabled"); }
        }

        bool omaggioEnabled;
        public bool OmaggioEnabled
        {
            get { return omaggioEnabled; }
            set { omaggioEnabled = value; OnPropertyChanged(this, "OmaggioEnabled"); }
        }

        bool cumulativoEnabled;
        public bool CumulativoEnabled
        {
            get { return cumulativoEnabled; }
            set { cumulativoEnabled = value; OnPropertyChanged(this, "CumulativoEnabled"); }
        }

        bool cumulativo = false;
        public bool Cumulativo
        {
            get { return cumulativo; }
            set { cumulativo = value; OnPropertyChanged(this, "Cumulativo"); }
        }

        bool hourEnabled = true;
        public bool HourEnabled
        {
            get { return hourEnabled; }
            set { hourEnabled = value; OnPropertyChanged(this, "HourEnabled"); }
        }

        bool resetEnabled = true;
        public bool ResetEnabled
        {
            get { return resetEnabled; }
            set { resetEnabled = value; OnPropertyChanged(this, "ResetEnabled"); }
        }

        private ObservableCollection<Hour> availableHours = null;
        public ObservableCollection<Hour> AvailableHours
        {
            get
            {
                if (availableHours == null)
                    availableHours = new ObservableCollection<Hour>();
                return availableHours;
            }
            set
            {
                availableHours = value;
                OnPropertyChanged(this, "AvailableHours");
            }
        }

        decimal prezzo;
        public decimal Prezzo
        {
            get { return prezzo; }
            set { prezzo = value; OnPropertyChanged(this, "Prezzo"); }

        }

        decimal prezzoInteri;
        public decimal PrezzoInteri
        {
            get { return prezzoInteri; }
            set { prezzoInteri = value; OnPropertyChanged(this, "PrezzoInteri"); }
        }

        decimal prezzoRidotti;
        public decimal PrezzoRidotti
        {
            get { return prezzoRidotti; }
            set { prezzoRidotti = value; OnPropertyChanged(this, "PrezzoRidotti"); }
        }

        decimal prezzoScontati;
        public decimal PrezzoScontati
        {
            get { return prezzoScontati; }
            set { prezzoScontati = value; OnPropertyChanged(this, "PrezzoScontati"); }
        }

        decimal prezzoCumulativi;
        public decimal PrezzoCumulativi
        {
            get { return prezzoCumulativi; }
            set { prezzoCumulativi = value; OnPropertyChanged(this, "PrezzoCumulativi"); }
        }

        Pagamento pagamento = null;
        public Pagamento Pagamento
        {
            get { return pagamento; }
            set { pagamento = value; OnPropertyChanged(this, "Pagamento"); }
        }

        ObservableCollection<LK_TipoBiglietto> availableTicketTypes = null;
        public ObservableCollection<LK_TipoBiglietto> AvailableTicketTypes
        {
            get
            {
                if (availableTicketTypes == null)
                    availableTicketTypes = new ObservableCollection<LK_TipoBiglietto>();
                return availableTicketTypes;
            }
            set
            {
                OnPropertyChanged(this, "AvailableTicketTypes");
                availableTicketTypes = value;
            }
        }

        bool allDisabled = false;
        public bool AllDisabled
        {
            get { return allDisabled; }
            set { allDisabled = value; OnPropertyChanged(this, "AllDisabled"); }
        }
        #endregion



        #region Constructors

        public Ticket() { }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(object sender, string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(sender, new PropertyChangedEventArgs(propertyName));

            switch (propertyName)
            {
                case "ObjTipoVisita":
                    if (Pagamento != null)
                    {
                        InteriEnabled = false;
                        RidottiEnabled = false;
                        ScontatiEnabled = false;
                        CumulativoEnabled = false;
                        OmaggioEnabled = false;
                        ResetEnabled = false;
                    }
                    else
                    {
                        InteriEnabled = ObjTipoVisita.Interi;
                        RidottiEnabled = ObjTipoVisita.Ridotti;
                        ScontatiEnabled = ObjTipoVisita.Scontati;
                        CumulativoEnabled = ObjTipoVisita.Cumulativo;
                        OmaggioEnabled = ObjTipoVisita.Omaggio;
                        ResetEnabled = true;
                    }

                    break;

                case "Interi":
                case "Ridotti":
                case "Scontati":
                case "Cumulativi":
                case "Omaggio":

                    if (Pagamento == null)
                    {
                        PrezzoInteri = (ObjTipoVisita.PrezzoIntero != null ? (decimal)ObjTipoVisita.PrezzoIntero : 0) * Interi;
                        PrezzoRidotti = (ObjTipoVisita.PrezzoRidotto != null ? (decimal)ObjTipoVisita.PrezzoRidotto : 0) * Ridotti;
                        PrezzoScontati = (ObjTipoVisita.PrezzoScontato != null ? (decimal)ObjTipoVisita.PrezzoScontato : 0) * Scontati;
                        if (Cumulativi > 0)
                            PrezzoCumulativi = (ObjTipoVisita.PrezzoCumulativo != null ? (decimal)ObjTipoVisita.PrezzoCumulativo : 0);
                        else
                            PrezzoCumulativi = 0;
                    }
                    else
                    {
                        PrezzoInteri = (Pagamento.PrezzoIntero != null ? (decimal)Pagamento.PrezzoIntero : 0) * Interi;
                        PrezzoRidotti = (Pagamento.PrezzoRidotto != null ? (decimal)Pagamento.PrezzoRidotto : 0) * Ridotti;
                        PrezzoScontati = (Pagamento.PrezzoScontato != null ? (decimal)Pagamento.PrezzoScontato : 0) * Scontati;
                        if (Cumulativi > 0)
                            PrezzoCumulativi = (Pagamento.PrezzoCumulativo != null ? (decimal)Pagamento.PrezzoCumulativo : 0);
                        else
                            PrezzoCumulativi = 0;
                    }

                    Prezzo = PrezzoInteri + PrezzoRidotti + PrezzoScontati + PrezzoCumulativi;
                    break;
            }

        }
    }
}
