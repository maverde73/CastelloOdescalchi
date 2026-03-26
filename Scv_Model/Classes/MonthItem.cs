using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Scv_Model
{
	public class MonthItem : INotifyPropertyChanged
	{
		#region Private Fields

		private int number = 0;

		#endregion// Private Fields

		#region Public Properties

		public int Number 
		{
			get { return number; }
			set { number = value; OnPropertyChanged("Number"); }
		
		}

		public string Description 
		{
			get
			{
				string monthDescription = string.Empty;
				switch (Number)
				{
					case 1:
						monthDescription = "Gennaio";
						break;
					case 2:
						monthDescription = "Febbraio";
						break;
					case 3:
						monthDescription = "Marzo";
						break;
					case 4:
						monthDescription = "Aprile";
						break;
					case 5:
						monthDescription = "Maggio";
						break;
					case 6:
						monthDescription = "Giugno";
						break;
					case 7:
						monthDescription = "Luglio";
						break;
					case 8:
						monthDescription = "Agosto";
						break;
					case 9:
						monthDescription = "Settembre";
						break;
					case 10:
						monthDescription = "Ottobre";
						break;
					case 11:
						monthDescription = "Novembre";
						break;
					case 12:
						monthDescription = "Dicembre";
						break;
				}

				return monthDescription;
			}
		}

		#endregion// Public Properties



		#region Constructors

		public MonthItem() { }

		public MonthItem(int number)
		{
			this.Number = number;
		}

		#endregion// Constructors

		public event PropertyChangedEventHandler PropertyChanged;
		private void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
