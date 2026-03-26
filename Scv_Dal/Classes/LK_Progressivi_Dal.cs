using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Scv_Entities;

namespace Scv_Dal
{
	public class LK_Progressivi_Dal
	{
		public List<LK_Progressivi> GetProgressivi()
		{
			using (Scv_Entities.IN_VIAEntities _cntx = new IN_VIAEntities())
			{
				return _cntx.LK_Progressivi.ToList();
			}
		}

		public LK_Progressivi GetProgressiviBySymbol(string symbol)
		{
			using (Scv_Entities.IN_VIAEntities _cntx = new IN_VIAEntities())
			{
				return _cntx.LK_Progressivi.FirstOrDefault(x => x.Tipo == symbol);
			}
		}

        public List<LK_Progressivi> GetSingleItem(string symbol)
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                return _context.LK_Progressivi.Where(rx => rx.Tipo == symbol).ToList();
            }
        }

        public string InsertOrUpdate(LK_Progressivi obj)
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {

                if (_context.Connection.State != System.Data.ConnectionState.Open)
                    _context.Connection.Open();

                using (var transaction = _context.Connection.BeginTransaction())
                {
                    try
                    {
                        if (obj.Tipo != string.Empty)
                            _context.AttachUpdated(obj);
                        else
                            _context.LK_Progressivi.AddObject(obj);

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

            return obj.Tipo;
        }

        public void SaveObject()
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                _context.SaveChanges();
            }
        }

	}
}
