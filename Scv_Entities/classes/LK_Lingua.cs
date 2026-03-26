using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects.DataClasses;
using System.ComponentModel;

namespace Scv_Entities
{
	public partial class LK_Lingua: EntityObject, IDataErrorInfo, INotifyPropertyChanged
	{
		#region Private Fields

		bool isSelected = false;

		bool isDefault = false;

		#endregion// Private Fields



		#region Public Properties

		public bool IsSelected
		{
			get { return isSelected; }
			set { isSelected = value; OnPropertyChanged(this, "IsSelected"); }
		}

		public bool IsDefault
		{
			get { return isDefault; }
			set { isDefault = value; OnPropertyChanged(this, "IsDefault"); }
		}

		#endregion// Public Properties




		public LK_Lingua()
		{
			
		}

		public LK_Lingua(int languageID)
		{
			this.Id_Lingua = languageID;
		}


		public string  Error
		{
			//get { throw new NotImplementedException(); }
			get { return string.Empty; }
		}

		public string  this[string columnName]
		{
			get
			{
				string result = null;

				switch (columnName)
				{
					case "Id_Lingua":
						if(Id_Lingua < 1)
							result = "Il campo 'Lingua' è obbligatorio";
						break;
				}

				return result;
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
		private void OnPropertyChanged(object sender, string propertyName)
		{
			if(PropertyChanged != null)
				PropertyChanged(sender, new PropertyChangedEventArgs(propertyName));
		}
	}
	
}
