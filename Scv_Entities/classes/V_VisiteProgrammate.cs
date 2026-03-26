using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects.DataClasses;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using Scv_Model;

namespace Scv_Entities
{
    public partial class V_VisiteProgrammate : EntityObject, IDataErrorInfo, INotifyPropertyChanged
    {
		private bool confirmedChecked = false;

        private bool isEmpty = true;
        public bool IsEmpty
        {
            get { return isEmpty; }
            set { isEmpty = value; OnPropertyChanged(this, "IsEmpty"); }
        }

        private bool isNew = false;
        public bool IsNew
        {
            get { return isNew; }
            set { isNew = value; OnPropertyChanged(this, "IsNew"); }
        }

        private bool editing = false;
        public bool Editing
        {
            get { return editing; }
			set { editing = value; }
        }

        private bool isErasable = true;
        public bool IsErasable
        {
            get { return isErasable; }
            set { isErasable = value; OnPropertyChanged(this, "IsErasable"); }
        }

        private bool isCancelable = true;
        public bool IsCancelable
        {
            get { return isCancelable; }
            set { isCancelable = value; OnPropertyChanged(this, "IsCancelable"); }
        }

		private bool isNumericEditable = true;
		public bool IsNumericEditable
		{
			get { return isNumericEditable; }
			set { isNumericEditable = value; OnPropertyChanged(this, "IsNumericEditable"); }
		}

		private ConflictType conflictType = ConflictType.NoConflict;
		public ConflictType ConflictType
		{
			get { return conflictType; }
			set { conflictType = value; OnPropertyChanged(this, "ConflictType"); }
		}

		public SolidColorBrush BgColor
		{
			get 
			{ 
				SolidColorBrush b = null;
				switch(ConflictType)
				{
					case Scv_Model.ConflictType.Unacceptable:
						b = new SolidColorBrush(Colors.LightSalmon);
						break;

					case Scv_Model.ConflictType.Acceptable:
						b = new SolidColorBrush(Colors.LightGreen);
						break;

					case Scv_Model.ConflictType.NoConflict:
						b = new SolidColorBrush(Colors.White);
						break;
				}

				return b;							
			}
		}

		partial void OnNr_InteriChanged()
		{
			OnNestedPropertyChanged(this, "Nr_Interi");
			OnPropertyChanged(this, "Nr_Interi");
		}

		partial void OnNr_RidottiChanged()
		{
			OnPropertyChanged(this, "Nr_Ridotti");
		}

		partial void OnNr_OmaggioChanged()
		{
			OnPropertyChanged(this, "Nr_Omaggio");
		}

		partial void OnOra_VisitaChanged()
		{
			OnNestedPropertyChanged(this, "Ora_Visita");
		}

        public V_VisiteProgrammate()
        {

        }

        public string Error
        {
			//get { throw new NotImplementedException(); }
			get { return string.Empty; }
		}

        public string this[string columnName]
        {
            get
            {
                string result = null;

                switch (columnName)
                {
                    case "Ora_Visita":
                        if (Editing && String.IsNullOrEmpty(this.Ora_Visita))
                            result = "Il campo 'Ora' è obbligatorio";
                        break;

                    //case "Nr_Interi":
                    //    if (Editing && Nr_Interi == null)
                    //        result = "Il campo 'I' è obbligatorio";
                    //    break;
                }

                return result;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(object sender, string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(sender, new PropertyChangedEventArgs(propertyName));
        }

		public event PropertyChangedEventHandler NestedPropertyChanged;
		private void OnNestedPropertyChanged(object sender, string propertyName)
		{
			if (NestedPropertyChanged != null)
				NestedPropertyChanged(sender, new PropertyChangedEventArgs(propertyName));
		}

    }
}
