using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Scv_Model
{
	public class MoneyCut : INotifyPropertyChanged
	{
		#region Private Fields

		private int cut = 0;
		private int pieces = 0;
		private decimal total = 0;

		#endregion// Private Fields


		#region Public Properties

		public int Cut 
		{
			get { return cut; }
			set { cut = value; OnPropertyChanged(this, "Cut"); }
		
		}
		public int Pieces 
		{
			get { return pieces; }
			set { pieces = value; OnPropertyChanged(this, "Pieces"); }
		}

		public decimal Total
		{
			get { return Cut * Pieces; }
		}

		#endregion// Public Properties



		#region Constructors

		public MoneyCut() { }

		public MoneyCut(int cut)
		{
			this.Cut = cut;
		}

		#endregion// Constructors

		public event PropertyChangedEventHandler PropertyChanged;
		private void OnPropertyChanged(object sender, string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(sender, new PropertyChangedEventArgs(propertyName));
		}
	}
}
