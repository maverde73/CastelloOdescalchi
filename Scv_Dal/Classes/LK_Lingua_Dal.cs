using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Scv_Entities;
using System.Data.Objects;
using Scv_Model.Common;

namespace Scv_Dal
{
    public class LK_Lingua_Dal
	{
        public List<LK_Lingua> GetItems()
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                List<LK_Lingua> ItemsList = new List<LK_Lingua>();

                ExpressionBuilder<LK_Lingua> eb = new ExpressionBuilder<LK_Lingua>();

                ObjectQuery<LK_Lingua> oQItemsList = null;


                oQItemsList = _context.LK_Lingue.AsQueryable() as ObjectQuery<LK_Lingua>;

                ItemsList = oQItemsList.ToList();

                return ItemsList;
            }
        }

		public List<LK_Lingua> GetSingleItem(int id)
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				return _context.LK_Lingue.Where(rx => rx.Id_Lingua == id).ToList();
			}
		}

		public LK_Lingua GetItem(int id)
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				return _context.LK_Lingue.FirstOrDefault(rx => rx.Id_Lingua == id);
			}
		}

		public LK_Lingua GetDefault()
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				return _context.LK_Lingue.FirstOrDefault(rx => rx.Fl_Default == true);
			}
		}


        public List<LK_Lingua> GetList()
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                return _context.LK_Lingue.ToList();
            }
        }
	}
}
