using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scv_Model
{
	public class PrintableTicket
	{
		#region Public Properties

		public TicketType TicketType { get; set; }
		public int TicketNumber { get; set; }
		public string TicketProtocol { get; set; }
		public string TType
		{
			get
			{
				string result = string.Empty;
				switch (TicketType)
				{
					case Scv_Model.TicketType.FullPrice:
						result = "Intero";
						break;

					case Scv_Model.TicketType.Discount:
						result = "Ridotto";
						break;

					case Scv_Model.TicketType.Free:
						result = "Omaggio";
						break;

                    case Scv_Model.TicketType.Cumulative:
                        result = "Cumulativo";
                        break;
				}

				return result;
			}
		}

        private int pax = 1;

        public int Pax
        {
            get { return pax; }
            set { pax = value; }
        }
        

		#endregion// Public Properties



		#region Constructors

		public PrintableTicket()
		{ }



        public PrintableTicket(TicketType ticketType, int ticketNumber, string ticketProtocol, int pax = 1)
        {
            this.Pax = pax;
            this.TicketType = ticketType;
            this.TicketNumber = ticketNumber;
            this.TicketProtocol = ticketProtocol;
        }

		#endregion// Constructors
	}
}
