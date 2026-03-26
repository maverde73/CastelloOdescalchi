using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scv_Model
{
	public class CommonDelegates
	{
		public delegate void ClosingDetailWindowEventHandler(object sender, ClosingDetailWindowEventArgs e);
		public delegate void GuideAssignmentEventHandler(object sender, GuideAssignmentEventArgs e);
		public delegate void GuideAssignmentDelegate();
	}
}
