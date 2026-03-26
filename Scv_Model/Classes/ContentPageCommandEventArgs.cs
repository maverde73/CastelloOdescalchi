using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scv_Model
{
	public class ContentPageCommandEventArgs : IContentPageCommandEventArgs
	{

		#region private fields

		private List<int> selectedIDs = null;

		private BaseFilter filter = null;

		#endregion

		public ContentPageCommandEventArgs()
		{ }

		public ContentPageCommandEventArgs(string pageTemplateName, ContentPageCommandType commandType, string commandArgument)
		{
			this.PageTemplateName = pageTemplateName;
			this.CommandType = commandType;
			this.CommandArgument = CommandArgument;
		}

		#region IContentPageEventArgs Members

		public string PageTemplateName { get; set; }

		public ContentPageCommandType CommandType { get; set; }

		public string CommandArgument { get; set; }

		public List<int> SelectedIDs
		{
			get
			{
				if (selectedIDs == null)
					selectedIDs = new List<int>();
				return selectedIDs;
			}
			set { selectedIDs = value; }
		}

		public BaseFilter Filter
		{
			get
			{
				if (filter == null)
					filter = new BaseFilter();
				return filter;
			}
			set { filter = value; }
		}

		#endregion
	}
}
