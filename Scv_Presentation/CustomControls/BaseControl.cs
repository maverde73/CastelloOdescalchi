using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;

namespace Presentation
{
	public abstract class BaseControl : UserControl
	{
		public abstract void UpdateChildren();
	}
}
