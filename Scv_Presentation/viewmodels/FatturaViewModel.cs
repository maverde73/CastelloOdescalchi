using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows;
using Scv_Entities;
using Scv_Dal;
using System.Collections.ObjectModel;

namespace Presentation
{
    public class FatturaViewModel
    {
        #region Variables
        Richiedente_Dal richiedente_Dal = new Richiedente_Dal();
        Prenotazione_Dal prenotazione_Dal = new Prenotazione_Dal();
        VisitaProgrammata_Dal visitaProgrammata_Dal = new VisitaProgrammata_Dal();
        #endregion

        #region Properties
        #endregion

        #region Methods
        public V_Prenotazione getPrenotazione(int idPrenotazione)
        {
            return prenotazione_Dal.Get_V_Item(idPrenotazione);
        }

        public V_RichiedenteFull getRichiedente(int idRichiedente)
        {
            return richiedente_Dal.GetVItem(idRichiedente);
        }

        public DateTime getDataVisita(int idPrenotazione)
        {
            DateTime dtVisita = DateTime.Now.Date;
            var vprogs = visitaProgrammata_Dal.GetVListByIdPrenotazione(idPrenotazione);
            if (vprogs.Count > 0)
                dtVisita = vprogs[0].Dt_Visita;
            return dtVisita;
        }
        #endregion

        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Event Handling
        private void OnPropertyChanged(object sender, string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(sender, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
