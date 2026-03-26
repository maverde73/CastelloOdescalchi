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
    public class Biglietto_Dal
    {
        public int InsertOrUpdate(Biglietto biglietto)
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                if (biglietto.Id_Biglietto > 0)
                    _context.AttachUpdated(biglietto);
                else
                    _context.Biglietti.AddObject(biglietto);

                _context.SaveChanges();

                return biglietto.Id_Biglietto;
            }
        }

        public List<LK_TipoBiglietto> Get_LK_TipoBiglietto_List()
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                return _context.LK_TipoBiglietto.OrderBy(tpx => tpx.Id_TipoBiglietto).ToList();
            }
        }

        public List<Biglietto> Get_Biglietto_List(int giorniValidita)
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                DateTime dataValidita = DateTime.Now.AddDays(giorniValidita * -1);
                return _context.Biglietti.Where(bx => bx.DataOraEmissione.Date >= dataValidita.Date).ToList();
            }
        }

        public Biglietto Get_Biglietto_ByCode(long code)
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                return _context.Biglietti.FirstOrDefault(bx => bx.Codice == code);
            }
        }


        //public bool Vidima(long code)
        //{
        //    using (IN_VIAEntities _context = new IN_VIAEntities())
        //    {
        //        var biglietto = _context.Biglietti.FirstOrDefault(bx => bx.Codice == code);
        //        if (biglietto != null)
        //        {
        //            biglietto.Vidimato = true;
        //            biglietto.DataOraVidimazione = DateTime.Now;
        //            _context.SaveChanges();
        //            return true;
        //        }
        //        else
        //            return false;
        //    }
        //}


        public Biglietto Vidima(long code,bool vidima = true)
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                var biglietto = _context.Biglietti.FirstOrDefault(bx => bx.Codice == code && bx.Vidimato == false);
                if (biglietto != null)
                {
                    if (vidima)
                    {
                        biglietto.Vidimato = true;
                        biglietto.DataOraVidimazione = DateTime.Now;
                        biglietto.Passed = biglietto.Passed + 1;
                        _context.SaveChanges();
                    }
                    return biglietto;
                }
                else
                    return null;
            }
        }

        public List<Biglietto> Get_List_ByIdVisitaProgrammata(int IdVisitaProgrammata)
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
               return _context.Biglietti.Where(bx => bx.Id_VisitaProgrammata == IdVisitaProgrammata).ToList();
            }
        }

    }
}
