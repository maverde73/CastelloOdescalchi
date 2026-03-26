using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scv_Model
{
	public class ClosingDetailWindowEventArgs
	{
		#region Private Fields

		private int detailID = 0;
		private bool processImmediately = false;

		#endregion// Private Fields



		#region Public Properties

		public int DetailID
		{
			get { return detailID; }
			set { detailID = value; }
		}

		public bool ProcessImmediately
		{
			get { return processImmediately; }
			set { processImmediately = value; }
		}

		#endregion // Public Properties



		#region Constructors

		public ClosingDetailWindowEventArgs()
		{ }

		public ClosingDetailWindowEventArgs(int detailID, bool processImmediately)
		{
			this.DetailID = detailID;
			this.ProcessImmediately = processImmediately;
		}

		#endregion// Constructors
	}
}
