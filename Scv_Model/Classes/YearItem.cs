using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Scv_Model
{
	public class YearItem : INotifyPropertyChanged
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
			get { return Number.ToString(); }
		}

		#endregion// Public Properties



		#region Constructors

		public YearItem() { }

		public YearItem(int number)
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
