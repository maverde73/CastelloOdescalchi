using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Scv_Dal;
using Scv_Entities;
using System.Globalization;
using System.Threading;
using Scv_Model;
using System.Windows;
using System.Windows.Media;

namespace Presentation
{
    public class TipoVisitaViewModel : INotifyPropertyChanged
    {
        #region DAL

        private LK_TipoVisita_Dal dal = new LK_TipoVisita_Dal();


        #endregion// DAL

        #region Private Fields



        #endregion// Private Fields

        #region Properties

        private LK_TipoVisita objTipoVisita = null;
        public LK_TipoVisita ObjTipoVisita
        {
            get
            {
                if (objTipoVisita == null)
                    objTipoVisita = new LK_TipoVisita();
                return objTipoVisita;
            }
            set { objTipoVisita = value; OnPropertyChanged(this, "ObjTipoVisita"); }
        }

        private ObservableCollection<TipoBiglietto> srcTicketTypes = null;
        public ObservableCollection<TipoBiglietto> SrcTicketTypes
        {
            get
            {
                if (srcTicketTypes == null)
                    srcTicketTypes = new ObservableCollection<TipoBiglietto>();
                return srcTicketTypes;
            }
            set
            {
                srcTicketTypes = value;
                OnPropertyChanged(this, "SrcTicketTypes");
            }
        }


        #endregion

        #region Classes
        public class TipoBiglietto : INotifyPropertyChanged
        {
            private string tipo;
            public string Tipo
            {
                get { return tipo; }
                set { tipo = value; OnPropertyChanged(this, "Tipo"); }
            }

            private bool attivo;
            public bool Attivo
            {
                get { return attivo; }
                set { attivo = value; OnPropertyChanged(this, "Attivo"); }
            }

            private bool prezzoIsEnabled;
            public bool PrezzoIsEnabled
            {
                get { return prezzoIsEnabled; }
                set { prezzoIsEnabled = value; OnPropertyChanged(this, "PrezzoIsEnabled"); }
            }

            private decimal ? prezzo;
            public decimal ? Prezzo
            {
                get { return prezzo; }
                set { prezzo = value; OnPropertyChanged(this, "Prezzo"); }
            }

            #region Public Events

            public event PropertyChangedEventHandler PropertyChanged;

            #endregion

            #region Events Handling

            private void OnPropertyChanged(object sender, string propertyName)
            {
                if (PropertyChanged != null)
                    PropertyChanged(sender, new PropertyChangedEventArgs(propertyName));

                switch (propertyName)
                {
                    case "Attivo":
                        PrezzoIsEnabled =  (Tipo != "Omaggio" && Attivo);
                        if (!Attivo)
                            Prezzo = null;
                        break;

                    case "Tipo":
                        PrezzoIsEnabled = (Tipo != "Omaggio" && Attivo);
                        break;
                }
            }

            #endregion
        }
        #endregion

        #region Constructors

        public TipoVisitaViewModel(int detailID)
        {
            if (detailID > 0)
                ObjTipoVisita = dal.GetItem(detailID);

            LoadTicketTypes();
        }

        #endregion

        #region Methods

        private void LoadTicketTypes()
        {
            List<TipoBiglietto> tipiBiglietto = new List<TipoBiglietto>();
            TipoBiglietto tb = new TipoBiglietto { Tipo = "Intero" };
            tipiBiglietto.Add(tb);
            tb = new TipoBiglietto { Tipo = "Ridotto" };
            tipiBiglietto.Add(tb);
            tb = new TipoBiglietto { Tipo = "Scontato" };
            tipiBiglietto.Add(tb);
            tb = new TipoBiglietto { Tipo = "Omaggio" };
            tipiBiglietto.Add(tb);
            tb = new TipoBiglietto { Tipo = "Cumulativo" };
            tipiBiglietto.Add(tb);

            if (ObjTipoVisita.Id_TipoVisita > 0)
            {

                    for (int i = 0; i < tipiBiglietto.Count; i++)
                    {
                        switch (tipiBiglietto[i].Tipo)
                        {
                            case "Intero":
                                tipiBiglietto[i].Attivo = ObjTipoVisita.Interi;
                                tipiBiglietto[i].Prezzo = ObjTipoVisita.PrezzoIntero;
                                break;

                            case "Ridotto":
                                tipiBiglietto[i].Attivo = ObjTipoVisita.Ridotti;
                                tipiBiglietto[i].Prezzo = ObjTipoVisita.PrezzoRidotto;
                                break;

                            case "Scontato":
                                tipiBiglietto[i].Attivo = ObjTipoVisita.Scontati;
                                tipiBiglietto[i].Prezzo = ObjTipoVisita.PrezzoScontato;
                                break;

                            case "Omaggio":
                                tipiBiglietto[i].Attivo = ObjTipoVisita.Omaggio;
                                break;

                            case "Cumulativo":
                                tipiBiglietto[i].Attivo = ObjTipoVisita.Cumulativo;
                                tipiBiglietto[i].Prezzo = ObjTipoVisita.PrezzoCumulativo;
                                break;
                        }
                    }
               
            }

            SrcTicketTypes = new ObservableCollection<TipoBiglietto>(tipiBiglietto);
        }



        #endregion

        #region Public Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Events Handling

        private void OnPropertyChanged(object sender, string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(sender, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}