using GestioneTornelloREA.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GestioneTornelloREA.Business
{
    public class TransitionBusiness
    {

        private static TerminalHelper _terminalController = new TerminalHelper();

        private static int passageRemaning { get; set;}

        public static void StartService()
        {
            _terminalController.OnTestReceived += _terminalController_OnTestReceived;
            _terminalController.OnEntryReceived += _terminalController_OnEntryReceived;
            _terminalController.OnPassReceived += _terminalController_OnPassReceived;
            _terminalController.OnReturnReceived += _terminalController_OnReturnReceived;
            _terminalController.ListOnPort();
        }

        private static void _terminalController_OnReturnReceived(object sender, string request)
        {
            /*if(passageRemaning>0)
                _terminalController.SendRequestOpen(GetVersoEntrata(), "Biglietto valido", passageRemaning.ToString());*/
        }

        private static void _terminalController_OnPassReceived(object sender, string request)
        {
            _terminalController.SendResponsePass();
            passageRemaning--;
            if (passageRemaning > 0)
            {
                Thread.Sleep(500);
                _terminalController.SendRequestOpen(GetVersoEntrata(), passageRemaning.ToString(), "1");
            }
        }

        private static void _terminalController_OnEntryReceived(object sender, string barcode)
        {
            var message = GetCanPass(barcode);

            if (passageRemaning > 0)
            {
                _terminalController.SendResponseEntry(GetVersoEntrata(), message, "1");
            }
            else
            {
                _terminalController.SendResponseEntry("x", message,"0");
            }
            
        }

       

        private static void _terminalController_OnTestReceived(object sender, string time)
        {
            _terminalController.SendOk();
        }




        public static string GetCanPass(string barcode)
        {
            TicketChecker ticket_Checker = null;
            ticket_Checker = new TicketChecker();
            var ticketState = new Common.TicketState();
            //ticketState.State = Common.TicketStateEnum.Valido;

            var textToShow = "";
            passageRemaning = 0;
            ticketState = ticket_Checker.CheckTicket(barcode);
            //Thread.Sleep(3000);
            switch (ticketState.State)
            {
                case Common.TicketStateEnum.Valido:
                    textToShow = "Biglietto valido per "+ ticketState.Pax;
                    passageRemaning = ticketState.Pax;
                    break;

                case Common.TicketStateEnum.Validato:
                    textToShow = "Biglietto già vidimato";
                    break;

                case Common.TicketStateEnum.Scaduto:
                    textToShow = "Biglietto scaduto";
                    break;

                case Common.TicketStateEnum.Invalido:
                    textToShow = "Biglietto non valido";
                    break;

                default:
                    textToShow = "Biglietto non valido";
                    break;
            }
            return textToShow;
        }

        private static string GetVersoEntrata()
        {
            var act = Convert.ToInt32(ConfigurationManager.AppSettings["versoentrata"]);
            string ret = "";
            if (act != 0)
            {
                if (act != 1)
                {
                    ret= "x";
                }
                else
                {
                    ret = "r";
                }
            }
            else
            {
                ret = "l";
            }

            return ret;

        }

        public static void StopService()
        {
            _terminalController.StopListen();
        }

        public static void SendOpen()
        {
            _terminalController.SendRequestOpen("l", "Open", "4");
        }

        public static void SendPassRes()
        {
            _terminalController.SendResponsePass();
        }
    }
}
