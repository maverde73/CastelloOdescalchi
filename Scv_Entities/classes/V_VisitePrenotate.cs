using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects.DataClasses;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace Scv_Entities
{
	public partial class V_VisitePrenotate : EntityObject, INotifyPropertyChanged
	{
		private bool isEmpty = true;
		public bool IsEmpty
		{
			get { return isEmpty; }
			set { isEmpty = value; OnPropertyChanged(this, "IsEmpty"); }
		}

		private bool editing = false;
		public bool Editing
		{
			get { return editing; }
			set { editing = value; }
		}

		private bool isErasable = false;
		public bool IsErasable
		{
			get { return isErasable; }
			set { isErasable = value; OnPropertyChanged(this, "IsErasable"); }
		}

		private bool isLanguageEditable = true;
		public bool IsLanguageEditable
		{
			get { return isLanguageEditable; }
			set { isLanguageEditable = value; OnPropertyChanged(this, "IsLanguageEditable"); }
		}

		private bool isReadonlyLanguage = false;
		public bool IsReadonlyLanguage
		{
			get { return isReadonlyLanguage; }
			set { isReadonlyLanguage = value; OnPropertyChanged(this, "IsReadonlyLanguage"); }
		}

		private bool isNumericEditable = true;
		public bool IsNumericEditable
		{
			get { return isNumericEditable; }
			set { isNumericEditable = value; OnPropertyChanged(this, "IsNumericEditable"); }
		}

		private bool isLoadedFromDb = false;
		public bool IsLoadedFromDb
		{
			get { return isLoadedFromDb; }
			set { isLoadedFromDb = value; OnPropertyChanged(this, "IsLoadedFromDb"); }
		}

		private ObservableCollection<LK_Lingua> availableLanguages = null;
		public ObservableCollection<LK_Lingua> AvailableLanguages
		{
			get
			{
				if (availableLanguages == null)
					availableLanguages = new ObservableCollection<LK_Lingua>();
				return availableLanguages;
			}
			set { availableLanguages = value; OnPropertyChanged(this, "AvailableLanguages"); }
		}

		private LK_Lingua selectedLanguage = null;
		public LK_Lingua SelectedLanguage
		{
			get { return selectedLanguage; }
			set { selectedLanguage = value; OnPropertyChanged(this, "SelectedLanguage"); }
		}

		partial void OnNr_VisitatoriChanged()
		{
			OnPropertyChanged(this, "Nr_Visitatori");
		}

		partial void OnId_LinguaChanged()
		{
			OnPropertyChanged(this, "Id_Lingua");
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
