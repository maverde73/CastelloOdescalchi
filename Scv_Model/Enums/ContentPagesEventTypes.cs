using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scv_Model
{
	public enum ContentPageCommandType
	{
		New = 1,
		Open,
		Save,
		Update,
		Delete,
		Print,
        Schedule,
		SelectionChanged,
		Filter,
		Send,
		Other = 100
	}
}
