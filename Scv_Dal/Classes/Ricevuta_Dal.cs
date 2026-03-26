//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Scv_Entities;

//namespace Scv_Dal
//{
//    public class Ricevuta_Dal
//    {
//        public List<Ricevuta> GetRicevuteByIdPagamento(int idPagamento)
//        {
//            using (IN_VIAEntities _context = new IN_VIAEntities())
//            {
//                return _context.Ricevutas.Where(x => x.Id_Pagamento == idPagamento).ToList();
//            }
//        }

//        public Ricevuta GetRicevuta(int idRicevuta)
//        {
//            using (IN_VIAEntities _context = new IN_VIAEntities())
//            {
//                return _context.Ricevutas.FirstOrDefault(x => x.Id_Ricevuta == idRicevuta);
//            }
//        }

//        public int InsertOrUpdate(Ricevuta obj)
//        {
//            int id = 0;
//            LK_Progressivi_Dal dalPR = new LK_Progressivi_Dal();
//            LK_Progressivi pr = null;

//            using (IN_VIAEntities _context = new IN_VIAEntities())
//            {

//                if (_context.Connection.State != System.Data.ConnectionState.Open)
//                    _context.Connection.Open();

//                using (var transaction = _context.Connection.BeginTransaction())
//                {
//                    try
//                    {

//                        //Creazione ricevuta
//                        if (obj.Id_Ricevuta != 0)
//                        {
//                            _context.AttachUpdated(obj);
//                        }
//                        else
//                        {
//                            _context.Ricevutas.AddObject(obj);
//                        }

//                        _context.SaveChanges();

//                        id = obj.Id_Ricevuta;

//                        transaction.Commit();
//                    }
//                    catch (Exception e)
//                    {
//                        transaction.Rollback();
//                        throw e;
//                    }
//                    finally
//                    {
//                        _context.Connection.Close();
//                    }
//                }
//            }

//            return id;
//        }


//    }
//}
