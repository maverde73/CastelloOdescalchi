using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace GestioneTornelloREA.Business
{
    public class Common
    {
      
        public enum TicketStateEnum
        {
            Valido,
            Scaduto,
            Invalido,
            Validato,
            Annullato
        }

        public struct TicketState
        {
            public TicketStateEnum State;
            public DateTime Date;
            public int Pax;
            public int Type;
            public bool Valid;
            public string sType;
            public int Pos;
            public override string ToString()
            {
                return State.ToString() + "\n" +
                    sType + "\n" +
                    "Pax:" + Pax.ToString() + "\n" +
                    "Data:" + Date.ToString("dd/MM/yyyy") + "\n" +
                    "Ora:" + Date.ToString("HHH\\:mm\\:ss");

            }
        }
    }
}
