using Scv_Dal;
using Scv_Entities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GestioneTornelloREA.Business
{
    public class TicketChecker
    {
        Biglietto_Dal biglietto_Dal = new Biglietto_Dal();
        List<LK_TipoBiglietto> tipiBiglietto = null;
        int giorniValidita = 3;
        List<Biglietto> biglietti = null;
        int versoEntrata = 0;

        #region Logging

        private static readonly object _logLock = new object();

        private static void Log(string message)
        {
            lock (_logLock)
            {
                try
                {
                    var logFile = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "\\TicketChecker_Log.txt";
                    var oldLogFile = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "\\TicketChecker_Log_old.txt";

                    try
                    {
                        if (File.Exists(logFile) && new FileInfo(logFile).Length > 1048576)
                        {
                            File.Copy(logFile, oldLogFile, true);
                            File.Delete(logFile);
                        }
                    }
                    catch { }

                    using (FileStream fs = new FileStream(logFile, FileMode.Append, FileAccess.Write))
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        sw.WriteLine(string.Format("[{0}] {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), message));
                    }
                }
                catch { }
            }
        }

        #endregion



        public Common.TicketState CheckTicket(string barcode)
        {
            Log(string.Format("--- START CheckTicket | barcode={0}", barcode));

            Common.TicketState ret = new Common.TicketState() { State = Common.TicketStateEnum.Invalido };
            try
            {
                Log("Step 1: Loading ticket types from DB...");
                tipiBiglietto = biglietto_Dal.Get_LK_TipoBiglietto_List();
                Log(string.Format("Step 1: OK, {0} ticket types loaded", tipiBiglietto != null ? tipiBiglietto.Count.ToString() : "null"));

                if (ConfigurationManager.AppSettings["giornivalidita"] != null)
                    giorniValidita = Convert.ToInt32(ConfigurationManager.AppSettings["giornivalidita"]);

                Log(string.Format("Step 2: Decoding barcode. giorniValidita={0}", giorniValidita));
                ulong code = Convert.ToUInt64(barcode);
                Log(string.Format("Step 2: Barcode parsed as ulong={0}", code));

                uint pax = 0;
                DateTime date = new DateTime();
                uint cont = 0;
                uint tipo = 0;

                Barcode.DecodeBarCode(ref pax, ref date, ref cont, ref tipo, code);
                Log(string.Format("Step 2: Decoded -> date={0}, pax={1}, tipo={2}, cont={3}",
                    date.ToString("yyyy-MM-dd HH:mm:ss"), pax, tipo, cont));

                ret.State = Common.TicketStateEnum.Valido;

                var expiryThreshold = DateTime.Now.AddDays(giorniValidita * -1).Date;
                Log(string.Format("Step 3: Expiry check -> ticketDate={0}, threshold={1}, now={2}",
                    date.Date.ToString("yyyy-MM-dd"), expiryThreshold.ToString("yyyy-MM-dd"), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));

                if (date.Date <= expiryThreshold)
                {
                    ret.State = Common.TicketStateEnum.Scaduto;
                    Log(string.Format("RESULT: SCADUTO | barcode={0}, ticketDate={1}, threshold={2}",
                        barcode, date.Date.ToString("yyyy-MM-dd"), expiryThreshold.ToString("yyyy-MM-dd")));
                    return ret;
                }

                ret.Pax = Convert.ToInt32(pax);
                ret.Date = date;

                Log(string.Format("Step 4: DB lookup for code={0}...", code));
                var biglietto = biglietto_Dal.Get_Biglietto_ByCode(Convert.ToInt64(code));

                if (biglietto != null)
                {
                    Log(string.Format("Step 4: Ticket found in DB. Vidimato={0}, Annullato={1}",
                        biglietto.Vidimato, biglietto.Annullato));

                    if (biglietto.Vidimato)
                    {
                        ret.Valid = false;
                        ret.State = Common.TicketStateEnum.Validato;
                        Log(string.Format("RESULT: VALIDATO (already used) | barcode={0}", barcode));
                    }
                    else if (biglietto.Annullato)
                    {
                        ret.Valid = false;
                        ret.State = Common.TicketStateEnum.Annullato;
                        Log(string.Format("RESULT: ANNULLATO | barcode={0}", barcode));
                    }
                    else
                    {
                        Log(string.Format("Step 5: Vidimazione for code={0}...", code));
                        var bigliettoVidimato = biglietto_Dal.Vidima(Convert.ToInt64(code));
                        if (bigliettoVidimato != null)
                        {
                            ret.Valid = true;
                            ret.State = Common.TicketStateEnum.Valido;
                            ret.Pax = bigliettoVidimato.Pax;
                            Log(string.Format("RESULT: VALIDO | barcode={0}, pax={1}", barcode, bigliettoVidimato.Pax));
                        }
                        else
                        {
                            ret.State = Common.TicketStateEnum.Invalido;
                            Log(string.Format("RESULT: INVALIDO (Vidima returned null) | barcode={0}", barcode));
                        }
                    }
                }
                else
                {
                    ret.State = Common.TicketStateEnum.Invalido;
                    Log(string.Format("RESULT: INVALIDO (ticket not found in DB) | barcode={0}, code={1}", barcode, code));
                }

                return ret;
            }
            catch (ThreadAbortException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                Log(string.Format("RESULT: INVALIDO (EXCEPTION) | barcode={0} | {1}: {2} | StackTrace: {3}",
                    barcode, ex.GetType().Name, ex.Message, ex.StackTrace));
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
