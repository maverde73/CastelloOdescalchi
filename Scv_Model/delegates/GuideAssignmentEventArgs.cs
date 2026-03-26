using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scv_Model
{
	public class GuideAssignmentEventArgs
	{
		#region Private Fields

		private int percentDone = 0;

		private bool done = false;

		#endregion// Private Fields



		#region Public Properties

		public int PercentDone
		{
			get { return percentDone; }
			set { percentDone = value; }
		}

		public bool Done
		{
			get { return done; }
			set { done = value; }
		}

		#endregion // Public Properties



		#region Constructors

		public GuideAssignmentEventArgs()
		{ }

		public GuideAssignmentEventArgs(int percentDone, bool done)
		{
			this.PercentDone = percentDone;
			this.Done = done;
		}

		#endregion// Constructors
	}
}
