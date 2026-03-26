using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows;
using Scv_Dal;
using Scv_Entities;
using System.Collections.ObjectModel;
using Scv_Model;
using System.Windows.Media;

namespace Presentation
{
	public class GuideAssignmentViewModel : INotifyPropertyChanged
	{
		#region Public Events

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion// Public Events



		#region Private Variables

		private VisitaProgrammata_Dal dalScheduled = new VisitaProgrammata_Dal();
		public GuidaDisponibile_Dal dalAvailableGuides = new GuidaDisponibile_Dal();
		public Guida_Dal dalGuides = new Guida_Dal();
		private Parametri_Dal dalParametri = new Parametri_Dal();

		#endregion// PRivate Variables



		#region Private Fields

		private BaseFilter guidesFilter = null;

		private BaseFilter visitsFilter = null;

		private List<V_EvidenzeGiornaliere> srcEvidenzeGiornaliere = null;

		private List<V_EvidenzeGiornaliere> srcOldEvidenzeGiornaliere = null;

		private bool forwardToSenderGuide = false;

		private bool forwardToSenderVisitor = false;

		private V_EvidenzeGiornaliere selectedItem = null;

		private int pgValue = 0;

		private bool isEnabled = true;

		#endregion// Private Fields



		#region Public Properties

		public BaseFilter GuidesFilter
		{
			get
			{
				if (guidesFilter == null)
					guidesFilter = new BaseFilter();
				return guidesFilter;
			}
			set { guidesFilter = value; OnPropertyChanged(this, "GuidesFilter"); }
		}

		public BaseFilter VisitsFilter
		{
			get
			{
				if (visitsFilter == null)
					visitsFilter = new BaseFilter();
				return visitsFilter;
			}
			set { visitsFilter = value; OnPropertyChanged(this, "VisitsFilter"); }
		}

		public List<V_EvidenzeGiornaliere> SrcEvidenzeGiornaliere
		{
            get 
			{
				if (srcEvidenzeGiornaliere == null)
					srcEvidenzeGiornaliere = new List<V_EvidenzeGiornaliere>();					
				return srcEvidenzeGiornaliere; 			
			}
			set { srcEvidenzeGiornaliere = value; OnPropertyChanged(this, "SrcEvidenzeGiornaliere"); }
		}

		public List<V_EvidenzeGiornaliere> SrcOldEvidenzeGiornaliere
		{
			get
			{
				if (srcOldEvidenzeGiornaliere == null)
					srcOldEvidenzeGiornaliere = new List<V_EvidenzeGiornaliere>();
				return srcOldEvidenzeGiornaliere;
			}
			set
			{
				srcOldEvidenzeGiornaliere = value;
				OnPropertyChanged(this, "SrcOldEvidenzeGiornaliere");
			}
		}

		public bool ForwardToSenderGuide
		{
			get { return forwardToSenderGuide; }
		}

		public bool ForwardToSenderVisitor
		{
			get { return forwardToSenderVisitor; }
		}

		public V_EvidenzeGiornaliere SelectedItem
		{
			get { return selectedItem; }
			set { selectedItem = value; OnPropertyChanged(this, "SelectedItem"); }
		}

		public int PgValue
		{
			get { return pgValue; }
			set { pgValue = value; OnPropertyChanged(this, "PgValue"); }
		}

		public bool IsEnabled
		{
			get { return isEnabled; }
			set { isEnabled = value; OnPropertyChanged(this, "IsEnabled"); }
		}

		#endregion // Public Properties



		#region Constructors

		public GuideAssignmentViewModel()
		{
			bool.TryParse(dalParametri.GetItem("forwardToSenderGuide").Valore, out forwardToSenderGuide);
			bool.TryParse(dalParametri.GetItem("forwardToSenderVisitor").Valore, out forwardToSenderVisitor);
			dalScheduled.GuideAssignmentUpdated += new CommonDelegates.GuideAssignmentEventHandler(dalScheduled_GuideAssignmentUpdated);
		}

		#endregion// Constructors



		#region Event Handling

		private void OnPropertyChanged(object sender, string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(sender, new PropertyChangedEventArgs(propertyName));

			switch (propertyName)
			{
				case "SelectedItem":
					if(SelectedItem != null)
					{
						SelectedItem.AvailableGuides = new ObservableCollection<V_GuideDisponibili>(dalAvailableGuides.GetV_GuideDisponibili(SelectedItem));
					}
					break;
			}
		}

		public void OnScheduledVisitNestedPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			V_EvidenzeGiornaliere obj = sender as V_EvidenzeGiornaliere;
			if (obj != null)
			{
				obj.NestedPropertyChanged -= new PropertyChangedEventHandler(OnScheduledVisitNestedPropertyChanged);
				switch (e.PropertyName)
				{
					case "Id_Guida":
						try
						{
							//Abilitazione della casella "Avvisa" in base all'ID della guida (id==0 o null = disabilitata) 
							obj.IsAvvisaEnabled = GetAvvisaGuida(obj);

							//Se si disabilita la casella di avviso, significa che la guida è N.D. o null, quindi si svuota la casella "Accetta"
							if (!obj.IsAvvisaEnabled)
							{
								obj.Fl_AvvisaGuida = false;
								obj.Fl_AccettaGuida = false;
							}

							//Quando si seleziona una guida è necessario impostarne il nominativo qui, per provocare l'aggiornamento
							//delle nested properties Cognome e Nome, che vengono così mostrate sia in edit (ComboBox) che in modalità
							//readonly della griglia (TextBlock)
							if (obj.Id_Guida != null && obj.AvailableGuides.Count > 0)
							{
								V_GuideDisponibili g = obj.AvailableGuides.FirstOrDefault(x => x.Id_Guida == obj.Id_Guida);
								if (g != null)
								{
									obj.Cognome = g.Cognome;
									obj.Nome = g.Nome;
								}
							}
						}
						catch
						{

						}
						break;
				}

				obj.GuideForeground = GetGuideColor(obj);

				obj.NestedPropertyChanged += new PropertyChangedEventHandler(OnScheduledVisitNestedPropertyChanged);
			}
		}

		private SolidColorBrush GetGuideColor(V_EvidenzeGiornaliere obj)
		{
			return ((obj.Id_Guida != null && obj.Id_Guida > 0) && (obj.Fl_AccettaGuida == false || obj.Fl_AccettaGuida == null)) ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.Black);
		}

		private bool GetAvvisaGuida(V_EvidenzeGiornaliere obj)
		{
			return (obj.Id_Guida == null || obj.Id_Guida == 0) ? false : true;
		}

		void dalScheduled_GuideAssignmentUpdated(object sender, GuideAssignmentEventArgs e)
		{
			PgValue = e.PercentDone;
		}

		#endregion// Event Handling



		#region Main Methods

		public void BindMaster(BaseFilter filter)	
		{
			int count = 0;
			SrcEvidenzeGiornaliere = dalScheduled.GetV_EvidenzeGiornaliereGroup(filter.Args, filter.Sort, filter.SortDirection.ToString(), 0, 0, true, out count).ToList();
			SrcEvidenzeGiornaliere.ToList().ForEach(x => x.GuideForeground = GetGuideColor(x));
			SrcEvidenzeGiornaliere.ToList().ForEach(x => x.IsAvvisaEnabled = GetAvvisaGuida(x));
			SrcEvidenzeGiornaliere.ToList().ForEach(x => x.NestedPropertyChanged += new PropertyChangedEventHandler(OnScheduledVisitNestedPropertyChanged));

			SrcOldEvidenzeGiornaliere.Clear();
			foreach (V_EvidenzeGiornaliere e in SrcEvidenzeGiornaliere)
				SrcOldEvidenzeGiornaliere.Add(e);
		}

		public void DoUpdateGuides(object sender, DoWorkEventArgs e)
		{
			dalScheduled.UpdateVisitsGuides(SrcEvidenzeGiornaliere);
		}

		#endregion // Main Methods



		#region Guides

		private ObservableCollection<Guida> availableGuides = null;
		public ObservableCollection<Guida> AvailableGuides
		{
			get
			{
				if (availableGuides == null)
					availableGuides = new ObservableCollection<Guida>();
				return availableGuides;
			}
			set { availableGuides = value; }
		}

		private List<Guida> availableGuidesFilter = null;
		public List<Guida> AvailableGuidesFilter
		{
			get
			{
				if (availableGuidesFilter == null)
					availableGuidesFilter = new List<Guida>();
				return availableGuidesFilter;
			}
			set { availableGuidesFilter = value; OnPropertyChanged(this, "AvailableGuidesFilter"); }
		}

		public List<Guida> LoadAvailableGuidesFilter(BaseFilter filter)
		{
			int count  = 0;
			List<Guida> tmpList = dalGuides.GetFilteredList(filter.Args, filter.Sort, filter.SortDirection.ToString(), filter.PageSize, filter.PageNumber, out count);
			Guida o = new Guida();
			o.Id_Guida = 0;
			o.Cognome = "Tutte";
			tmpList.Insert(0, o);
			AvailableGuidesFilter = tmpList;
			return AvailableGuidesFilter;
		}

		#endregion// Guides
	}
}
