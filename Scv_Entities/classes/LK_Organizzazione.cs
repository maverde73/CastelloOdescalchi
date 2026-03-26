using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects.DataClasses;
using System.ComponentModel;

namespace Scv_Entities
{
	public partial class LK_Organizzazione : EntityObject, INotifyPropertyChanged
    {
        public LK_Organizzazione()
        {
            this.Descrizione = string.Empty;
            this.Nota = string.Empty;
            this.Id_Organizzazione = 0;
        }

		public event PropertyChangedEventHandler PropertyChanged;
		private void OnPropertyChanged(object sender, string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

    }
}
