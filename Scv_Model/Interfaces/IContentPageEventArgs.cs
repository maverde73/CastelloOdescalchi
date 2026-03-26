using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scv_Model
{
	interface IContentPageCommandEventArgs
	{
		string PageTemplateName { get; set; }
		ContentPageCommandType CommandType { get; set; }
		string CommandArgument { get; set; }
		List<int> SelectedIDs { get; set; }
		BaseFilter Filter { get; set; }
	}
}
