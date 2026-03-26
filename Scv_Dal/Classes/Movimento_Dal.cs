using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Scv_Entities;
using System.Data.Objects;
using Scv_Model.Common;
using Scv_Model;
namespace Scv_Dal
{
	public class Movimento_Dal
	{
		public List<Movimento> GetFilteredList(List<MethodArgument> listArgument, string[] orderByField, string orderByType, int pageSize, int pageNumber, out int filteredRowsCount)
		{
			//_context = new SCV_DEVEntities();

			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				List<Movimento> ItemsList = new List<Movimento>();

				ExpressionBuilder<Movimento> eb = new ExpressionBuilder<Movimento>();

				ObjectQuery<Movimento> oQItemsList = null;

				if (listArgument != null)
				{
					if (listArgument.Count > 0)
					{
						var expQuery = eb.WhereExpression(listArgument);
						oQItemsList = _context.Movimenti.AsQueryable().Where(expQuery) as ObjectQuery<Movimento>;
					}
					else
					{
						oQItemsList = _context.Movimenti.AsQueryable() as ObjectQuery<Movimento>;
					}
				}
				else
					oQItemsList = _context.Movimenti.AsQueryable() as ObjectQuery<Movimento>;

				filteredRowsCount = 0;

				if (orderByField != null)
				{
					oQItemsList = eb.OrderedQuery(oQItemsList, orderByField, orderByType) as ObjectQuery<Movimento>;
					if (pageSize > 0)
						ItemsList = oQItemsList.Skip(pageSize * pageNumber).Take(pageSize).ToList();
					else
						ItemsList = oQItemsList.ToList();
				}
				else
					ItemsList = oQItemsList.ToList();

				ItemsList.ToList().ForEach(x => x.IsEmpty = false);
				ItemsList.ToList().ForEach(x => x.IsErasable = true);

				return ItemsList;
			}
		}

		public List<V_Movimento> GetFilteredList_V_Movimento(List<MethodArgument> listArgument, string[] orderByField, string orderByType, int pageSize, int pageNumber, out int filteredRowsCount)
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				List<V_Movimento> ItemsList = new List<V_Movimento>();

				ExpressionBuilder<V_Movimento> eb = new ExpressionBuilder<V_Movimento>();

				ObjectQuery<V_Movimento> oQItemsList = null;

				if (listArgument != null)
				{
					if (listArgument.Count > 0)
					{
						var expQuery = eb.WhereExpression(listArgument);
						oQItemsList = _context.V_Movimento.AsQueryable().Where(expQuery) as ObjectQuery<V_Movimento>;
					}
					else
					{
						oQItemsList = _context.V_Movimento.AsQueryable() as ObjectQuery<V_Movimento>;
					}
				}
				else
					oQItemsList = _context.V_Movimento.AsQueryable() as ObjectQuery<V_Movimento>;

				filteredRowsCount = 0;

				if (orderByField != null)
				{
					oQItemsList = eb.OrderedQuery(oQItemsList, orderByField, orderByType) as ObjectQuery<V_Movimento>;
					if (pageSize > 0)
						ItemsList = oQItemsList.Skip(pageSize * pageNumber).Take(pageSize).ToList();
					else
						ItemsList = oQItemsList.ToList();
				}
				else
					ItemsList = oQItemsList.ToList();

				return ItemsList;
			}
		}

		public List<MovementMaster> GetFilteredList_MovimentoMasterDetail(List<MethodArgument> listArgument, string[] orderByField, string orderByType, int pageSize, int pageNumber, out int filteredRowsCount)
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				List<V_MovimentoMaster> ItemsList = new List<V_MovimentoMaster>();

				ExpressionBuilder<V_MovimentoMaster> eb = new ExpressionBuilder<V_MovimentoMaster>();

				ObjectQuery<V_MovimentoMaster> oQItemsList = null;

				if (listArgument != null)
				{
					if (listArgument.Count > 0)
					{
						var expQuery = eb.WhereExpression(listArgument);
						oQItemsList = _context.V_MovimentoMaster.AsQueryable().Where(expQuery) as ObjectQuery<V_MovimentoMaster>;
					}
					else
					{
						oQItemsList = _context.V_MovimentoMaster.AsQueryable() as ObjectQuery<V_MovimentoMaster>;
					}
				}
				else
					oQItemsList = _context.V_MovimentoMaster.AsQueryable() as ObjectQuery<V_MovimentoMaster>;

				filteredRowsCount = 0;

				if (orderByField != null)
				{
					oQItemsList = eb.OrderedQuery(oQItemsList, orderByField, orderByType) as ObjectQuery<V_MovimentoMaster>;
					if (pageSize > 0)
						ItemsList = oQItemsList.Skip(pageSize * pageNumber).Take(pageSize).ToList();
					else
						ItemsList = oQItemsList.ToList();
				}
				else
					ItemsList = oQItemsList.ToList();

				List<MovementMaster> masterList = new List<MovementMaster>();
				List<MovementDetail> detailList = null;
				MovementMaster mm = null;
				MovementDetail md = null;

				foreach (V_MovimentoMaster m in ItemsList.OrderBy(x => x.Identificativo).ToList())
				{
					mm = new MovementMaster();
					mm.ID = m.Identificativo != null ? (int)m.Identificativo : 0;
					mm.MovementTypeID = m.Id_TipoMovimento;
					mm.MovementType = m.TipoMovimento;
					mm.Date = m.Dt_Movimento;
					mm.Document = m.RicevutaBolla;
					mm.BillID = m.Id_Fattura;
					mm.Note = m.Nota;
					mm.PaymentType = m.TipoPagamento;
					mm.StoreID = m.Id_EsercizioVendita != null ? (int)m.Id_EsercizioVendita : 0;
					mm.Store = m.EsercizioVendita;
					mm.StoreDiscount = m.Sconto != null ? (decimal)m.Sconto : 0;
					mm.MovementNote = m.NotaMovimento;
					mm.PosNumber = m.Nr_Pos;
					detailList = new List<MovementDetail>();
					foreach (V_Movimento vm in _context.V_Movimento.Where(x => x.Identificativo == mm.ID).ToList())
					{
						md = new MovementDetail();
						md.MovementID = mm.ID;
						md.MovementTypeID = mm.MovementTypeID;
						md.ISBN = vm.ISBN;
						md.Article = vm.Articolo;
						md.ArticleType = vm.TipoArticolo;
						md.PublicPrice = vm.PrezzoPubblico != null ? (decimal)vm.PrezzoPubblico : 0;
						md.Price = vm.PrezzoVendita != null ? (decimal)vm.PrezzoVendita : 0;
						md.Quantity = vm.Nr_Pezzi != null ? (int)vm.Nr_Pezzi : 0;
						detailList.Add(md);
					}
					mm.Details = detailList;
					masterList.Add(mm);
				}

				return masterList;
			}
		}

		//recupera la lista di interi relativi al numero dei movimenti master secondo i criteri di filtraggio
		//serve per il pager della grid
		public List<int> GetMaxMovimentoMaster(List<MethodArgument> listArgument, string[] orderByField, string orderByType, int pageSize, int pageNumber, out int filteredRowsCount)
		{
			List<int> list = new List<int>();

			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				List<V_MovimentoMaster> ItemsList = new List<V_MovimentoMaster>();

				ExpressionBuilder<V_MovimentoMaster> eb = new ExpressionBuilder<V_MovimentoMaster>();

				ObjectQuery<V_MovimentoMaster> oQItemsList = null;

				if (listArgument != null)
				{
					if (listArgument.Count > 0)
					{
						var expQuery = eb.WhereExpression(listArgument);
						oQItemsList = _context.V_MovimentoMaster.AsQueryable().Where(expQuery) as ObjectQuery<V_MovimentoMaster>;
					}
					else
					{
						oQItemsList = _context.V_MovimentoMaster.AsQueryable() as ObjectQuery<V_MovimentoMaster>;
					}
				}
				else
					oQItemsList = _context.V_MovimentoMaster.AsQueryable() as ObjectQuery<V_MovimentoMaster>;

				filteredRowsCount = 0;

				if (orderByField != null)
				{
					oQItemsList = eb.OrderedQuery(oQItemsList, orderByField, orderByType) as ObjectQuery<V_MovimentoMaster>;
					ItemsList = oQItemsList.ToList();
				}
				else
					ItemsList = oQItemsList.ToList();

				for (int i = ItemsList.Count; i < pageSize; i++)
					ItemsList.Add(new V_MovimentoMaster());

				list = Enumerable.Range(0, ItemsList.Count).ToList();
				return list;
			}
		}

		public List<V_Mandato> GetFilteredList_V_Mandato(List<MethodArgument> listArgument, string[] orderByField, string orderByType, int pageSize, int pageNumber, out int filteredRowsCount)
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				List<V_Mandato> ItemsList = new List<V_Mandato>();

				ExpressionBuilder<V_Mandato> eb = new ExpressionBuilder<V_Mandato>();

				ObjectQuery<V_Mandato> oQItemsList = null;

				if (listArgument != null)
				{
					if (listArgument.Count > 0)
					{
						var expQuery = eb.WhereExpression(listArgument);
						oQItemsList = _context.V_Mandato.AsQueryable().Where(expQuery) as ObjectQuery<V_Mandato>;
					}
					else
					{
						oQItemsList = _context.V_Mandato.AsQueryable() as ObjectQuery<V_Mandato>;
					}
				}
				else
					oQItemsList = _context.V_Mandato.AsQueryable() as ObjectQuery<V_Mandato>;

				filteredRowsCount = 0;

				if (orderByField != null)
				{
					oQItemsList = eb.OrderedQuery(oQItemsList, orderByField, orderByType) as ObjectQuery<V_Mandato>;
					if (pageSize > 0)
						ItemsList = oQItemsList.Skip(pageSize * pageNumber).Take(pageSize).ToList();
					else
						ItemsList = oQItemsList.ToList();
				}
				else
					ItemsList = oQItemsList.ToList();

				for (int i = ItemsList.Count; i < pageSize; i++)
					ItemsList.Add(new V_Mandato());

				return ItemsList;
			}
		}

		public List<int> GetMaxFilteredList_V_Mandato(List<MethodArgument> listArgument, string[] orderByField, string orderByType, int pageSize, int pageNumber, out int filteredRowsCount)
		{
			List<int> list = new List<int>();

			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				List<V_Mandato> ItemsList = new List<V_Mandato>();

				ExpressionBuilder<V_Mandato> eb = new ExpressionBuilder<V_Mandato>();

				ObjectQuery<V_Mandato> oQItemsList = null;

				if (listArgument != null)
				{
					if (listArgument.Count > 0)
					{
						var expQuery = eb.WhereExpression(listArgument);
						oQItemsList = _context.V_Mandato.AsQueryable().Where(expQuery) as ObjectQuery<V_Mandato>;
					}
					else
					{
						oQItemsList = _context.V_Mandato.AsQueryable() as ObjectQuery<V_Mandato>;
					}
				}
				else
					oQItemsList = _context.V_Mandato.AsQueryable() as ObjectQuery<V_Mandato>;

				filteredRowsCount = 0;

				if (orderByField != null)
				{
					oQItemsList = eb.OrderedQuery(oQItemsList, orderByField, orderByType) as ObjectQuery<V_Mandato>;
					if (pageSize > 0)
						ItemsList = oQItemsList.Skip(pageSize * pageNumber).Take(pageSize).ToList();
					else
						ItemsList = oQItemsList.ToList();
				}
				else
					ItemsList = oQItemsList.ToList();

				for (int i = ItemsList.Count; i < pageSize; i++)
					ItemsList.Add(new V_Mandato());

				list = Enumerable.Range(0, ItemsList.Count).ToList();

				return list;
			}
		}

		public Movimento GetItem(int id)
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				Movimento p = _context.Movimenti.SingleOrDefault(rx => rx.Id_Movimento == id);
				return p;
			}
		}

		public List<Movimento> GetMovementByIdentifier(int identifier)
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				return _context.Movimenti.Where(rx => rx.Identificativo == identifier).ToList();
			}
		}

		public List<Movimento> GetSingleItem(int id)
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				return _context.Movimenti.Where(rx => rx.Id_Movimento == id).ToList();
			}
		}

		public int InsertOrUpdate(List<Movimento> obj, LK_TipoMovimento tm, string rAnno, string rNumero)
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{

				if (_context.Connection.State != System.Data.ConnectionState.Open)
					_context.Connection.Open();

				using (var transaction = _context.Connection.BeginTransaction())
				{
					try
					{
						//Creazione identificativo che raggruppa i dettagli della transazione
						int identifier = 0;

						//L'identificativo è l'ID del primo dettaglio registrato                        
						_context.Movimenti.AddObject(obj[0]);
						_context.SaveChanges();
						identifier = obj[0].Id_Movimento;

						//Si registrano tutti i dettagli
						//tranne il primo che viene aggiornato.
						foreach (Movimento m in obj)
						{
							m.Identificativo = identifier;
							m.Dt_Update = DateTime.Now.Date;

							if (m.Id_Movimento != 0)
								_context.AttachUpdated(m);
							else
								_context.Movimenti.AddObject(m);
						}

						_context.SaveChanges();

						//Aggiornamento progressivi

						//Creazione DAL Progressivi
						LK_Progressivi_Dal dalPR = new LK_Progressivi_Dal();

						//Si ricava il progressivo relativo al tipo movimento
						if (tm.Simbolo != null && tm.Simbolo != string.Empty)
						{
							LK_Progressivi pr = _context.LK_Progressivi.FirstOrDefault(x => x.Tipo == tm.Simbolo);
							//LK_Progressivi pr = dalPR.GetSingleItem(tm.Simbolo)[0];

							//Se viene trovato un progressivo per il tipo di movimento
							//esso viene aggiornato con i nuovi valori e registrato.
							if (pr != null)
							{
								int anno = 0;
								int numero = 0;
								int.TryParse(rAnno, out anno);
								int.TryParse(rNumero, out numero);

								pr.Anno = anno;
								pr.Progr_UltimoUscito = numero;

								if (pr.Tipo != string.Empty)
									_context.AttachUpdated(pr);
								else
									_context.LK_Progressivi.AddObject(pr);

								_context.SaveChanges();
							}
						}

						transaction.Commit();
					}
					catch (Exception e)
					{
						transaction.Rollback();
						throw e;
					}
					finally
					{
						_context.Connection.Close();
					}
				}
			}

			return obj[0].Id_Movimento;
		}

		public int InsertOrUpdateRefund(Movimento obj, int paymentID)
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{

				if (_context.Connection.State != System.Data.ConnectionState.Open)
					_context.Connection.Open();

				using (var transaction = _context.Connection.BeginTransaction())
				{
					try
					{
						//Creazione identificativo che raggruppa i dettagli della transazione
						int identifier = 0;

						obj.Dt_Update = DateTime.Now.Date;

						//L'identificativo è l'ID del primo dettaglio registrato                        
						_context.Movimenti.AddObject(obj);
						_context.SaveChanges();

						//Progressivo/Ricevuta
						LK_Progressivi pr = _context.LK_Progressivi.FirstOrDefault(x => x.Tipo == "RM");
						if (pr != null)
						{
							if (pr.Anno != obj.Dt_Movimento.Year)
							{
								pr.Anno = pr.Anno > 0 ? obj.Dt_Movimento.Year : 0;
								pr.Progr_UltimoUscito = pr.Anno > 0 ? 1 : pr.Progr_UltimoUscito + 1;
							}
							else
								pr.Progr_UltimoUscito++;

							if (pr.Tipo != string.Empty)
								_context.AttachUpdated(pr);
							else
								_context.LK_Progressivi.AddObject(pr);
						}

						identifier = obj.Id_Movimento;

						//Assegnazione identificativo al movimento
						Movimento m = _context.Movimenti.FirstOrDefault(x => x.Id_Movimento == identifier);
						m.Identificativo = identifier;
						_context.AttachUpdated(m);
						_context.SaveChanges();

						//Verifica del residuo del pagamento ed eventuale eliminazione in caso di residuo zero.
						//Pagamento p = _context.Pagamenti.SingleOrDefault(x => x.Id_Pagamento == paymentID);
						//if (p != null)
						//{
						//    p.Importo -= obj.PrezzoVendita != null ? (decimal)obj.PrezzoVendita : 0;
						//    p.Importo = p.Importo >= 0 ? p.Importo : 0;
						//    if (p.Importo == 0)
						//    {
						//        //Eliminare il pagamento
						//        _context.DeleteObject(p);
						//    }
						//    else
						//    {
						//        //salvare le modifiche
						//        _context.AttachUpdated(p);
						//    }

						//    _context.SaveChanges();
						//}

						_context.SaveChanges();

						transaction.Commit();
					}
					catch (Exception e)
					{
						transaction.Rollback();
						throw e;
					}
					finally
					{
						_context.Connection.Close();
					}
				}
			}

			return obj.Id_Movimento;
		}

		public void ChangeMovementPaymentType(int movementID, int paymentTypeID, string posNumber)
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{

				if (_context.Connection.State != System.Data.ConnectionState.Open)
					_context.Connection.Open();

				using (var transaction = _context.Connection.BeginTransaction())
				{
					try
					{
						List<Movimento> list = _context.Movimenti.Where(x => x.Identificativo == movementID).ToList();

						if (list != null)
						{
							foreach (Movimento obj in list)
							{
								obj.Id_TipoPagamento = paymentTypeID;
								obj.Nr_Pos = posNumber;
							}
						}

						_context.SaveChanges();

						transaction.Commit();
					}
					catch (Exception e)
					{
						transaction.Rollback();
						throw e;
					}
					finally
					{
						_context.Connection.Close();
					}
				}
			}
		}

		public void SaveObject()
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				_context.SaveChanges();
			}
		}

		public void DeleteObject(List<Movimento> obj, LK_TipoMovimento tm)
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{

				if (_context.Connection.State != System.Data.ConnectionState.Open)
					_context.Connection.Open();

				using (var transaction = _context.Connection.BeginTransaction())
				{
					try
					{

						foreach (Movimento m in obj)
						{
							_context.Movimenti.DeleteObject(_context.Movimenti.FirstOrDefault(x => x.Id_Movimento == m.Id_Movimento));
						}
						_context.SaveChanges();

						//Aggiornamento progressivi

						//Creazione DAL Progressivi
						LK_Progressivi_Dal dalPR = new LK_Progressivi_Dal();

						if (tm.Simbolo != null && tm.Simbolo != string.Empty)
						{
							//Si ricava il progressivo relativo al tipo movimento
							LK_Progressivi pr = dalPR.GetSingleItem(tm.Simbolo)[0];

							//Se viene trovato un progressivo per il tipo di movimento
							//esso viene aggiornato con i nuovi valori e registrato.
							if (pr != null)
							{
								pr.Progr_UltimoUscito -= 1;

								if (pr.Tipo != string.Empty)
									_context.AttachUpdated(pr);
								else
									_context.LK_Progressivi.AddObject(pr);

								_context.SaveChanges();
							}
						}

						transaction.Commit();
					}
					catch (Exception e)
					{
						transaction.Rollback();
						throw e;
					}
					finally
					{
						_context.Connection.Close();
					}
				}
			}
		}

		public List<MovementMaster> GetBills(BaseFilter filter, int yearNumber, int monthNumber, int selectedStatusID)
		{
			//Viene eseguita una chiamata a GetFilteredList_MovimentoMasterDetail(), che ottiene una lista di master con relativi details
			//Tale chiamata viene fatta con argomenti di paginazione impostati a zero, in quanto la paginazione viene
			//eseguita qui DOPO l'ottenimento della lista, in quanto essa va ulteriormente filtrata per anno, mese,
			//stato ed eventuale presenza di fattura, filtri che vengono impostati a seconda di selectedStatusID.
			List<MovementMaster> list = new List<MovementMaster>();
			int count = 0;
			if (filter.GetFilter("Id_EsercizioVendita") == null)
				filter.SetFilter("Id_EsercizioVendita", Utilities.ValueType.Int, 0, Utilities.SQLOperator.GreaterThan);

			List<MovementMaster> tmpList = new List<MovementMaster>();
			tmpList.AddRange(GetFilteredList_MovimentoMasterDetail(filter.Args, filter.Sort, filter.SortDirection.ToString(), 0, 0, out count));

			switch (selectedStatusID)
			{
				case 1:
					list.AddRange(
						tmpList
						.Where(x =>
						x.ID > 0
						&&
						x.Date.Value.Year == yearNumber
						&&
						x.Date.Value.Month <= monthNumber
						&&
						x.BillID == null
						)
						.Skip(filter.PageSize * filter.PageNumber)
						.Take(filter.PageSize > 0 ? filter.PageSize : tmpList.Count)
						.OrderBy(x => x.StoreID)
						.ThenBy(x => x.ID)
						.ToList()
						);
					break;
				case 2:
					list.AddRange(
						tmpList
						.Where(x =>
						x.ID > 0
						&&
						x.Date.Value.Year == yearNumber
						&&
						x.Date.Value.Month <= monthNumber
						&&
						x.BillID != null
						)
						.Skip(filter.PageSize * filter.PageNumber)
						.Take(filter.PageSize > 0 ? filter.PageSize : tmpList.Count)
						.OrderBy(x => x.StoreID)
						.ThenBy(x => x.ID)
						.ToList()
						);
					break;

				case 3:
					list.AddRange(
						tmpList
						.Where(x =>
						x.ID > 0
						&&
						x.Date.Value.Year == yearNumber
						&&
						x.Date.Value.Month <= monthNumber
						)
						.Skip(filter.PageSize * filter.PageNumber)
						.Take(filter.PageSize > 0 ? filter.PageSize : tmpList.Count)
						.OrderBy(x => x.StoreID)
						.ThenBy(x => x.ID)
						.ToList()
						);
					break;
			}

			for (int i = list.Count; i < filter.PageSize; i++)
				list.Add(new MovementMaster());

			return list;
		}

		public List<int> GetMaxBills(BaseFilter filter, int yearNumber, int monthNumber, int selectedStatusID)
		{
			List<MovementMaster> list = new List<MovementMaster>();
			List<int> listCount = new List<int>();
			int count = 0;
			if (filter.GetFilter("Id_EsercizioVendita") == null)
				filter.SetFilter("Id_EsercizioVendita", Utilities.ValueType.Int, 0, Utilities.SQLOperator.GreaterThan);
			switch (selectedStatusID)
			{
				case 1:
					list.AddRange(
						GetFilteredList_MovimentoMasterDetail(filter.Args, filter.Sort, filter.SortDirection.ToString(), 0, 0, out count)
						.Where(x =>
						x.ID > 0
						&&
						x.Date.Value.Year == yearNumber
						&&
						x.Date.Value.Month <= monthNumber
						&&
						x.StoreID > 0
						&&
						x.BillID == null
						)
						.OrderBy(x => x.StoreID)
						.ThenBy(x => x.ID)
						.ToList()
						);
					break;
				case 2:
					list.AddRange(
						GetFilteredList_MovimentoMasterDetail(filter.Args, filter.Sort, filter.SortDirection.ToString(), 0, 0, out count)
						.Where(x =>
						x.ID > 0
						&&
						x.Date.Value.Year == yearNumber
						&&
						x.Date.Value.Month <= monthNumber
						&&
						x.StoreID > 0
						&&
						x.BillID != null
						)
						.OrderBy(x => x.StoreID)
						.ThenBy(x => x.ID)
						.ToList()
						);
					break;

				case 3:
					list.AddRange(
						GetFilteredList_MovimentoMasterDetail(filter.Args, filter.Sort, filter.SortDirection.ToString(), 0, 0, out count)
						.Where(x =>
						x.ID > 0
						&&
						x.Date.Value.Year == yearNumber
						&&
						x.Date.Value.Month <= monthNumber
						&&
						x.StoreID > 0
						)
						.OrderBy(x => x.StoreID)
						.ThenBy(x => x.ID)
						.ToList()
						);
					break;
			}

			for (int i = list.Count; i < filter.PageSize; i++)
				list.Add(new MovementMaster());

			listCount = Enumerable.Range(0, list.Count).ToList();
			return listCount;
		}

		public List<Invoice> GetInvoice(int invoiceID)
		{
			List<Invoice> list = new List<Invoice>();
			List<V_Fatture> tmpList = new List<V_Fatture>();
			Invoice obj = null;
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				if (invoiceID > 0)
					tmpList = _context.V_Fatture.Where(x => x.Id_Fattura == invoiceID).OrderBy(x => x.Articolo).ToList();
				else
					tmpList = _context.V_Fatture.ToList();

				foreach (V_Fatture vf in tmpList)
				{
					obj = new Invoice();
					obj.Id_TipoMovimento = 7;//Invio altri esercizi commerciali (bolla)
					obj.Articolo = vf.Articolo;
					obj.Id_EsercizioVendita = vf.Id_EsercizioVendita;
					obj.EsercizioVendita = vf.EsercizioVendita;
					obj.Id_Fattura = vf.Id_Fattura;
					obj.Nr_Pezzi = vf.Quantita != null ? (short)vf.Quantita : (short)0;
					obj.PrezzoPubblico = vf.PrezzoPubblico;
					obj.PrezzoVendita = vf.PrezzoVendita;
					obj.InvoiceNumber = vf.Nr_Fattura;
					obj.Sconto = vf.Sconto;
					obj.InvoiceDate = vf.Dt_Fattura;
					obj.StoreName = vf.EsercizioVendita.ToUpper();
					obj.StoreAddress1 = vf.Indirizzo;
					obj.StoreAddress2 = (vf.CAP != null ? vf.CAP + " " : string.Empty) + (vf.Citta != null ? vf.Citta : string.Empty);

					list.Add(obj);
				}
			}
			return list;
		}

		public List<Invoice> GetInvoiceList(BaseFilter filter, int yearNumber, int monthNumber)
		{
			List<Invoice> list = new List<Invoice>();
			Invoice obj = null;
			List<Fattura> invoiceList = new List<Fattura>();
			List<Fattura> tmpList = new List<Fattura>();
			EsercizioVendita s = null;
			EsercizioVendita_Dal dalStore = new EsercizioVendita_Dal();
			Fattura_Dal dalInvoice = new Fattura_Dal();

			int count = 0;
			filter.SetFilter("Id_Fattura", Utilities.ValueType.Int, 0, Utilities.SQLOperator.GreaterThan);
			tmpList = dalInvoice.GetFilteredList(filter.Args, filter.Sort, filter.SortDirection.ToString(), 0, 0, out count);
			invoiceList = tmpList
				.Where(x =>
					x.Dt_Fattura.Year == yearNumber
					&&
					x.Dt_Fattura.Month <= monthNumber
					)
				.Skip(filter.PageSize * filter.PageNumber)
				.Take(filter.PageSize != 0 ? filter.PageSize : tmpList.Count)
				.OrderBy(x => x.Id_EsercizioVendita)
				.ThenBy(x => x.Dt_Fattura)
				.ToList();

			foreach (Fattura f in invoiceList)
			{
				s = dalStore.GetItem((int)f.Id_EsercizioVendita);

				obj = new Invoice();
				obj.Id_Fattura = f.Id_Fattura;
				obj.Id_EsercizioVendita = f.Id_EsercizioVendita;
				obj.EsercizioVendita = s.Descrizione.ToUpper();
				obj.InvoiceNumber = f.Nr_Fattura;
				obj.InvoiceDate = f.Dt_Fattura;
				obj.PaymentDate = f.Dt_Pagamento;

				using (IN_VIAEntities _context = new IN_VIAEntities())
				{
					obj.Details = _context.V_InvoiceDetails
						.Where(x => x.Id_Fattura == f.Id_Fattura)
						.ToList();
					obj.InvoiceTotal = obj.Details.Sum(x => x.Totale != null ? (decimal)x.Totale : 0);
				}

				list.Add(obj);
			}

			for (int i = list.Count; i < filter.PageSize; i++)
				list.Add(new Invoice());

			return list;
		}

		public List<int> GetMaxInvoices(BaseFilter filter, int yearNumber, int monthNumber)
		{
			List<Fattura> invoiceList = new List<Fattura>();
			Fattura_Dal dalInvoice = new Fattura_Dal();

			int count = 0;
			filter.SetFilter("Id_Fattura", Utilities.ValueType.Int, 0, Utilities.SQLOperator.GreaterThan);
			invoiceList = dalInvoice.GetFilteredList(filter.Args, filter.Sort, filter.SortDirection.ToString(), 0, 0, out count)
				.Where(x =>
					x.Dt_Fattura.Year == yearNumber
					&&
					x.Dt_Fattura.Month <= monthNumber
					)
				.OrderBy(x => x.Id_EsercizioVendita)
				.ThenBy(x => x.Dt_Fattura)
				.ToList();

			for (int i = invoiceList.Count; i < filter.PageSize; i++)
				invoiceList.Add(new Fattura());

			return Enumerable.Range(0, invoiceList.Count).ToList();
		}

		public List<V_Articoli> GetInventario()
		{
			List<V_Articoli> list = new List<V_Articoli>();

			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				int count = 0;
				//list = _context.V_Articoli.OrderBy(x => x.Descrizione).ToList();
				list = new Articolo_Dal().GetFilteredList_V_Articoli(null, null, string.Empty, 0, 0, out count).OrderBy(x => x.Descrizione).ToList();

				foreach (V_Articoli art in list)
				{
					List<Movimento> movementList = new List<Movimento>();
					movementList.AddRange(GetFilteredList(null, null, string.Empty, 0, 0, out count));

					//foreach (Movimento m in _context.Movimenti.OrderBy(x => x.Dt_Movimento).Where(x => x.Id_Articolo == art.Id_Articolo))
					foreach (Movimento m in movementList.OrderBy(x => x.Dt_Movimento).Where(x => x.Id_Articolo == art.Id_Articolo))
					{
						//LK_TipoMovimento t = _context.LK_TipiMovimento.FirstOrDefault(x => x.Id_TipoMovimento == m.Id_TipoMovimento);
						LK_TipoMovimento t = new LK_TipoMovimento_Dal().GetItem(m.Id_TipoMovimento);
						if (t != null)
						{
							switch (t.Segno_Maga)
							{
								case "+":
									art.CopieMagazzino += m.Nr_Pezzi != null ? (int)m.Nr_Pezzi : 0;
									break;
								case "-":
									art.CopieMagazzino -= m.Nr_Pezzi != null ? (int)m.Nr_Pezzi : 0;
									break;
							}
							switch (t.Segno_Uff)
							{
								case "+":
									art.CopieUfficio += m.Nr_Pezzi != null ? (int)m.Nr_Pezzi : 0;
									break;
								case "-":
									art.CopieUfficio -= m.Nr_Pezzi != null ? (int)m.Nr_Pezzi : 0;
									break;
							}

						}
					}

					art.TotaleCopie = art.CopieUfficio + art.CopieMagazzino;
					List<V_Movimento> tmp = _context.V_Movimento.Where(x => x.Id_TipoMovimento == 1 && x.Id_Articolo == art.Id_Articolo).ToList();
					if(tmp != null && tmp.Count > 0)
						art.Dt_UltimaEntrata = tmp.Max(x => x.Dt_Movimento);
				}
			}

			return list;
		}

		public List<V_SintesiArticoli> GetSintesiArticoli(int year)
		{
			List<V_SintesiArticoli> list = new List<V_SintesiArticoli>();

			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				DateTime startTime = new DateTime(year, 1, 1);
				DateTime endTime = new DateTime(year, 12, 31);

				list = _context.V_SintesiArticoli.Where(x => x.Dt_Movimento >= startTime && x.Dt_Movimento <= endTime).ToList();
			}

			return list;
		}
	}
}
