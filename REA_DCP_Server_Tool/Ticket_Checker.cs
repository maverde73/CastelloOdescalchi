using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Scv_Dal;
using Scv_Entities;
using System.Configuration;


namespace REA_DCP_Server_Tool
{
    public class Ticket_Checker
    {
        Biglietto_Dal biglietto_Dal = new Biglietto_Dal();
        List<LK_TipoBiglietto> tipiBiglietto = null;
        int giorniValidita = 3;
        List<Biglietto> biglietti = null;
        int versoEntrata = 0;



        public Common.TicketState CheckTicket(string barcode, bool vidima = true)
        {
            Common.TicketState ret = new Common.TicketState() { State = Common.TicketStateEnum.Invalido };
            try
            {
                tipiBiglietto = biglietto_Dal.Get_LK_TipoBiglietto_List();

                if (ConfigurationManager.AppSettings["giornivalidita"] != null)
                    giorniValidita = Convert.ToInt32(ConfigurationManager.AppSettings["giornivalidita"]);

                ulong code = Convert.ToUInt64(barcode);

                uint pax = 0;
                DateTime date = new DateTime();
                uint cont = 0;
                uint tipo = 0;

                Barcode.DecodeBarCode(ref pax, ref date, ref cont, ref tipo, code);
                ret.State = Common.TicketStateEnum.Valido;

                if (date.Date <= DateTime.Now.AddDays(giorniValidita * -1).Date)
                {
                    ret.State = Common.TicketStateEnum.Scaduto;
                    return ret;
                }

                ret.Pax = Convert.ToInt32(pax);
                ret.Date = date;

                var biglietto = biglietto_Dal.Get_Biglietto_ByCode(Convert.ToInt64(code));

                if (biglietto != null)
                {
                    if (biglietto.Id_TipoBiglietto != 4)
                    {
                        if (biglietto.Vidimato)
                        {
                            ret.Valid = false;
                            ret.State = Common.TicketStateEnum.Validato;
                        }
                        else if (biglietto.Annullato)
                        {
                            ret.Valid = false;
                            ret.State = Common.TicketStateEnum.Annullato;
                        }
                        else
                        {
                            var bigliettoVidimato = biglietto_Dal.Vidima(Convert.ToInt64(code), vidima);
                            if (bigliettoVidimato != null)
                            {
                                ret.Valid = true;
                                ret.State = Common.TicketStateEnum.Valido;
                                ret.Pax = bigliettoVidimato.Pax;
                            }
                            else
                            {
                                ret.State = Common.TicketStateEnum.Invalido;
                            }
                        }
                    }
                    else
                    {
                        //GESTIONE CUMULATIVI CON MESSAGGIO PASS DA TORNELLO
                        if (biglietto.Vidimato && (biglietto.Passed >= biglietto.Pax))
                        {
                            ret.Valid = false;
                            ret.State = Common.TicketStateEnum.Eccedente;
                        }
                        else if (biglietto.Annullato)
                        {
                            ret.Valid = false;
                            ret.State = Common.TicketStateEnum.Annullato;
                        }
                        else
                        {
                            var bigliettoVidimato = biglietto_Dal.Vidima(Convert.ToInt64(code), vidima);
                            if (bigliettoVidimato != null)
                            {
                                ret.Valid = true;
                                ret.State = Common.TicketStateEnum.Valido;
                                ret.Pax = bigliettoVidimato.Pax;
                            }
                            else
                            {
                                ret.State = Common.TicketStateEnum.Invalido;
                            }
                        }
                    }


 
                }
                else
                {
                    ret.State = Common.TicketStateEnum.Invalido;
                }


                return ret;
            }
            catch (ThreadAbortException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                ret.State = Common.TicketStateEnum.Invalido;
                return ret;
            }



        }

        public Common.TicketState CheckTicketTest(string barcode)
        {
            Common.TicketState ret = new Common.TicketState() { State = Common.TicketStateEnum.Invalido };
            try
            {
                ulong code = Convert.ToUInt64(barcode);
                var bigliettoVidimato = biglietto_Dal.Vidima(Convert.ToInt64(code));
                if (bigliettoVidimato != null)
                {
                    ret.Valid = true;
                    ret.State = Common.TicketStateEnum.Valido;
                    ret.Type = Convert.ToInt32(1);
                    //ret.sType = tipiBiglietto.FirstOrDefault(tpx => tpx.Id_TipoBiglietto == ret.Type).Descrizione;
                    ret.Pax = bigliettoVidimato.Pax;

                }
                else
                {
                    ret.State = Common.TicketStateEnum.Invalido;
                }

                return ret;
            }
            catch (ThreadAbortException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                ret.State = Common.TicketStateEnum.Invalido;
                return ret;
            }



        }
    }
}
