using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Scv_Entities;
using Scv_Model.Common;
using System.Data.Objects;
using Scv_Model;

namespace Scv_Dal
{
    public class Stats_Dal
    {
        public List<V_TipoVisita_TipoBiglietto_Grouped> GetList(List<int> anni,List<int> mesi,List<int> idTipiVisita)
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                List<V_TipoVisita_TipoBiglietto_Grouped> stats = new List<V_TipoVisita_TipoBiglietto_Grouped>();

                stats = _context.V_TipoVisita_TipoBiglietto_Grouped.ToList();

                if(anni.Count > 0)
                    stats = stats.Where(stx => anni.Contains(stx.anno)).ToList();

                if (mesi.Count > 0)
                    stats = stats.Where(stx => mesi.Contains(stx.mese)).ToList();

                if (idTipiVisita.Count > 0)
                    stats = stats.Where(stx => idTipiVisita.Contains(stx.Id_TipoVisita)).ToList();

                int mese = 0;
                string meseDescrizione = "";
                for (int i = 0; i < stats.Count; i++)
                {
                    mese = stats[i].mese;
                    switch (mese)
                    {
                        case 1:
                            meseDescrizione = "gennaio";
                            break;

                        case 2:
                            meseDescrizione = "febbraio";
                            break;

                        case 3:
                            meseDescrizione = "marzo";
                            break;

                        case 4:
                            meseDescrizione = "aprile";
                            break;

                        case 5:
                            meseDescrizione = "maggio";
                            break;

                        case 6:
                            meseDescrizione = "giugno";
                            break;

                        case 7:
                            meseDescrizione = "luglio";
                            break;

                        case 8:
                            meseDescrizione = "agosto";
                            break;

                        case 9:
                            meseDescrizione = "settembre";
                            break;

                        case 10:
                            meseDescrizione = "ottobre";
                            break;

                        case 11:
                            meseDescrizione = "novembre";
                            break;

                        case 12:
                            meseDescrizione = "dicembre";
                            break;
                    }

                    stats[i].meseDescrizione = meseDescrizione;
                }

                return stats;
            }
        }
    }
}
