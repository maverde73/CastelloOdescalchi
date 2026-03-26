using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scv_Model
{
	public class TicketNumberResult
	{
		#region Private Fields

		private string message = string.Empty;

		private bool success = true;

		private int diff = 0;

		#endregion// Private Fields



		#region Public Properties

		public string Message
		{
			get { return message; }
			set { message = value; }
		}

		public bool Success
		{
			get { return success; }
			set { success = value; }
		}

		public int Diff
		{
			get { return diff; }
			set { diff = value; }
		}

		#endregion// Public Properties

	}
}
