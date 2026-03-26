using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects.DataClasses;
using System.ComponentModel;

namespace Scv_Entities
{
    public partial class V_TipoVisita_TipoBiglietto_Grouped : EntityObject, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(object sender, string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(sender, new PropertyChangedEventArgs(propertyName));

            if (propertyName == "mese")
            {
                switch (mese)
                {
                    case 1:
                        meseDescrizione = "gennaio";
                        break;

                    case 2:
                        meseDescrizione = "febbraio";
                        break;

                    case 3:
                        meseDescrizione = "marzo";
                        break;

                    case 4:
                        meseDescrizione = "aprile";
                        break;

                    case 5:
                        meseDescrizione = "maggio";
                        break;

                    case 6:
                        meseDescrizione = "giugno";
                        break;

                    case 7:
                        meseDescrizione = "luglio";
                        break;

                    case 8:
                        meseDescrizione = "agosto";
                        break;

                    case 9:
                        meseDescrizione = "settembre";
                        break;

                    case 10:
                        meseDescrizione = "ottobre";
                        break;

                    case 11:
                        meseDescrizione = "novembre";
                        break;

                    case 12:
                        meseDescrizione = "dicembre";
                        break;
                }
            }
        }

    }
}
