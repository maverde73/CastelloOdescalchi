using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Scv_Dal;
using System.Collections.ObjectModel;
using Scv_Entities;
using Telerik.Windows.Data;
using System.Windows;
using Scv_Model;

namespace Presentation
{
	class GuideMovementViewModel : INotifyPropertyChanged
	{
		#region Public Events

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion// Public Events



		#region Properties

		bool designTime = System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject());

		public Movimento_Dal dalMovement = new Movimento_Dal();
		public LK_Progressivi_Dal dalProgressives = new LK_Progressivi_Dal();
		public LK_TipoMovimento_Dal dalMovementType = new LK_TipoMovimento_Dal();
		public LK_TipoPagamento_Dal dalPaymentType = new LK_TipoPagamento_Dal();
		public LK_TipoArticolo_Dal dalArticleType = new LK_TipoArticolo_Dal();
		public Articolo_Dal dalArticle = new Articolo_Dal();
		public EsercizioVendita_Dal dalStore = new EsercizioVendita_Dal();
		public Guida_Dal dalGuide = new Guida_Dal();

        private List<Movimento> srcMovement = null;
		public List<Movimento> SrcMovement
		{
            get 
			{
				if (srcMovement == null)
					srcMovement = new List<Movimento>() { new Movimento() };					
				return srcMovement; 			
			}
			set { srcMovement = value; OnPropertyChanged(this, "SrcMovement"); }
		}

		private List<LK_Progressivi> srcProgressives = null;
		public List<LK_Progressivi> SrcProgressives
		{
			get
			{
				if (srcProgressives == null)
					srcProgressives = new List<LK_Progressivi>() { new LK_Progressivi() };
				return srcProgressives;
			}
			set { srcProgressives = value; OnPropertyChanged(this, "SrcProgressives"); }
		}

        private ObservableCollection<Movimento> srcMovementDetails =  null;
        public ObservableCollection<Movimento> SrcMovementDetails
		{
			get
			{
                if (srcMovementDetails == null)
                    srcMovementDetails = new ObservableCollection<Movimento>();
				return srcMovementDetails;
			}
			set { srcMovementDetails = value; OnPropertyChanged(this, "SrcMovementDetails"); }
		}

		private Movimento objMovement = null;
		public Movimento ObjMovement
		{
			get
			{
				if (objMovement == null)
					objMovement = new Movimento();
				return objMovement;
			}
			set { objMovement = value; OnPropertyChanged(this, "ObjMovement"); }
		}

		private LK_Progressivi objProgressives = null;
		public LK_Progressivi ObjProgressives
		{
			get
			{
				if (objProgressives == null)
					objProgressives = new LK_Progressivi();
				return objProgressives;
			}
			set { objProgressives = value; OnPropertyChanged(this, "ObjProgressives"); }
		}

		private decimal objPrice = 0;
		public decimal ObjPrice
		{
			get { return objPrice; }
			set { objPrice = value; OnPropertyChanged(this, "ObjPrice"); }
		}
		#endregion // Properties



		#region Constructors

		public GuideMovementViewModel(int detailID, List<int> allowedpaymentTypeIDs)
		{
			LoadAvailableMovementTypes();
			LoadAvailablePaymentTypes(allowedpaymentTypeIDs);
			LoadAvailableGuides();

			if (detailID > 0)
			{
				SrcMovement = dalMovement.GetMovementByIdentifier(detailID);
				SrcMovementDetails = new ObservableCollection<Movimento>();
			}
			else
			{				
				Movimento mov = new Movimento();
				mov.Id_TipoMovimento = availableMovementTypes.First().Id_TipoMovimento;
                mov.Id_TipoPagamento = (int?)null;
				mov.Id_EsercizioVendita = 0;
				mov.Id_Guida = 0;
				mov.Dt_Movimento = DateTime.Now;               

				SrcMovement = new List<Movimento>() { mov };

				GetProgressiveByMovementType(SrcMovement[0].Id_TipoMovimento);

				SrcMovementDetails = new ObservableCollection<Movimento>();
			}

			ObjMovement = SrcMovement[0];

			ObjProgressives = SrcProgressives[0];

			//Default empy detail for begin editing the grid
			AddNewDetailRow();
		}

		#endregion// Constructors



		#region Event Handling

		private void OnPropertyChanged(object sender, string propertyName)
		{

			switch (propertyName)
			{
				case "Id_TipoMovimento":

					break;
			}

			if (PropertyChanged != null)
				PropertyChanged(sender, new PropertyChangedEventArgs(propertyName));
		}

		#endregion// Event Handling



		#region Main Methods

        public List<Movimento> GetPrenotationSingleItem(int id)
        {
            return dalMovement.GetSingleItem(id);
        }

		public void GetProgressiveByMovementType(int movementTypeID)
		{
			LK_TipoMovimento tm = dalMovementType.GetSingleItem(movementTypeID)[0];
			if (tm.Simbolo != null)
			{
				LK_Progressivi p = dalProgressives.GetProgressiviBySymbol(tm.Simbolo);
				SrcProgressives[0] = p != null ? p : new LK_Progressivi();
				if (SrcProgressives[0].Anno == 0 || SrcProgressives[0].Anno == DateTime.Now.Year)
				{
					SrcProgressives[0].Progr_UltimoUscito++;
				}
				else if (SrcProgressives[0].Anno != DateTime.Now.Year)
				{
					SrcProgressives[0].Anno = DateTime.Now.Year;
					srcProgressives[0].Progr_UltimoUscito = 1;
				}
			}
			else
			{
				SrcProgressives = new List<LK_Progressivi>() { new LK_Progressivi() }; ;
			}

			ObjProgressives = SrcProgressives[0];
		}

		#endregion // Main Methods



		#region Details Methods

		public void AddNewDetailRow()
		{
			Movimento nv = new Movimento();
			nv.Id_Movimento = CreateMovementID();
			SrcMovementDetails.Add(nv);
		}

		public void RemoveDetailRow(int movementID)
		{
			foreach (Movimento v in SrcMovementDetails)
			{
				if (v.Id_Movimento == movementID && v.Id_Articolo != null)
				{
					int id = (int)v.Id_Articolo;
					SrcMovementDetails.Remove(v);
					EsistenzaArticolo ea = GetStorage(id);
					SrcMovement[0].EsistenzaMagazzino = ea.EsistenzaMagazzino;
					SrcMovement[0].EsistenzaUfficio = ea.EsistenzaUfficio;

					break;
				}
			}
		}

		private int CreateMovementID()
		{
			return SrcMovementDetails.Count > 0 ? SrcMovementDetails.Max(x => x.Id_Movimento) + 1 : 1;
		}


		public EsistenzaArticolo GetStorage(int id)
		{
			EsistenzaArticolo ea = new EsistenzaArticolo();
			List<V_Movimento> list = new List<V_Movimento>();

			using (Scv_Entities.IN_VIAEntities _cntx = new IN_VIAEntities())
				list = _cntx.V_Movimento.Where(x => x.Id_Articolo == id).ToList();

			foreach (V_Movimento m in list)
			{
				switch (m.Segno_Maga)
				{
					case "+":
						ea.EsistenzaMagazzino += (int)m.Nr_Pezzi;
						break;
					case "-":
						ea.EsistenzaMagazzino -= (int)m.Nr_Pezzi;
						break;
				}
				switch (m.Segno_Uff)
				{
					case "+":
						ea.EsistenzaUfficio += (int)m.Nr_Pezzi;
						break;
					case "-":
						ea.EsistenzaUfficio -= (int)m.Nr_Pezzi;
						break;
				}
			}

			foreach (Movimento m in SrcMovementDetails)
			{
				if (m.Id_TipoMovimento > 0)
				{
					LK_TipoMovimento tm = dalMovementType.GetSingleItem(m.Id_TipoMovimento)[0];
					switch (tm.Segno_Maga)
					{
						case "+":
							ea.EsistenzaMagazzino += m.Nr_Pezzi!= null ? (int)m.Nr_Pezzi : 0;
							break;
						case "-":
							ea.EsistenzaMagazzino -= m.Nr_Pezzi != null ? (int)m.Nr_Pezzi : 0;
							break;
					}
					switch (tm.Segno_Uff)
					{
						case "+":
							ea.EsistenzaUfficio += m.Nr_Pezzi != null ? (int)m.Nr_Pezzi : 0;
							break;
						case "-":
							ea.EsistenzaUfficio -= m.Nr_Pezzi != null ? (int)m.Nr_Pezzi : 0;
							break;
					}
				}
			}

			return ea;
		}

		#endregion// Visitd Methods



        #region Movememnt Types

        private ObservableCollection<LK_TipoMovimento> availableMovementTypes = null;
		public ObservableCollection<LK_TipoMovimento> AvailableMovementTypes
        {
            get
            {
				if (availableMovementTypes == null)
					availableMovementTypes = new ObservableCollection<LK_TipoMovimento>();
				return availableMovementTypes;
            }
			set { availableMovementTypes = value; }
        }

		private void LoadAvailableMovementTypes()
        {
			BaseFilter filter = new BaseFilter();
			filter.SortDirection = SortDirection.ASC;
			filter.AddSortField("Descrizione");
			filter.SetFilter("Fl_Pagamento", Scv_Model.Common.Utilities.ValueType.Bool, true);
			filter.SetFilter("Fl_Articolo", Scv_Model.Common.Utilities.ValueType.Bool, false);
			int count = 0;
			AvailableMovementTypes = new ObservableCollection<LK_TipoMovimento>(dalMovementType.GetFilteredList(filter.Args, filter.Sort, filter.SortDirection.ToString(), filter.PageSize, filter.PageNumber, out count));
        }
        
        #endregion// Movement Types



        #region Payment Types

        private ObservableCollection<LK_TipoPagamento> availablePaymentTypes = null;
		public ObservableCollection<LK_TipoPagamento> AvailablePaymentTypes
        {
            get
            {
                if (availablePaymentTypes == null)
					availablePaymentTypes = new ObservableCollection<LK_TipoPagamento>();
                return availablePaymentTypes;
            }
            set { availablePaymentTypes = value; }
        }


        private void LoadAvailablePaymentTypes(List<int> allowedPaymentTypeIDs)
        {
			ObservableCollection<LK_TipoPagamento> tmpPaymentTypeList = new ObservableCollection<LK_TipoPagamento>(dalPaymentType.GetList());
			AvailablePaymentTypes = new ObservableCollection<LK_TipoPagamento>();

			if (allowedPaymentTypeIDs == null)
				AvailablePaymentTypes = tmpPaymentTypeList;
			else
			{
				foreach (LK_TipoPagamento tpg in tmpPaymentTypeList)
					if (allowedPaymentTypeIDs.Contains(tpg.Id_TipoPagamento))
						AvailablePaymentTypes.Add(tpg);
			}

			LK_TipoPagamento p = new LK_TipoPagamento();
			p.Id_TipoPagamento = 0;
			p.Descrizione = string.Empty;
			AvailablePaymentTypes.Insert(0, p);
		}


        #endregion// Payment Types



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

		private void LoadAvailableGuides()
		{
			BaseFilter filter = new BaseFilter();
			filter.SortDirection = SortDirection.ASC;
			filter.AddSortField("Cognome");
			filter.AddSortField("Nome");
			int count = 0;
			AvailableGuides = new ObservableCollection<Guida>(dalGuide.GetFilteredList(filter.Args, filter.Sort, filter.SortDirection.ToString(), filter.PageSize, filter.PageNumber, out count));
		}

		#endregion// Guides
	}
}
