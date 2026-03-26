using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects.DataClasses;
using System.ComponentModel;

namespace Scv_Entities
{
    public partial class Biglietto : EntityObject, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string fullDescription = "";
        public string FullDescription
        {
            get 
            {
               
                string tipo = "Intero";
                switch (this.Id_TipoBiglietto)
                {
                    case 1:
                        tipo = "Intero";
                        break;

                    case 2:
                        tipo = "Ridotto";
                        break;

                    case 3:
                        tipo = "Scontato";
                        break;

                    case 4:
                        tipo = "Cumulativo";
                        break;

                    case 5:
                        tipo = "Omaggio";
                        break;
                }

                fullDescription = this.Codice + " - " + tipo +  ((this.Annullato) ? " - annullato" : "");
                return fullDescription; 
            }
            set { fullDescription = value; OnPropertyChanged(this, "FullDescription"); }
        }

        private void OnPropertyChanged(object sender, string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(sender, new PropertyChangedEventArgs(propertyName));

            //switch (propertyName)
            //{
            //    case "Numero":
            //        decimal taglio = 0;
            //        decimal.TryParse(Taglio.Replace(".", ","), out taglio);
            //        if (IsValueReadOnly && taglio > 0)
            //            Valore = Numero * taglio;
            //        break;
            //}


        }
    }
}
