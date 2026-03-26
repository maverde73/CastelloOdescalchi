using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects.DataClasses;
using System.ComponentModel;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System.Windows;

namespace Scv_Entities
{
	public partial class V_EvidenzeGiornaliereGroup : EntityObject, IDataErrorInfo, INotifyPropertyChanged
	{
		#region Events

		public event PropertyChangedEventHandler PropertyChanged;

		public event PropertyChangedEventHandler NestedPropertyChanged;

		#endregion// Events



		#region Private Fields

		private bool isEmpty = true;
		private bool editing = false;
		private bool isErasable = false;
		private bool isParentItem = false;
		private bool isAvvisaEnabled = false;
		private int parentItemID = 0;
		private bool isReadOnlyItem = false;
		private int totaleVisitatoriGruppo = 0;
		private bool canAccept = false;
		private bool canNotify = false;
		private bool fl_AvvisaGuida = false;
		private SolidColorBrush guideForeground = null;
		private V_GuideDisponibili selectedGuide = null;

		private List<Pagamento> payments = null;

		private ObservableCollection<V_GuideDisponibili> availableGuides = null;

		#endregion// Private Fields


		#region Public Properties

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

		public bool IsParentItem
		{
			get { return isParentItem; }
			set { isParentItem = value; OnPropertyChanged(this, "IsParentItem"); }
		}

		public int ParentItemID
		{
			get { return parentItemID; }
			set { parentItemID = value; OnPropertyChanged(this, "ParentItemID"); }
		}

		public bool IsReadOnlyItem
		{
			get { return isReadOnlyItem; }
			set { isReadOnlyItem = value; OnPropertyChanged(this, "IsReadOnlyItem"); }
		}

		public bool IsAvvisaEnabled
		{
			get { return isAvvisaEnabled; }
			set
			{
				isAvvisaEnabled = value;
				OnPropertyChanged(this, "IsAvvisaEnabled");
				OnNestedPropertyChanged(this, "IsAvvisaEnabled");
			}
		}

		public bool Fl_AvvisaGuida
		{
			get { return fl_AvvisaGuida; }
			set
			{
				fl_AvvisaGuida = value;
				OnPropertyChanged(this, "Fl_AvvisaGuida");
				OnNestedPropertyChanged(this, "Fl_AvvisaGuida");
			}
		}

		public bool ShowOra
		{
			get { return !IsParentItem; }
		}

		public bool ShowGuida
		{
			get { return !IsParentItem; }
		}

		public bool ShowLingua
		{
			get { return !IsParentItem; }
		}

		public bool ShowTipoVisita
		{
			get { return !IsParentItem; }
		}

		public bool ShowTotaleVisitatori
		{
			get { return !IsParentItem; }
		}

		public bool ShowAvvisa
		{
			get { return !IsParentItem; }
		}

		public bool ShowAccetta
		{
			get { return !IsParentItem; }
		}



		public string Nominativo
		{
			get { return Cognome + " " + Nome; }
		}

		public V_GuideDisponibili SelectedGuide
		{
			get { return selectedGuide; }
			set { selectedGuide = value; OnPropertyChanged(this, "SelectedGuide"); }
		}

		public DateTime? VisitDate
		{
			get
			{
				return Dt_Visita != DateTime.MinValue ? Dt_Visita : (DateTime?)null;
			}
		}


		public ObservableCollection<V_GuideDisponibili> AvailableGuides
		{
			get
			{
				if (availableGuides == null)
					availableGuides = new ObservableCollection<V_GuideDisponibili>();
				return availableGuides;
			}
			set { availableGuides = value; OnPropertyChanged(this, "AvailableGuides"); }
		}

		public bool CanAccept
		{
			get { return Id_Guida != null && Id_Guida > 0; }
		}

		public bool CanNotify
		{
			get { return Id_Guida != null && Id_Guida > 0; }
		}

		public SolidColorBrush GuideForeground
		{
			get
			{
				if (guideForeground == null)
					guideForeground = new SolidColorBrush(Colors.Black);
				return guideForeground;
			}
			set
			{
				guideForeground = value;
				OnPropertyChanged(this, "GuideForeground");
				OnNestedPropertyChanged(this, "GuideForeground");
			}
		}

		public Visibility ColumnVisibility
		{
			get { return IsParentItem ? Visibility.Visible : Visibility.Hidden; }
		}

		private SolidColorBrush bgColor = new SolidColorBrush(Colors.White);
		public SolidColorBrush BgColor
		{
			get { return bgColor; }
			set { bgColor = value; OnPropertyChanged(this, "BgColor"); }
		}

		#endregion// Public Properties


		partial void OnFl_AccettaGuidaChanged()
		{
			OnPropertyChanged(this, "Fl_AccettaGuida");
			OnNestedPropertyChanged(this, "Fl_AccettaGuida");
		}

		partial void OnId_GuidaChanged()
		{
			OnNestedPropertyChanged(this, "Id_Guida");
		}

		#region Error Handler

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
					//case "Ora_Visita":
					//    if (Editing && String.IsNullOrEmpty(this.Ora_Visita))
					//        result = "Il campo 'Ora' è obbligatorio";
					//    break;

					//case "Nr_Interi":
					//    if (Editing && Nr_Interi == null)
					//        result = "Il campo 'I' è obbligatorio";
					//    break;
				}

				return result;
			}
		}

		#endregion// Error Handler



		#region Event Handlers

		private void OnPropertyChanged(object sender, string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(sender, new PropertyChangedEventArgs(propertyName));
		}

		private void OnNestedPropertyChanged(object sender, string propertyName)
		{
			if (NestedPropertyChanged != null)
				NestedPropertyChanged(sender, new PropertyChangedEventArgs(propertyName));
		}

		#endregion// Event Handlers

	}
}
