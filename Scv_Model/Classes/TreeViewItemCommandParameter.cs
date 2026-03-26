using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scv_Model
{
	public class TreeViewItemCommandParameter
	{
		#region Private Fields

		private ContentPageCommandType commandType;

		private int commandValue = 0;

		private string commandArgument = string.Empty;

		#endregion// Private Fields



		#region Public Properties

		public ContentPageCommandType CommandType
		{
			get { return commandType; }
			set { commandType = value; }
		}

		public int CommandValue
		{
			get { return commandValue; }
			set { commandValue = value; }
		}

		public string CommandArgument
		{
			get { return commandArgument; }
			set { commandArgument = value; }
		}

		#endregion// Public Properties



		#region Constructors

		public TreeViewItemCommandParameter()
		{ }

		public TreeViewItemCommandParameter(ContentPageCommandType commandType, int commandValue, string commandArgument)
		{
			this.CommandType = commandType;
			this.CommandValue = commandValue;
			this.CommandArgument = commandArgument;
		}

		#endregion// Constructors

	}
}
