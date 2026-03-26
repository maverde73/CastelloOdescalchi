using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using Scv_Model.Common;
using Scv_Model;
using Scv_Entities;
using System.Data.Objects;

namespace Scv_Dal
{
    public class VisitaProgrammata_Dal
    {
        #region public events

        public event Scv_Model.CommonDelegates.GuideAssignmentEventHandler GuideAssignmentUpdated;

        #endregion// Public Events

        #region Constructors
        public VisitaProgrammata_Dal()
        {
        }
        #endregion// Constructors

        #region VisiteProgrammate

        public ObservableCollection<VisitaProgrammata> GetFilteredList(List<MethodArgument> listArgument, string[] orderByField, string orderByType, int pageSize, int pageNumber, out int filteredRowsCount)
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                ObservableCollection<VisitaProgrammata> ItemsList = new ObservableCollection<VisitaProgrammata>();

                ExpressionBuilder<VisitaProgrammata> eb = new ExpressionBuilder<VisitaProgrammata>();

                ObjectQuery<VisitaProgrammata> oQItemsList = null;

                if (listArgument != null)
                {
                    if (listArgument.Count > 0)
                    {
                        var expQuery = eb.WhereExpression(listArgument);
                        oQItemsList = _context.VisiteProgrammate.AsQueryable().Where(expQuery) as ObjectQuery<VisitaProgrammata>;
                    }
                    else
                    {
                        oQItemsList = _context.VisiteProgrammate.AsQueryable() as ObjectQuery<VisitaProgrammata>;
                    }
                }
                else
                    oQItemsList = _context.VisiteProgrammate.AsQueryable() as ObjectQuery<VisitaProgrammata>;

                filteredRowsCount = 0;

                if (orderByField != null)
                {
                    oQItemsList = eb.OrderedQuery(oQItemsList, orderByField, orderByType) as ObjectQuery<VisitaProgrammata>;
                    if (pageSize > 0)
                        ItemsList = new ObservableCollection<VisitaProgrammata>(oQItemsList.Skip(pageSize * pageNumber).Take(pageSize).ToList());
                    else
                        ItemsList = new ObservableCollection<VisitaProgrammata>(oQItemsList.ToList());
                }
                else
                    ItemsList = new ObservableCollection<VisitaProgrammata>(oQItemsList.ToList());

                return ItemsList;
            }
        }

        public ObservableCollection<V_VisiteProgrammate> GetVListByIdVisitaPrenotata(int idVisitaPrenotata)
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                ObservableCollection<V_VisiteProgrammate> ItemsList = new ObservableCollection<V_VisiteProgrammate>(_context.V_VisiteProgrammate.Where(vpx => vpx.Id_VisitaPrenotata == idVisitaPrenotata).ToList());
                ItemsList.ToList().ForEach(x => x.IsEmpty = false);
                ItemsList.ToList().ForEach(x => x.IsErasable = true);
                return ItemsList;
            }
        }

        public ObservableCollection<V_VisiteProgrammate> GetVListByIdPrenotazione(int idPrenotazione)
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                ObservableCollection<V_VisiteProgrammate> ItemsList = new ObservableCollection<V_VisiteProgrammate>(_context.V_VisiteProgrammate.Where(vpx => vpx.Id_Prenotazione == idPrenotazione).ToList());
                ItemsList.ToList().ForEach(x => x.IsEmpty = false);
                ItemsList.ToList().ForEach(x => x.IsErasable = true);
                return ItemsList;
            }
        }

        public V_VisiteProgrammate GetVListById(int id)
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                return _context.V_VisiteProgrammate.FirstOrDefault(vpx => vpx.Id_VisitaProgrammata == id);
            }
        }

        public List<V_VisiteProgrammate> GetVListByDate(DateTime dtVisita)
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                return _context.V_VisiteProgrammate.Where(vpx => vpx.Dt_Visita == dtVisita).ToList();
            }
        }

        public List<V_VisiteProgrammate> GetVList()
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                return _context.V_VisiteProgrammate.ToList();
            }
        }

        public VisitaProgrammata GetItem(int id)
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                return _context.VisiteProgrammate.SingleOrDefault(rx => rx.Id_VisitaProgrammata == id);
            }
        }

        public List<VisitaProgrammata> GetSingleItem(int id)
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                return _context.VisiteProgrammate.Where(rx => rx.Id_VisitaProgrammata == id).ToList();
            }
        }

        #endregion// Visite Programmate

        #region V_EvidenzeGiornaliere

        public ObservableCollection<V_EvidenzeGiornaliere> GetEvidenzeGiornaliere(DateTime dt_Visite)
        {
            //Colori alternati delle righe
            GridViewRowColor gvColors = new GridViewRowColor();

            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                var dt_Visite_Date = dt_Visite.Date;
                var list = _context.V_EvidenzeGiornaliere
                    .Where(vpx => vpx.Dt_Visita == dt_Visite_Date)
                    .OrderBy(egx => egx.Ora_Visita)
                    .ThenBy(egx => egx.Protocollo)
                    .ThenBy(egx => egx.TipoVisita)
                    .ThenBy(egx => egx.LinguaVisita).ToList();

                for (int i = 0; i < list.Count; i++)
                {
                    if (i == 0)
                        list[i].IsParentItem = true;
                    else
                        list[i].IsParentItem = (
                            list[i].Ora_Visita != list[i - 1].Ora_Visita
                            ||
                            list[i].TipoVisita != list[i - 1].TipoVisita
                            ||
                            list[i].LinguaVisita != list[i - 1].LinguaVisita
                            );

                    if (list[i].IsParentItem)
                    {
                        list[i].IsReadOnlyItem = false;
                        list[i].BgColor = gvColors.CurrentGroupColor;
                        gvColors.ColorIndex = 1 - gvColors.ColorIndex;

                        var listPartial = list
                            .Where(lx => lx.Ora_Visita == list[i].Ora_Visita
                            && lx.TipoVisita == list[i].TipoVisita
                            && lx.LinguaVisita == list[i].LinguaVisita)
                            .ToList();

                        if (String.IsNullOrEmpty(list[i].NumeroVisitatoriGruppo))
                            list[i].NumeroVisitatoriGruppo = "0";


                        if (String.IsNullOrEmpty(list[i].ConsegnatiVisita))
                            list[i].ConsegnatiVisita = "0";

                        foreach (V_EvidenzeGiornaliere eg in listPartial)
                        {
                            var bi = eg.Nr_Interi != null ? (short)eg.Nr_Interi : 0;
                            var bo = eg.Nr_Omaggio != null ? (short)eg.Nr_Omaggio : 0;
                            var br = eg.Nr_Ridotti != null ? (short)eg.Nr_Ridotti : 0;
                            var bs = eg.Nr_Scontati != null ? (short)eg.Nr_Scontati : 0;
                            var bc = eg.Nr_Cumulativi != null ? (short)eg.Nr_Cumulativi : 0;
                            list[i].NumeroVisitatoriGruppo = (Convert.ToInt32(list[i].NumeroVisitatoriGruppo) + ((short)(bi + bo + br + bs + bc))).ToString();

                            var bic = eg.Nr_InteriConsegnati != null ? (short)eg.Nr_InteriConsegnati : 0;
                            var boc = eg.Nr_OmaggioConsegnati != null ? (short)eg.Nr_OmaggioConsegnati : 0;
                            var brc = eg.Nr_RidottiConsegnati != null ? (short)eg.Nr_RidottiConsegnati : 0;
                            var brs = eg.Nr_ScontatiConsegnati != null ? (short)eg.Nr_ScontatiConsegnati : 0;
                            var brcu = eg.Nr_CumulativiConsegnati != null ? (short)eg.Nr_CumulativiConsegnati : 0;
                            list[i].ConsegnatiVisita = (Convert.ToInt32(list[i].ConsegnatiVisita) + ((short)(bic + boc + brc + brs + brcu))).ToString();
                        }
                    }
                    else
                    {
                        list[i].BgColor = gvColors.CurrentChildColor;
                        list[i].IsReadOnlyItem = true;
                    }
                }

                ObservableCollection<V_EvidenzeGiornaliere> ItemsList = new ObservableCollection<V_EvidenzeGiornaliere>(list);

                return ItemsList;
            }
        }

        public ObservableCollection<V_EvidenzeGiornaliere> GetEvidenzeGiornaliere(int idPrenotazione)
        {
            //Colori alternati delle righe
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                var list = _context.V_EvidenzeGiornaliere.Where(vpx => vpx.Id_Prenotazione == idPrenotazione).ToList();
                ObservableCollection<V_EvidenzeGiornaliere> ItemsList = new ObservableCollection<V_EvidenzeGiornaliere>(list);

                return ItemsList;
            }
        }

        public ObservableCollection<V_EvidenzeGiornaliere> GetEvidenzeGiornaliere(string protocol)
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                var list = _context.V_EvidenzeGiornaliere
                    .Where(vpx => vpx.Protocollo == protocol)
                    .OrderBy(egx => egx.Ora_Visita)
                    .ThenBy(egx => egx.Protocollo)
                    .ThenBy(egx => egx.TipoVisita)
                    .ThenBy(egx => egx.LinguaVisita)
                    .ToList();


                for (int i = 0; i < list.Count; i++)
                {
                    if (i == 0)
                        list[i].IsParentItem = true;
                    else
                        list[i].IsParentItem = (
                            list[i].Ora_Visita != list[i - 1].Ora_Visita
                            ||
                            list[i].TipoVisita != list[i - 1].TipoVisita
                            ||
                            list[i].LinguaVisita != list[i - 1].LinguaVisita)
                            ;

                    list[i].IsReadOnlyItem = !list[i].IsParentItem;


                    if (list[i].IsParentItem)
                    {
                        list[i].IsReadOnlyItem = false;

                        var listPartial = list.Where(lx =>
                            lx.Ora_Visita == list[i].Ora_Visita
                            && lx.TipoVisita == list[i].TipoVisita
                            && lx.LinguaVisita == list[i].LinguaVisita)
                            .ToList();

                        if (String.IsNullOrEmpty(list[i].NumeroVisitatoriGruppo))
                            list[i].NumeroVisitatoriGruppo = "0";

                        if (String.IsNullOrEmpty(list[i].ConsegnatiVisita))
                            list[i].ConsegnatiVisita = "0";

                        foreach (V_EvidenzeGiornaliere eg in listPartial)
                        {
                            var bi = eg.Nr_Interi != null ? (short)eg.Nr_Interi : 0;
                            var bo = eg.Nr_Omaggio != null ? (short)eg.Nr_Omaggio : 0;
                            var br = eg.Nr_Ridotti != null ? (short)eg.Nr_Ridotti : 0;
                            var bs = eg.Nr_Scontati != null ? (short)eg.Nr_Scontati : 0;
                            var bc = eg.Nr_Cumulativi != null ? (short)eg.Nr_Cumulativi : 0;
                            list[i].NumeroVisitatoriGruppo = (Convert.ToInt32(list[i].NumeroVisitatoriGruppo) + ((short)(bi + bo + br + bs + bc))).ToString();

                            var bic = eg.Nr_InteriConsegnati != null ? (short)eg.Nr_InteriConsegnati : 0;
                            var boc = eg.Nr_OmaggioConsegnati != null ? (short)eg.Nr_OmaggioConsegnati : 0;
                            var brc = eg.Nr_RidottiConsegnati != null ? (short)eg.Nr_RidottiConsegnati : 0;
                            var brs = eg.Nr_ScontatiConsegnati != null ? (short)eg.Nr_ScontatiConsegnati : 0;
                            var brcu = eg.Nr_CumulativiConsegnati != null ? (short)eg.Nr_CumulativiConsegnati : 0;
                            list[i].ConsegnatiVisita = (Convert.ToInt32(list[i].ConsegnatiVisita) + ((short)(bic + boc + brc + brs + brcu))).ToString();
                        }
                    }
                    else
                        list[i].IsReadOnlyItem = true;

                }


                ObservableCollection<V_EvidenzeGiornaliere> ItemsList = new ObservableCollection<V_EvidenzeGiornaliere>(list);

                //foreach (V_EvidenzeGiornaliere o in ItemsList)
                //{
                //    o.Fl_AccettaGuida = o.Fl_AccettaGuida != null ? o.Fl_AccettaGuida : false;
                //    o.Fl_AvvisaGuida = o.Fl_AvvisaGuida != null ? o.Fl_AvvisaGuida : false;
                //    o.Fl_Conferma = o.Fl_Conferma != null ? o.Fl_Conferma : false;
                //}

                return ItemsList;
            }
        }

        public ObservableCollection<V_EvidenzeGiornaliere> GetEvidenzeGiornaliere()
        {
            //Colori alternati delle righe
            GridViewRowColor gvColors = new GridViewRowColor();

            using (IN_VIAEntities _context = new IN_VIAEntities())
            {

                var list = _context.V_EvidenzeGiornaliere
                    .OrderByDescending(egx => egx.Dt_Visita)
                    .ThenBy(egx => egx.Ora_Visita)
                    .ThenBy(egx => egx.Protocollo)
                    .ThenBy(egx => egx.TipoVisita)
                    .ThenBy(egx => egx.LinguaVisita).ToList();

                for (int i = 0; i < list.Count; i++)
                {
                    if (i == 0)
                        list[i].IsParentItem = true;
                    else
                        list[i].IsParentItem = (
                            list[i].Dt_Visita != list[i - 1].Dt_Visita
                            ||
                            list[i].Ora_Visita != list[i - 1].Ora_Visita
                            ||
                            list[i].TipoVisita != list[i - 1].TipoVisita
                            ||
                            list[i].LinguaVisita != list[i - 1].LinguaVisita
                            );

                    if (list[i].IsParentItem)
                    {
                        list[i].IsReadOnlyItem = false;
                        list[i].BgColor = gvColors.CurrentGroupColor;
                        gvColors.ColorIndex = 1 - gvColors.ColorIndex;

                        var listPartial = list.Where(lx => lx.Dt_Visita == list[i].Dt_Visita
                                                     && lx.Ora_Visita == list[i].Ora_Visita
                                                     && lx.TipoVisita == list[i].TipoVisita
                                                     && lx.LinguaVisita == list[i].LinguaVisita).ToList();
 
                        if (String.IsNullOrEmpty(list[i].NumeroVisitatoriGruppo))
                            list[i].NumeroVisitatoriGruppo = "0";


                        if (String.IsNullOrEmpty(list[i].ConsegnatiVisita))
                            list[i].ConsegnatiVisita = "0";

                        foreach (V_EvidenzeGiornaliere eg in listPartial)
                        {
                            var bi = eg.Nr_Interi != null ? (short)eg.Nr_Interi : 0;
                            var bo = eg.Nr_Omaggio != null ? (short)eg.Nr_Omaggio : 0;
                            var br = eg.Nr_Ridotti != null ? (short)eg.Nr_Ridotti : 0;
                            var bs = eg.Nr_Scontati != null ? (short)eg.Nr_Scontati : 0;
                            var bc = eg.Nr_Cumulativi != null ? (short)eg.Nr_Cumulativi : 0;
                            list[i].NumeroVisitatoriGruppo = (Convert.ToInt32(list[i].NumeroVisitatoriGruppo) + ((short)(bi + bo + br + bs + bc))).ToString();

                            var bic = eg.Nr_InteriConsegnati != null ? (short)eg.Nr_InteriConsegnati : 0;
                            var boc = eg.Nr_OmaggioConsegnati != null ? (short)eg.Nr_OmaggioConsegnati : 0;
                            var brc = eg.Nr_RidottiConsegnati != null ? (short)eg.Nr_RidottiConsegnati : 0;
                            var brs = eg.Nr_ScontatiConsegnati != null ? (short)eg.Nr_ScontatiConsegnati : 0;
                            var brcu = eg.Nr_CumulativiConsegnati != null ? (short)eg.Nr_CumulativiConsegnati : 0;
                            list[i].ConsegnatiVisita = (Convert.ToInt32(list[i].ConsegnatiVisita) + ((short)(bic + boc + brc + brs + brcu))).ToString();
                        }
                    }
                    else
                    {
                        list[i].BgColor = gvColors.CurrentChildColor;
                        list[i].IsReadOnlyItem = true;
                    }
                }

                ObservableCollection<V_EvidenzeGiornaliere> ItemsList = new ObservableCollection<V_EvidenzeGiornaliere>(list);

                return ItemsList;
            }
        }

        public ObservableCollection<V_EvidenzeGiornaliere> GetEvidenzeGiornaliere(List<MethodArgument> listArgument, string[] orderByField, string orderByType, int pageSize, int pageNumber, out int filteredRowsCount)
        {
            GridViewRowColor gvColors = new GridViewRowColor();
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                List<V_EvidenzeGiornaliere> ItemsList = new List<V_EvidenzeGiornaliere>();

                ExpressionBuilder<V_EvidenzeGiornaliere> eb = new ExpressionBuilder<V_EvidenzeGiornaliere>();

                ObjectQuery<V_EvidenzeGiornaliere> oQItemsList = null;

                if (listArgument != null)
                {
                    if (listArgument.Count > 0)
                    {
                        var expQuery = eb.WhereExpression(listArgument);
                        oQItemsList = _context.V_EvidenzeGiornaliere.AsQueryable().Where(expQuery) as ObjectQuery<V_EvidenzeGiornaliere>;
                    }
                    else
                    {
                        oQItemsList = _context.V_EvidenzeGiornaliere.AsQueryable() as ObjectQuery<V_EvidenzeGiornaliere>;
                    }
                }
                else
                    oQItemsList = _context.V_EvidenzeGiornaliere.AsQueryable() as ObjectQuery<V_EvidenzeGiornaliere>;

                filteredRowsCount = 0;

                if (orderByField != null)
                {
                    oQItemsList = eb.OrderedQuery(oQItemsList, orderByField, orderByType) as ObjectQuery<V_EvidenzeGiornaliere>;
                    if (pageSize > 0)
                        ItemsList = oQItemsList.Skip(pageSize * pageNumber).Take(pageSize).ToList();
                    else
                        ItemsList = oQItemsList.ToList();
                }
                else
                    ItemsList = oQItemsList.ToList();

                ItemsList = ItemsList
                            .OrderByDescending(egx => egx.Dt_Visita)
                            .ThenBy(egx => egx.Ora_Visita)
                            .ThenBy(egx => egx.Protocollo)
                            .ThenBy(egx => egx.TipoVisita)
                            .ThenBy(egx => egx.LinguaVisita).ToList();

                for (int i = 0; i < ItemsList.Count; i++)
                {
                    if (i == 0)
                        ItemsList[i].IsParentItem = true;
                    else
                        ItemsList[i].IsParentItem = (
                            ItemsList[i].Dt_Visita != ItemsList[i - 1].Dt_Visita
                            ||
                            ItemsList[i].Ora_Visita != ItemsList[i - 1].Ora_Visita
                            ||
                            ItemsList[i].TipoVisita != ItemsList[i - 1].TipoVisita
                            ||
                            ItemsList[i].LinguaVisita != ItemsList[i - 1].LinguaVisita
                            );

                    if (ItemsList[i].IsParentItem)
                    {
                        ItemsList[i].IsReadOnlyItem = false;
                        ItemsList[i].BgColor = gvColors.CurrentGroupColor;
                        gvColors.ColorIndex = 1 - gvColors.ColorIndex;

                        var listPartial = ItemsList
                            .Where(lx => lx.Dt_Visita == ItemsList[i].Dt_Visita
                            && lx.Ora_Visita == ItemsList[i].Ora_Visita
                            && lx.TipoVisita == ItemsList[i].TipoVisita
                            && lx.LinguaVisita == ItemsList[i].LinguaVisita)
                            .ToList();

                        if (String.IsNullOrEmpty(ItemsList[i].NumeroVisitatoriGruppo))
                            ItemsList[i].NumeroVisitatoriGruppo = "0";


                        if (String.IsNullOrEmpty(ItemsList[i].ConsegnatiVisita))
                            ItemsList[i].ConsegnatiVisita = "0";

                        foreach (V_EvidenzeGiornaliere eg in listPartial)
                        {
                            var bi = eg.Nr_Interi != null ? (short)eg.Nr_Interi : 0;
                            var bo = eg.Nr_Omaggio != null ? (short)eg.Nr_Omaggio : 0;
                            var br = eg.Nr_Ridotti != null ? (short)eg.Nr_Ridotti : 0;
                            var bs = eg.Nr_Scontati != null ? (short)eg.Nr_Scontati : 0;
                            var bc = eg.Nr_Cumulativi != null ? (short)eg.Nr_Cumulativi : 0;
                            ItemsList[i].NumeroVisitatoriGruppo = (Convert.ToInt32(ItemsList[i].NumeroVisitatoriGruppo) + ((short)(bi + bo + br + bs + bc))).ToString();

                            var bic = eg.Nr_InteriConsegnati != null ? (short)eg.Nr_InteriConsegnati : 0;
                            var boc = eg.Nr_OmaggioConsegnati != null ? (short)eg.Nr_OmaggioConsegnati : 0;
                            var brc = eg.Nr_RidottiConsegnati != null ? (short)eg.Nr_RidottiConsegnati : 0;
                            var brs = eg.Nr_ScontatiConsegnati != null ? (short)eg.Nr_ScontatiConsegnati : 0;
                            var brcu = eg.Nr_CumulativiConsegnati != null ? (short)eg.Nr_CumulativiConsegnati : 0;
                            ItemsList[i].ConsegnatiVisita = (Convert.ToInt32(ItemsList[i].ConsegnatiVisita) + ((short)(bic + boc + brc + brs + brcu))).ToString();
                        }
                    }
                    else
                    {
                        ItemsList[i].BgColor = gvColors.CurrentChildColor;
                        ItemsList[i].IsReadOnlyItem = true;
                    }
                }

                ObservableCollection<V_EvidenzeGiornaliere> retList = new ObservableCollection<V_EvidenzeGiornaliere>(ItemsList);

                return retList;
            }
        }

        public ObservableCollection<V_EvidenzeGiornaliere> GetVisiteProgrammate()
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                var list = _context.V_EvidenzeGiornaliere
                           .OrderBy(egx => egx.Ora_Visita)
                           .ThenBy(egx => egx.Protocollo)
                           .ThenBy(egx => egx.TipoVisita)
                           .ThenBy(egx => egx.LinguaVisita)
                            .ToList();
                ObservableCollection<V_EvidenzeGiornaliere> ItemsList = new ObservableCollection<V_EvidenzeGiornaliere>(list);
                return ItemsList;
            }
        }

        public ObservableCollection<V_EvidenzeGiornaliere> GetV_EvidenzeGiornaliereGroup(List<MethodArgument> listArgument, string[] orderByField, string orderByType, int pageSize, int pageNumber, bool groupByVisit, out int filteredRowsCount)
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                ObservableCollection<V_EvidenzeGiornaliereGroup> ItemsList = new ObservableCollection<V_EvidenzeGiornaliereGroup>();

                ExpressionBuilder<V_EvidenzeGiornaliereGroup> eb = new ExpressionBuilder<V_EvidenzeGiornaliereGroup>();

                ObjectQuery<V_EvidenzeGiornaliereGroup> oQItemsList = null;

                if (listArgument != null)
                {
                    if (listArgument.Count > 0)
                    {
                        var expQuery = eb.WhereExpression(listArgument);
                        oQItemsList = _context.V_EvidenzeGiornaliereGroup.AsQueryable().Where(expQuery) as ObjectQuery<V_EvidenzeGiornaliereGroup>;
                    }
                    else
                    {
                        oQItemsList = _context.V_EvidenzeGiornaliereGroup.AsQueryable() as ObjectQuery<V_EvidenzeGiornaliereGroup>;
                    }
                }
                else
                    oQItemsList = _context.V_EvidenzeGiornaliereGroup.AsQueryable() as ObjectQuery<V_EvidenzeGiornaliereGroup>;

                filteredRowsCount = 0;

                if (orderByField != null)
                {
                    oQItemsList = eb.OrderedQuery(oQItemsList, orderByField, orderByType) as ObjectQuery<V_EvidenzeGiornaliereGroup>;
                    if (pageSize > 0)
                        ItemsList = new ObservableCollection<V_EvidenzeGiornaliereGroup>(oQItemsList.Skip(pageSize * pageNumber).Take(pageSize));
                    else
                        ItemsList = new ObservableCollection<V_EvidenzeGiornaliereGroup>(oQItemsList);
                }
                else
                    ItemsList = new ObservableCollection<V_EvidenzeGiornaliereGroup>(oQItemsList);

                GuidaDisponibile_Dal dal = new GuidaDisponibile_Dal();

                ObservableCollection<V_EvidenzeGiornaliere> evgList = new ObservableCollection<V_EvidenzeGiornaliere>();
                V_EvidenzeGiornaliere obj = null;

                List<string> groupIDs = new List<string>();

                foreach (V_EvidenzeGiornaliereGroup evgg in ItemsList
                    .OrderByDescending(x => x.Id_Guida)
                    .OrderBy(x => x.Dt_Visita)
                    .ThenBy(x => x.Ora_Visita)
                    .ThenBy(x => x.LinguaVisita)
                    .ToList()
                    )
                {
                    if (!groupIDs.Contains(evgg.Dt_Visita.ToShortDateString() + evgg.Ora_Visita + evgg.Id_Lingua.ToString() + evgg.Id_TipoVisita.ToString()))
                    {
                        groupIDs.Add(evgg.Dt_Visita.ToShortDateString() + evgg.Ora_Visita + evgg.Id_Lingua.ToString() + evgg.Id_TipoVisita.ToString());
                        obj = new V_EvidenzeGiornaliere();
                        obj.AvailableGuides = evgg.AvailableGuides;
                        obj.Cognome = evgg.Cognome;
                        obj.Dt_Visita = evgg.Dt_Visita;
                        obj.Dt_InvioAvviso = evgg.Dt_InvioAvviso;
                        obj.Fl_AccettaGuida = evgg.Fl_AccettaGuida;
                        obj.Fl_Conferma = evgg.Fl_Conferma;
                        obj.Id_Guida = evgg.Id_Guida;
                        obj.Id_Lingua = evgg.Id_Lingua;
                        obj.LinguaVisita = evgg.LinguaVisita;
                        obj.Nome = evgg.Nome;
                        obj.Ora_Visita = evgg.Ora_Visita;
                        obj.Nota = evgg.Nota;
                        obj.Id_TipoVisita = (int)evgg.Id_TipoVisita;
                        obj.TipoVisita = evgg.TipoVisita;
                        evgList.Add(obj);
                    }
                }

                return new ObservableCollection<V_EvidenzeGiornaliere>(evgList);
            }
        }

        public List<V_EvidenzeGiornaliere> GetV_EvidenzeGiornaliereVisitsSummary(ObservableCollection<V_EvidenzeGiornaliere> list)
        {
            List<string> hoursList = new List<string>(); //raggruppamento
            List<V_EvidenzeGiornaliere> evList = new List<V_EvidenzeGiornaliere>(); //raggruppamento
            V_EvidenzeGiornaliere ev = null; //raggruppamento
            foreach (V_EvidenzeGiornaliere eg in list.OrderBy(x => x.Ora_Visita).ThenBy(x => x.SiglaLingua))
            {
                if (!hoursList.Contains(eg.Ora_Visita + eg.SiglaLingua))
                {
                    hoursList.Add(eg.Ora_Visita + eg.SiglaLingua);
                    if (ev != null)
                        evList.Add(ev);
                    ev = new V_EvidenzeGiornaliere();
                    ev.Nr_Interi = 0;
                    ev.Nr_Ridotti = 0;
                    ev.Nr_Omaggio = 0;
                    ev.Nr_Scontati = 0;
                    ev.Nr_Cumulativi = 0;
                    ev.Ora_Visita = eg.Ora_Visita;
                    ev.SiglaLingua = eg.SiglaLingua;
                }
                if (ev != null)
                {
                    ev.Nr_Interi += (short)(eg.Nr_Interi != null ? eg.Nr_Interi : 0);
                    ev.Nr_Ridotti += (short)(eg.Nr_Ridotti != null ? eg.Nr_Ridotti : 0);
                    ev.Nr_Omaggio += (short)(eg.Nr_Omaggio != null ? eg.Nr_Omaggio : 0);
                    ev.Nr_Scontati += (short)(eg.Nr_Scontati != null ? eg.Nr_Scontati : 0);
                    ev.Nr_Cumulativi += (short)(eg.Nr_Cumulativi != null ? eg.Nr_Cumulativi : 0);
                }
            }

            if (ev != null)
                evList.Add(ev);

            return evList;
        }

        public List<V_EvidenzeGiornaliere> GetV_EvidenzeGiornaliereGroupByLanguageSold(ObservableCollection<V_EvidenzeGiornaliere> list)
        {
            List<string> hoursList = new List<string>();
            List<V_EvidenzeGiornaliere> evList = new List<V_EvidenzeGiornaliere>(); //raggruppamento
            V_EvidenzeGiornaliere ev = null; //raggruppamento
            foreach (V_EvidenzeGiornaliere eg in list.OrderBy(x => x.IndexLingua).ThenBy(x => x.Ora_Visita))
            {
                if (!hoursList.Contains(eg.SiglaLingua))
                {
                    hoursList.Add(eg.SiglaLingua);
                    if (ev != null)
                        evList.Add(ev);
                    ev = new V_EvidenzeGiornaliere();
                    ev.Nr_InteriConsegnati = 0;
                    ev.Nr_RidottiConsegnati = 0;
                    ev.Nr_OmaggioConsegnati = 0;
                    ev.Nr_Scontati = 0;
                    ev.Nr_Cumulativi = 0;
                    ev.Ora_Visita = eg.Ora_Visita;
                    ev.SiglaLingua = eg.SiglaLingua;
                }
                if (ev != null)
                {
                    ev.Nr_InteriConsegnati += (short)(eg.Nr_InteriConsegnati != null ? eg.Nr_InteriConsegnati : 0);
                    ev.Nr_RidottiConsegnati += (short)(eg.Nr_RidottiConsegnati != null ? eg.Nr_RidottiConsegnati : 0);
                    ev.Nr_OmaggioConsegnati += (short)(eg.Nr_OmaggioConsegnati != null ? eg.Nr_OmaggioConsegnati : 0);
                    ev.Nr_ScontatiConsegnati += (short)(eg.Nr_ScontatiConsegnati != null ? eg.Nr_ScontatiConsegnati : 0);
                    ev.Nr_CumulativiConsegnati += (short)(eg.Nr_CumulativiConsegnati != null ? eg.Nr_CumulativiConsegnati : 0);
                }
            }

            if (ev != null)
                evList.Add(ev);

            return evList;
        }

        public int GetBigliettiEmessiByIdPrenotazione(int prenotationID)
        {
            int bigliettiEmessi = 0;

            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                foreach (V_EvidenzeGiornaliere eg in _context.V_EvidenzeGiornaliere.Where(x => x.Id_Prenotazione == prenotationID).ToList())
                {
                    bigliettiEmessi += eg.Nr_InteriConsegnati != null ? (int)eg.Nr_InteriConsegnati : 0;
                    bigliettiEmessi += eg.Nr_RidottiConsegnati != null ? (int)eg.Nr_RidottiConsegnati : 0;
                    bigliettiEmessi += eg.Nr_OmaggioConsegnati != null ? (int)eg.Nr_OmaggioConsegnati : 0;
                    bigliettiEmessi += eg.Nr_ScontatiConsegnati != null ? (int)eg.Nr_ScontatiConsegnati : 0;
                    bigliettiEmessi += eg.Nr_CumulativiConsegnati != null ? (int)eg.Nr_CumulativiConsegnati : 0;
                }
            }

            return bigliettiEmessi;
        }

        public bool GetPrenotazionePagamentoParziale(int prenotationID)
        {
            bool b = false;

            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                List<V_VisiteProgrammate> vprogs = _context.V_VisiteProgrammate.Where(x => x.Id_Prenotazione == prenotationID).ToList();
                foreach (V_VisiteProgrammate vprog in vprogs)
                {
                    if (_context.Pagamenti.FirstOrDefault(x => x.Fl_Annullato == false && x.Id_VisitaProgrammata == vprog.Id_VisitaProgrammata) != null)
                        b = true;
                }
            }

            return b;
        }

        public bool CheckVisitaPrenotataPagamentoParziale(int visitID, bool checkTickets = false)
        {
            bool b = false;

            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                List<V_VisiteProgrammate> vprogs = null;
                bool tickets = false;

                if (!checkTickets)
                    vprogs = _context.V_VisiteProgrammate.Where(x => x.Id_VisitaPrenotata == visitID).ToList();
                else
                {
                    vprogs = _context.V_VisiteProgrammate.Where(x => x.Id_VisitaPrenotata == visitID && ((x.Nr_InteriConsegnati > 0) || (x.Nr_RidottiConsegnati > 0) || (x.Nr_ScontatiConsegnati > 0) || (x.Nr_CumulativiConsegnati > 0))).ToList();
                    tickets = (vprogs != null);
                }

                foreach (V_VisiteProgrammate vprog in vprogs)
                {
                    if (_context.Pagamenti.FirstOrDefault(x => x.Fl_Annullato == false && x.Id_VisitaProgrammata == vprog.Id_VisitaProgrammata) != null)
                    {
                        b = true;
                        //Nel caso in cui si debba controllare oltre alla presenza di un pagamento parziale
                        //anche l'eventuale emissione di biglietti interi o porziali la funzione restituirà true
                        //solo nel caso in cui i biglietti siano stati emessi
                        if (checkTickets)
                            b = tickets;

                        if (b)
                            //per questa visita prenotata esiste almeno una visita programmata
                            //per la quale sia stato effettuato un pagamento (con eventuale emissione di biglietti)
                            break;
                    }

                }
            }

            return b;
        }

        public bool CheckVisitaProgrammataPagamentoParziale(int visitID, bool checkTickets = false)
        {
            bool b = false;

            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                if (!checkTickets)
                {
                    if (_context.Pagamenti.FirstOrDefault(x => x.Fl_Annullato == false && x.Id_VisitaProgrammata == visitID) != null)
                        b = true;
                }
                else
                {
                    Pagamento pag = null;
                    if (_context.Pagamenti.FirstOrDefault(x => x.Fl_Annullato == false && x.Id_VisitaProgrammata == visitID) != null)
                    {
                        VisitaProgrammata vp = _context.VisiteProgrammate.FirstOrDefault(vpx => vpx.Id_VisitaProgrammata == visitID && ((vpx.Nr_InteriConsegnati > 0) || (vpx.Nr_RidottiConsegnati > 0) || (vpx.Nr_ScontatiConsegnati > 0) || (vpx.Nr_CumulativiConsegnati > 0)));
                        if (vp != null)
                            b = true;
                    }
                }
            }

            return b;
        }

        public bool CheckBigliettiEmessiByIdPrenotazione(int Id_Prenotazione)
        {
            return true;
        }

        public ConflictResult CheckConflittoVisite(List<V_VisiteProgrammate> prenotationVisits, DateTime currentDate)
        {
            ConflictResult r = new ConflictResult();

            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                List<V_EvidenzeGiornaliere> allVisits = _context.V_EvidenzeGiornaliere.Where(x => x.Dt_Visita == currentDate).OrderBy(x => x.Ora_Visita).ToList();

                if (allVisits != null)
                {
                    foreach (V_EvidenzeGiornaliere eg in allVisits)
                    {
                        foreach (V_VisiteProgrammate vp in prenotationVisits)
                        {

                            if (
                                vp.Id_VisitaProgrammata != eg.Id_VisitaProgrammata
                                &&
                                vp.Dt_Visita == eg.Dt_Visita
                                &&
                                vp.Id_TipoVisita == eg.Id_TipoVisita
                                &&
                                vp.Ora_Visita == eg.Ora_Visita
                                &&
                                vp.Id_Lingua != eg.Id_Lingua
                                )
                            {
                                r.Conflict = ConflictType.Unacceptable;
                                r.Conflicts.Add(new Conflict(vp.Id_VisitaProgrammata, ConflictType.Unacceptable));
                            }

                            if (eg.Id_TipoVisita == 2)// Speciale Scavi
                            {
                                if (
                                    vp.Id_VisitaProgrammata != eg.Id_VisitaProgrammata
                                    &&
                                    vp.Dt_Visita == eg.Dt_Visita
                                    &&
                                    vp.Ora_Visita == eg.Ora_Visita
                                    )
                                {
                                    //if(vp.Id_Lingua == eg.Id_Lingua)
                                    //{
                                    if (r.Conflict != ConflictType.Unacceptable)
                                        r.Conflict = ConflictType.Acceptable;
                                    r.Conflicts.Add(new Conflict(vp.Id_VisitaProgrammata, ConflictType.Acceptable));
                                    //}
                                    //else
                                    //{
                                    //    r.Conflict = ConflictType.Unacceptable;
                                    //    r.Conflicts.Add(new Conflict(vp.Id_VisitaProgrammata, ConflictType.Unacceptable));
                                    //}
                                }
                            }
                        }
                    }
                }

                foreach (V_VisiteProgrammate vp in prenotationVisits)
                    if (!r.Conflicts.Select(x => x.ID).Contains(vp.Id_VisitaProgrammata))
                        r.Conflicts.Add(new Conflict(vp.Id_VisitaProgrammata, ConflictType.NoConflict));
            }

            return r;
        }

        #endregion// V_EvidenzeGiornaliere

        #region Utils

        public DateTime? GetDateByIdPrenotazione(int idPrenotazione)
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                DateTime? retDate = null;
                var vProg = _context.V_VisiteProgrammate.FirstOrDefault(vpx => vpx.Id_Prenotazione == idPrenotazione);
                if (vProg != null)
                    retDate = vProg.Dt_Visita;

                return retDate;
            }
        }

        public int GetFreeTickets(int prenotationID, int? visitId = null)
        {
            int freeTickets = 0;

            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                if (visitId != null)
                {
                    VisitaProgrammata vp = _context.VisiteProgrammate.FirstOrDefault(x => x.Id_VisitaProgrammata == (int)visitId);
                    if (vp != null)
                        freeTickets = vp.Nr_Omaggio != null ? (int)vp.Nr_Omaggio : 0;
                }
                else
                {
                    foreach (V_VisiteProgrammate vp in _context.V_VisiteProgrammate.Where(x => x.Id_Prenotazione == prenotationID))
                        freeTickets += vp.Nr_Omaggio != null ? (int)vp.Nr_Omaggio : 0;
                }
            }

            return freeTickets;
        }

        public bool CheckSinglePaidVisits(int prenotationID)
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                int ids = 0;
                ids = _context.V_Pagamento.Where(x => x.Id_Prenotazione == prenotationID).Count(x => x.Id_VisitaProgrammata != null);
                return ids > 0 ? true : false;
            }
        }

        public bool CheckPaidPrenotation(int prenotationID)
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                int ids = 0;
                ids = _context.V_Pagamento.Where(x => x.Id_Prenotazione == prenotationID).Count(x => x.Id_VisitaProgrammata == null);
                return ids > 0 ? true : false;
            }
        }

        public decimal GetToPay(string protocol)
        {
            int totI = 0;
            int totO = 0;
            int totR = 0;
            int totS = 0;
            int totC = 0;

            Parametri_Dal dalParametri = new Parametri_Dal();

            //var costoIntero = Convert.ToDecimal(dalParametri.GetItem("biglietto_intero").Valore);
            //var costoRidotto = Convert.ToDecimal(dalParametri.GetItem("biglietto_ridotto").Valore);

            decimal costoIntero=0;
            decimal costoRidotto=0;
            decimal costoScontato = 0;
            decimal costoCumulativo = 0;

            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                int id_TipoVisita = (int)_context.Prenotazioni.FirstOrDefault(px => px.Protocollo == protocol).Id_TipoVisita;
                var tv = _context.LK_TipiVisita.FirstOrDefault(px => px.Id_TipoVisita == id_TipoVisita);
                costoIntero = (tv.PrezzoIntero != null) ? (decimal)tv.PrezzoIntero : 0;
                costoRidotto = (tv.PrezzoRidotto != null) ? (decimal)tv.PrezzoRidotto : 0;
                costoScontato = (tv.PrezzoScontato != null) ? (decimal)tv.PrezzoScontato : 0;
                costoCumulativo = (tv.PrezzoCumulativo != null) ? (decimal)tv.PrezzoCumulativo : 0;
                
            }

            

            foreach (V_EvidenzeGiornaliere eg in GetEvidenzeGiornaliere(protocol))
            {
                totI += eg.Nr_Interi != null ? (int)eg.Nr_Interi : 0;
                totO += eg.Nr_Omaggio != null ? (int)eg.Nr_Omaggio : 0;
                totR += eg.Nr_Ridotti != null ? (int)eg.Nr_Ridotti : 0;
                totS += eg.Nr_Scontati != null ? (int)eg.Nr_Scontati : 0;
                totC += eg.Nr_Cumulativi != null ? (int)eg.Nr_Cumulativi : 0;
            }

            return (costoIntero * totI) + (costoRidotto * totR) + (costoScontato * totS) + (costoCumulativo * totC) + costoCumulativo;
        }

        public decimal GetDelta(string protocol, decimal paid)
        {
            return GetToPay(protocol) - paid;
        }

        private int GetTotVisitors_V_EvidenzeGiornaliere(int visitID)
        {
            int t = 0;
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                foreach (V_EvidenzeGiornaliere eg in _context.V_EvidenzeGiornaliere.Where(x => x.Id_VisitaProgrammata == visitID))
                {
                    if (eg.Nr_Interi != null)
                        t += (int)eg.Nr_Interi;
                    if (eg.Nr_Ridotti != null)
                        t += (int)eg.Nr_Ridotti;
                    if (eg.Nr_Omaggio != null)
                        t += (int)eg.Nr_Omaggio;
                    if (eg.Nr_Scontati != null)
                        t += (int)eg.Nr_Scontati;
                    if (eg.Nr_Cumulativi != null)
                        t += (int)eg.Nr_Cumulativi;
                }
            }

            return t;
        }

        private int GetTotVisitors_V_EvidenzeGiornaliere(DateTime visitDate, string visitHour, int visitLanguageID)
        {
            int t = 0;
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                foreach (V_EvidenzeGiornaliere eg in _context.V_EvidenzeGiornaliere.Where(x => x.Dt_Visita == visitDate && x.Ora_Visita == visitHour && x.Id_Lingua == visitLanguageID))
                {
                    if (eg.Nr_Interi != null)
                        t += (int)eg.Nr_Interi;
                    if (eg.Nr_Ridotti != null)
                        t += (int)eg.Nr_Ridotti;
                    if (eg.Nr_Omaggio != null)
                        t += (int)eg.Nr_Omaggio;
                    if (eg.Nr_Scontati != null)
                        t += (int)eg.Nr_Scontati;
                    if (eg.Nr_Cumulativi != null)
                        t += (int)eg.Nr_Cumulativi;
                }
            }

            return t;
        }

        #endregion// Utils

        #region Save/Update/Delete

        public PrenotationStatus InsertOrUpdate(DateTime dt_Visita, Prenotazione prenotation, ObservableCollection<V_VisitePrenotate> prenotationVisits, ObservableCollection<V_VisiteProgrammate> scheduledTours, ObservableCollection<V_EvidenzeGiornaliere> evidenzeGiornaliere, out bool saveOK)
        {
            saveOK = true;
            PrenotationStatus prenotationStatus = new PrenotationStatus();

            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                if (_context.Connection.State != System.Data.ConnectionState.Open)
                    _context.Connection.Open();

                using (var transaction = _context.Connection.BeginTransaction())
                {
                    try
                    {
                        var currentUser = ApplicationState.GetValue<LK_User>("currentUser");
                        DateTime dtNow = DateTime.Now;

                        //Prenotazione
                        if (prenotation != null)
                        {
                            _context.AttachUpdated(prenotation);
                            _context.SaveChanges();
                        }

                        //foreach (V_EvidenzeGiornaliere evx in evidenzeGiornaliere)
                        //{
                        //    scheduledTours.Where(vpx => vpx.Id_Prenotazione == evx.Id_Prenotazione
                        //                                && vpx.Id_VisitaPrenotata == evx.Id_VisitaPrenotata
                        //                                && vpx.Dt_Visita == evx.Dt_Visita
                        //                                && vpx.Id_Lingua == evx.Id_Lingua
                        //                                && vpx.Ora_Visita == evx.Ora_Visita).ToList().ForEach(vpx => vpx.Id_Guida = evx.Id_Guida);
                        //}

                        //Visite prenotate
                        int tmpID = 0;
                        List<int> prenotationVisitsToDeleteIDs = new List<int>();

                        //Si ricavano gli ID delle visite prenotate cosi come sono nel DB. 
                        //La lista servirà da confronto con quella passata come argomento,
                        //per ottenerne una lista di visite prenotate da eliminare, nel
                        //caso l'utente abbia eliminato delle visite prenotate nella lavorazione.
                        List<int> prenotationVisitsFromDbIDs = _context.VisitePrenotate.Where(x => x.Id_Prenotazione == prenotation.Id_Prenotazione).Select(x => x.Id_VisitaPrenotata).ToList();

                        //Si popola la lista degli id delle visite prenotate da eliminare
                        foreach (int i in prenotationVisitsFromDbIDs)
                        {
                            if (!prenotationVisits.Select(vpx => vpx.Id_VisitaPrenotata).ToList().Contains(i))
                                prenotationVisitsToDeleteIDs.Add(i);
                        }

                        short prenotationVisitors = 0;

                        foreach (V_VisitePrenotate vPrens in prenotationVisits)
                        {
                            //Fra le visite prenotate alcune saranno già presenti in DB, altre saranno state
                            //create dalla lavorazione. Dato che ogni visita programmata deve essere associata
                            //alla stessa visita prenotata alla quale era associata nella lavorazione,
                            //e siccome gli ID delle visite prenotate create ora in lavorazione sono
                            //solo provvisori, per poter associare i nuovi ID ricavati dall'INSERT in tabella
                            //alle visite programmate, mantenendo intatta l'associazione fra visite programmate
                            //e visite prenotate, si memorizza l'ID di ogni visita prenotata.
                            //Nelle visite già presenti in archivio tmpID rimarrà lo stesso anche dopo l'INSERT
                            //mentre in quelle createn in lavorazione cambierà.
                            tmpID = vPrens.Id_VisitaPrenotata;

                            //Se la visita prenotata NON esiste in archivio significa che è fra quelle appena creata
                            //in lavorazione, perciò se ne crea una nuova.
                            VisitaPrenotata vPren = _context.VisitePrenotate.FirstOrDefault(x => x.Id_VisitaPrenotata == vPrens.Id_VisitaPrenotata);
                            if (vPren == null)
                                vPren = new VisitaPrenotata();

                            //Sia che la visita esista già in archivio, sia che venga creata in lavorazione
                            //è necessario reinizializzarla perchè in entrambi i casi deve aderire alle eventuali
                            //modifiche apportate in lavorazione.
                            vPren.Dt_Risposta = vPrens.Dt_Risposta;
                            vPren.Id_Lingua = vPrens.Id_Lingua;
                            vPren.Id_Prenotazione = prenotation.Id_Prenotazione;
                            vPren.Id_TipoVisita = (int)prenotation.Id_TipoVisita;
                            vPren.Id_User = vPrens.Id_User;

                            //Nelle visite prenotate la lingua è univoca, ovvero non possono esserci due visite prenotate
                            //con la stessa lingua. Tuttavia è possibile aggiungere illimitate visite con la stessa
                            //lingua tra le visite programmate. Durante il salvataggio si ricava il numero di visitatori
                            //complessivi per una data lingua e se ne aggiorna il totale nella visita prenotata
                            //relativa a tale lingua.
                            //QUI (AGGIUNTO && x.Id_VisitaPrenotata == vPren.Id_VisitaPrenotata)
                            short Nr_Visitatori = (short)scheduledTours
                                .Where(x => x.Id_Lingua == vPren.Id_Lingua && x.Id_VisitaPrenotata == vPren.Id_VisitaPrenotata)
                                .Sum(x =>
                                        (
                                            (x.Nr_Interi != null ? (short)x.Nr_Interi : 0)
                                            +
                                            (x.Nr_Ridotti != null ? (short)x.Nr_Ridotti : 0)
                                            +
                                            (x.Nr_Omaggio != null ? (short)x.Nr_Omaggio : 0)
                                            +
                                            (x.Nr_Scontati != null ? (short)x.Nr_Scontati : 0)
                                            +
                                            (x.Nr_Cumulativi != null ? (short)x.Nr_Cumulativi : 0)
                                        )
                                    );
                            if (Nr_Visitatori > vPren.Nr_Visitatori)
                                vPren.Nr_Visitatori = Nr_Visitatori;
                            else
                                vPren.Nr_Visitatori = (short)prenotationVisits.FirstOrDefault(x => x.Id_VisitaPrenotata == tmpID).Nr_Visitatori;


                            //Si aggiorna il numero totale di visitatori della prenotazione.
                            prenotationVisitors += vPren.Nr_Visitatori;

                            vPren.Dt_Update = DateTime.Now.Date;

                            //Si aggiorna/crea la visita prenotata.
                            if (vPren.Id_VisitaPrenotata > 0)
                                _context.AttachUpdated(vPren);
                            else
                                _context.VisitePrenotate.AddObject(vPren);

                            _context.SaveChanges();

                            //Si impostano tutte le visite programmate relative alla visita prenotata corrente
                            //e si associano a quest'ultima.
                            scheduledTours.Where(x => x.Id_VisitaPrenotata == tmpID).ToList().ForEach(x => x.Id_VisitaPrenotata = vPren.Id_VisitaPrenotata);

                        }

                        //Si eliminano eventuali visite prenotate eliminate dall'utente in lavorazione
                        foreach (int i in prenotationVisitsToDeleteIDs)
                        {
                            VisitaPrenotata vPrenToDelete = _context.VisitePrenotate.FirstOrDefault(x => x.Id_VisitaPrenotata == i);
                            if (vPrenToDelete != null)
                            {
                                _context.VisitePrenotate.Attach(vPrenToDelete);
                                _context.VisitePrenotate.DeleteObject(vPrenToDelete);
                                _context.SaveChanges();
                            }
                        }

                        //Visite Programmate
                        List<int> idsVisiteProgrammate = new List<int>();
                        List<int> idsVisitePrenotate = new List<int>();
                        //List<int> idsVisiteProgrammateNoGuida = new List<int>();
                        idsVisitePrenotate = _context.VisitePrenotate.Where(vpx => vpx.Id_Prenotazione == prenotation.Id_Prenotazione).Select(vpx => vpx.Id_VisitaPrenotata).ToList();

                        int totaleVisitatoriVisiteProg = 0;
                        int totaleVisitatoriProgrammati = 0;
                        for (int y = 0; y < idsVisitePrenotate.Count; y++)
                        {
                            int idCurrentVPren = idsVisitePrenotate[y];
                            totaleVisitatoriVisiteProg = 0;
                            idsVisiteProgrammate.Clear();

                            var scheduledToursPerPrenotation = scheduledTours.Where(vpx => vpx.Id_VisitaPrenotata == idCurrentVPren).ToList();

                            foreach (V_VisiteProgrammate v in scheduledToursPerPrenotation)
                            {
                                VisitaProgrammata vprog = null;

                                if (!v.IsNew)
                                    vprog = _context.VisiteProgrammate.FirstOrDefault(vpx => vpx.Id_VisitaProgrammata == v.Id_VisitaProgrammata);
                                else
                                {
                                    vprog = new VisitaProgrammata();
                                    vprog.Dt_Update = DateTime.Now;
                                }

                                string oldOraVisita = vprog.Ora_Visita;

                                vprog.Id_VisitaPrenotata = idCurrentVPren;
                                vprog.Ora_Visita = v.Ora_Visita;
                                vprog.Dt_Visita = dt_Visita.Date;
                                vprog.Nr_Interi = v.Nr_Interi;
                                vprog.Nr_Ridotti = v.Nr_Ridotti;
                                vprog.Nr_Omaggio = v.Nr_Omaggio;
                                vprog.Nr_Scontati = v.Nr_Scontati;
                                vprog.Nr_Cumulativi = v.Nr_Cumulativi;
                                vprog.Id_Guida = v.Id_Guida == 0 ? null : v.Id_Guida;
                                vprog.Fl_AccettaGuida = v.Fl_AccettaGuida;
                                vprog.Dt_Update = DateTime.Now;

                                var bi = vprog.Nr_Interi != null ? (short)vprog.Nr_Interi : 0;
                                var bo = vprog.Nr_Omaggio != null ? (short)vprog.Nr_Omaggio : 0;
                                var br = vprog.Nr_Ridotti != null ? (short)vprog.Nr_Ridotti : 0;
                                var bs = vprog.Nr_Scontati != null ? (short)vprog.Nr_Scontati : 0;
                                var bc = vprog.Nr_Cumulativi != null ? (short)vprog.Nr_Cumulativi : 0;

                                totaleVisitatoriVisiteProg += (bi + bo + br + bs + bc);

                                vprog.Id_User = currentUser.Id_User;
                                vprog.Dt_Update = dtNow;

                                if (v.IsNew)
                                    _context.VisiteProgrammate.AddObject(vprog);
                                else
                                    _context.AttachUpdated(vprog);

                                _context.SaveChanges();

                                idsVisiteProgrammate.Add(vprog.Id_VisitaProgrammata);
                            }

                            List<VisitaProgrammata> visiteProgrammateToDelete = new List<VisitaProgrammata>();


                            if (idsVisiteProgrammate.Count > 0)
                                visiteProgrammateToDelete = _context.VisiteProgrammate.Where(x => x.Id_VisitaPrenotata == idCurrentVPren && !idsVisiteProgrammate.Contains(x.Id_VisitaProgrammata)).ToList();
                            else
                                visiteProgrammateToDelete = _context.VisiteProgrammate.Where(x => x.Id_VisitaPrenotata == idCurrentVPren).ToList();

                            foreach (VisitaProgrammata vp in visiteProgrammateToDelete)
                            {
                                _context.VisiteProgrammate.Attach(vp);
                                _context.VisiteProgrammate.DeleteObject(vp);
                                _context.SaveChanges();
                            }

                            totaleVisitatoriProgrammati += totaleVisitatoriVisiteProg;
                        }

                        var egGroupHeaders = evidenzeGiornaliere.Where(egx => egx.IsParentItem == true).ToList();

                        foreach (V_EvidenzeGiornaliere eg in egGroupHeaders)
                        {
                            var currentAndChildrens = _context.V_EvidenzeGiornaliere
                                                        .Where(egx =>
                                                               egx.Dt_Visita == eg.Dt_Visita
                                                               &&
                                                               egx.Ora_Visita == eg.Ora_Visita
                                                               &&
                                                               egx.Id_Lingua == eg.Id_Lingua
                                                               &&
                                                               egx.Id_TipoVisita == eg.Id_TipoVisita);




                            foreach (V_EvidenzeGiornaliere item in currentAndChildrens)
                            {
                                VisitaProgrammata vprogramm = _context.VisiteProgrammate.FirstOrDefault(vpx => vpx.Id_VisitaProgrammata == item.Id_VisitaProgrammata);

                                if (vprogramm != null)
                                {
                                    if (eg.Id_Guida == 0) eg.Id_Guida = null;
                                    vprogramm.Id_Guida = eg.Id_Guida;
                                    vprogramm.Fl_AccettaGuida = eg.Fl_AccettaGuida;
                                    vprogramm.Fl_AvvisaGuida = eg.Fl_AvvisaGuida;
                                    vprogramm.Dt_InvioAvviso = eg.Dt_InvioAvviso;
                                    _context.SaveChanges();
                                }
                            }
                        }

                        //Impostazione dati impostati a monte prima dell'assegnazione della prenotazione
                        bool confermata = prenotation.Fl_Conferma != null ? prenotation.Fl_Conferma == true : false;

                        prenotation = _context.Prenotazioni.FirstOrDefault(px => px.Id_Prenotazione == prenotation.Id_Prenotazione);

                        //SI assegna il nuovo totale visitatori alla prenotazione

                        if (_context.VisitePrenotate.Where(vpx => vpx.Id_Prenotazione == prenotation.Id_Prenotazione).Count() > 0)
                        {
                            prenotation.Tot_Visitatori = prenotationVisitors;
                        }
                        else
                        {
                            VisitaPrenotata v = new VisitaPrenotata();
                            v.Id_Prenotazione = prenotation.Id_Prenotazione;
                            v.Id_Lingua = prenotation.Id_LinguaRisposta;
                            v.Nr_Visitatori = prenotation.Tot_Visitatori;
                            _context.VisitePrenotate.AddObject(v);
                        }


                        //Si assegna lo stato di conferma della prenotazione
                        prenotation.Fl_Conferma = confermata;

                        if (totaleVisitatoriProgrammati > 0)
                        {
                            if (prenotation.Id_TipoRisposta == 9)//da lavorare
                                prenotation.Id_TipoRisposta = 2;//Visita programmata
                        }
                        else
                            if (
                                prenotation.Id_TipoRisposta == 2//Visita programmata
                                ||
                                prenotation.Id_TipoRisposta == 4//Visita da programmare
                            )
                                prenotation.Id_TipoRisposta = 9;//da lavorare

                        _context.SaveChanges();

                        if (prenotation.Id_TipoRisposta == 2 || prenotation.Id_TipoRisposta == 3 || prenotation.Id_TipoRisposta == 10)
                        {
                            if (_context.VisitePrenotate.Where(vpx => vpx.Id_Prenotazione == prenotation.Id_Prenotazione).Count() == 0)
                            {
                                transaction.Rollback();
                                saveOK = false;
                                return prenotationStatus;
                            }
                        }


                        transaction.Commit();

                        prenotationStatus.ResponsTypeID = prenotation.Id_TipoRisposta != null ? (int)prenotation.Id_TipoRisposta : 0;
                        prenotationStatus.ConfirmationChecked = prenotation.Fl_Conferma != null ? (bool)prenotation.Fl_Conferma : false; ;
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

            return prenotationStatus;
        }

        public void UpdateVisitsGuides(List<V_EvidenzeGiornaliere> visits)
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                if (_context.Connection.State != System.Data.ConnectionState.Open)
                    _context.Connection.Open();

                using (var transaction = _context.Connection.BeginTransaction())
                {
                    try
                    {
                        decimal tot = visits.Count;
                        decimal progress = 0;
                        decimal percentMax = 100;
                        List<int> visitsIDs = new List<int>();

                        foreach (V_EvidenzeGiornaliere visitGroup in visits)
                        {
                            //Estrae parent e children del gruppo
                            List<int> allVisitsIDs = _context.V_EvidenzeGiornaliere
                                .Where(x =>
                                    x.Dt_Visita == visitGroup.Dt_Visita
                                    &&
                                    x.Ora_Visita == visitGroup.Ora_Visita
                                    &&
                                    x.Id_Lingua == visitGroup.Id_Lingua
                                    &&
                                    x.Id_TipoVisita == visitGroup.Id_TipoVisita
                                    )
                                    .OrderBy(x => x.Dt_Visita).ThenBy(x => x.Ora_Visita).ThenBy(x => x.Id_Lingua)
                                    .Select(x => x.Id_VisitaProgrammata)
                                    .ToList();

                            List<VisitaProgrammata> vprogs = _context.VisiteProgrammate
                                .Where(x => allVisitsIDs.Contains(x.Id_VisitaProgrammata))
                                .OrderBy(x => x.Dt_Visita).ThenBy(x => x.Ora_Visita)
                                .ToList();

                            foreach (VisitaProgrammata v in vprogs)
                            {
                                if (!visitsIDs.Contains(v.Id_VisitaProgrammata))
                                {
                                    visitsIDs.Add(v.Id_VisitaProgrammata);
                                    if (visitGroup.Id_Guida > 0)
                                    {
                                        v.Id_Guida = visitGroup.Id_Guida;
                                        v.Fl_AccettaGuida = visitGroup.Fl_AccettaGuida;
                                        v.Fl_AvvisaGuida = visitGroup.Fl_AvvisaGuida;
                                        v.Nota = visitGroup.Nota;
                                        v.Dt_InvioAvviso = visitGroup.Dt_InvioAvviso;
                                    }
                                    else
                                    {
                                        v.Id_Guida = (int?)null;
                                        v.Fl_AccettaGuida = false;
                                        v.Fl_AvvisaGuida = false;
                                        v.Nota = visitGroup.Nota;
                                    }
                                    _context.AttachUpdated(v);
                                    _context.SaveChanges();
                                }
                            }

                            OnGuideUpdating((++progress / tot) * percentMax);
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

        public void UpdateVisitNoticeDate(int visitID)
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                if (_context.Connection.State != System.Data.ConnectionState.Open)
                    _context.Connection.Open();

                using (var transaction = _context.Connection.BeginTransaction())
                {
                    try
                    {
                        foreach (V_EvidenzeGiornaliere visitGroup in _context.V_EvidenzeGiornaliere.Where(x => x.Id_VisitaProgrammata == visitID))
                        {
                            //Estrae parent e children del gruppo
                            List<V_EvidenzeGiornaliere> allVisits = _context.V_EvidenzeGiornaliere.Where(x => x.Dt_Visita == visitGroup.Dt_Visita && x.Ora_Visita == visitGroup.Ora_Visita && x.Id_Lingua == visitGroup.Id_Lingua).ToList();

                            foreach (V_EvidenzeGiornaliere v in allVisits)
                            {

                                //Estrae la VisitaProgrammata
                                VisitaProgrammata vp = _context.VisiteProgrammate.FirstOrDefault(x => x.Id_VisitaProgrammata == v.Id_VisitaProgrammata);

                                //Se viene trovata, imposta i parametri secondo la EvidenzaGiornaliera
                                if (vp != null)
                                {
                                    vp.Dt_InvioAvviso = DateTime.Now;
                                    _context.AttachUpdated(vp);
                                    _context.SaveChanges();
                                }
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

        public void SaveObject()
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                _context.SaveChanges();
            }
        }

        public bool SavePrintedTickets(int scheduledVisitID, int deliverableFullPriceTickets, int deliverableReducedTickets, int deliverableFreeTickets, int deliverableDiscountTickets, int deliverableCumulativeTickets)
        {
            bool success = true;
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                if (_context.Connection.State != System.Data.ConnectionState.Open)
                    _context.Connection.Open();

                using (var transaction = _context.Connection.BeginTransaction())
                {
                    try
                    {
                        VisitaProgrammata v = _context.VisiteProgrammate.FirstOrDefault(x => x.Id_VisitaProgrammata == scheduledVisitID);
                        if (v != null)
                        {
                            if (v.Nr_InteriConsegnati == null)
                                v.Nr_InteriConsegnati = 0;

                            if (v.Nr_RidottiConsegnati == null)
                                v.Nr_RidottiConsegnati = 0;

                            if (v.Nr_OmaggioConsegnati == null)
                                v.Nr_OmaggioConsegnati = 0;

                            if (v.Nr_ScontatiConsegnati == null)
                                v.Nr_ScontatiConsegnati = 0;

                            if (v.Nr_CumulativiConsegnati == null)
                                v.Nr_CumulativiConsegnati = 0;

                            v.Nr_InteriConsegnati += (short)deliverableFullPriceTickets;
                            v.Nr_RidottiConsegnati += (short)deliverableReducedTickets;
                            v.Nr_OmaggioConsegnati += (short)deliverableFreeTickets;
                            v.Nr_ScontatiConsegnati += (short)deliverableDiscountTickets;
                            v.Nr_CumulativiConsegnati += (short)deliverableCumulativeTickets;

                            _context.AttachUpdated(v);
                            _context.SaveChanges();

                            LK_Progressivi pr = _context.LK_Progressivi.FirstOrDefault(rx => rx.Tipo == "TK");

                            int num = deliverableFullPriceTickets + deliverableReducedTickets + deliverableFreeTickets;

                            if (pr != null)
                            {
                                if (pr.Anno != v.Dt_Visita.Year)
                                {
                                    pr.Anno = pr.Anno > 0 ? v.Dt_Visita.Year : 0;
                                    pr.Progr_UltimoUscito = pr.Anno > 0 ? num : pr.Progr_UltimoUscito + num;
                                }
                                else
                                    pr.Progr_UltimoUscito += num;

                                if (pr.Tipo != string.Empty)
                                    _context.AttachUpdated(pr);
                                else
                                    _context.LK_Progressivi.AddObject(pr);

                                _context.SaveChanges();


                                //Registrazione ultimo numero
                                DateTime dt = DateTime.Now.Date;
                                UltimoNumeroBiglietto u = null;
                                u = _context.UltimoNumeroBigliettoes.FirstOrDefault(x => x.Dt_Numero == dt);
                                if (u == null)
                                {
                                    u = new UltimoNumeroBiglietto();
                                    u.Id_UltimoNumeroBiglietto = 0;
                                    u.Dt_Numero = dt;
                                    u.Nr = pr.Progr_UltimoUscito;
                                    _context.UltimoNumeroBigliettoes.AddObject(u);
                                }
                                else
                                {
                                    u.Nr = pr.Progr_UltimoUscito;
                                    _context.AttachUpdated(u);
                                }

                                _context.SaveChanges();
                            }

                            transaction.Commit();
                        }
                    }
                    catch (Exception e)
                    {
                        transaction.Rollback();
                        success = false;
                        throw e;
                    }
                    finally
                    {
                        _context.Connection.Close();
                    }
                }
            }
            return success;
        }

        #endregion// Save/Update/Delete

        #region Event Handlers

        private void OnGuideUpdating(decimal updatingPercent)
        {
            if (updatingPercent > 100)
                updatingPercent = 100;

            if (GuideAssignmentUpdated != null)
                GuideAssignmentUpdated(this, new GuideAssignmentEventArgs((int)updatingPercent, updatingPercent >= 100));
        }

        #endregion// Event Handlers
    }
}
