using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects.DataClasses;
using System.ComponentModel;

namespace Scv_Entities
{
	public partial class LK_Chiusura : EntityObject, INotifyPropertyChanged
	{

		private DateTime date = DateTime.Now;

		public DateTime Date
		{
			get
			{
				return date;
			}
			set
			{
				date = value;
				OnPropertyChanged(this, "Date");
			}
		}

		partial void OnAnnoChanged()
		{
			Date = new DateTime(Anno, Date.Month, Date.Day);
		}

		partial void OnMeseChanged()
		{
            var dt = DateTime.Now;
            int month = Mese > 0 ? ((int)Mese)  : DateTime.Now.Month;
            int year = Date.Year;
            int day = Date.Day;
            try
            {
                Date = new DateTime(year, month, day);
            }
            catch (Exception ex)
            {
                Date = DateTime.Now.Date;
            }
		}

		partial void OnGiornoChanged()
		{
            try
            {
                Date = new DateTime(Date.Year, Date.Month, Giorno);
            }
            catch (Exception ex)
            {
                Date = DateTime.Now.Date;
            }
		}

		public event PropertyChangedEventHandler PropertyChanged;
		private void OnPropertyChanged(object sender, string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

	}
}
