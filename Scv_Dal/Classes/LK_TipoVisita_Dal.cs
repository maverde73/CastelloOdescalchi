using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Scv_Entities;
using Scv_Model.Common;
using System.Data.Objects;

namespace Scv_Dal
{
    public class LK_TipoVisita_Dal
    {
        public List<LK_TipoVisita> GetItems()
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                List<LK_TipoVisita> ItemsList = new List<LK_TipoVisita>();

                ExpressionBuilder<LK_TipoVisita> eb = new ExpressionBuilder<LK_TipoVisita>();

                ObjectQuery<LK_TipoVisita> oQItemsList = null;

                oQItemsList = _context.LK_TipiVisita.AsQueryable() as ObjectQuery<LK_TipoVisita>;

                ItemsList = oQItemsList.OrderBy(tvx => tvx.Ordine).ToList();

                return ItemsList;
            }
        }

        public List<LK_TipoVisita> GetSingleItem(int id)
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                return _context.LK_TipiVisita.Where(rx => rx.Id_TipoVisita == id).ToList();
            }
        }

        public LK_TipoVisita GetItem(int id)
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                return _context.LK_TipiVisita.FirstOrDefault(rx => rx.Id_TipoVisita == id);
            }
        }

        public LK_TipoVisita GetDefault()
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                return _context.LK_TipiVisita.FirstOrDefault(rx => rx.Fl_Default == true);
            }
        }

        public int InsertOrUpdate(LK_TipoVisita obj)
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {

                try
                {
                    if (obj.Id_TipoVisita != 0)
                        _context.AttachUpdated(obj);
                    else
                        _context.LK_TipiVisita.AddObject(obj);

                    _context.SaveChanges();

                }
                catch (Exception e)
                {
                    throw e;
                }
                finally
                {
                    _context.Connection.Close();
                }
            }


            return obj.Id_TipoVisita;
        }

        public void DeleteItem(int id)
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                LK_TipoVisita tv = _context.LK_TipiVisita.FirstOrDefault(tvx => tvx.Id_TipoVisita == id);
                _context.LK_TipiVisita.DeleteObject(tv);
                _context.SaveChanges();
            }
        }

        public bool CheckIfCanDelete(int id)
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                return (_context.Prenotazioni.FirstOrDefault(prx => prx.Id_TipoVisita == id) == null);
            }
        }


    }
}
