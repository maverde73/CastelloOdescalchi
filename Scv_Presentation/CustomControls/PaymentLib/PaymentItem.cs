using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Presentation.CustomControls.PaymentLib
{
	public class PaymentItem : INotifyPropertyChanged
	{
		#region Public Events

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion// Public Events



		#region Private Fields

		private int id = 0;

		private int paymentTypeID = 0;

		private string paymentType = string.Empty;

		private decimal amount = 0;

		private bool isEmpty = true;

		private bool editing = false;

		private bool isErasable = false;

		#endregion// Private Fields



		#region Public Properties

		public int ID
		{
			get { return id; }
			set { id = value; OnPropertyChanged(this, "ID"); }
		}

		public int PaymentTypeID
		{
			get { return paymentTypeID; }
			set { paymentTypeID = value; OnPropertyChanged(this, "PaymentTypeID"); }
		}

		public string PaymentType
		{
			get { return paymentType; }
			set { paymentType = value; OnPropertyChanged(this, "PaymentType"); }
		}

		public decimal Amount
		{
			get { return amount; }
			set { amount = value; OnPropertyChanged(this, "Amount"); }
		}

		public bool IsEmpty
		{
			get { return isEmpty; }
			set { isEmpty = value; OnPropertyChanged(this, "IsEmpty"); }
		}

		public bool Editing
		{
			get { return editing; }
			set { editing = value; }
		}

		public bool IsErasable
		{
			get { return isErasable; }
			set { isErasable = value; OnPropertyChanged(this, "IsErasable"); }
		}

		#endregion// Public Properties


		#region Event Handlers

		private void OnPropertyChanged(object sender, string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(sender, new PropertyChangedEventArgs(propertyName));
		}

		#endregion// Event Handlers

	}
}
