using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scv_Model
{
	public class TextPartFilterItem
	{
		#region Private Fields

		List<string> variables = null;

		#endregion// Private Fields



		#region Public Properties

		public int LanguageID { get; set; }

		public string Part { get; set; }

		public int StartLineFeeds { get; set; }

		public int EndLineFeeds { get; set; }

		public bool Colon { get; set; }

		public List<string> Variables
		{
			get
			{
				if (variables == null)
					variables = new List<string>();
				return variables;
			}
			set { variables = value; }
		}

		public string VariablesSeparator { get; set; }

		public VariablePosition VarsPosition { get; set; }
		
		#endregion//Public Properties

		

		#region Constructors

		public TextPartFilterItem()
		{
			LanguageID = 0;
			Part = string.Empty;
			StartLineFeeds = 0;
			EndLineFeeds = 0;
		}

		public TextPartFilterItem(int languageID, string part)
		{
			this.LanguageID = languageID;
			this.Part = part;
		}

		public TextPartFilterItem(int languageID, string part, int endLineFeeds)
		{
			this.LanguageID = languageID;
			this.Part = part;
			this.EndLineFeeds = EndLineFeeds;
		}

		public TextPartFilterItem(int languageID, string part, bool colon, List<string> variables, string variablesSeparator, VariablePosition varsPosition,  int endLineFeeds)
		{
			this.LanguageID = languageID;
			this.Part = part;
			this.EndLineFeeds = EndLineFeeds;
			this.Variables = variables;
			this.VariablesSeparator = variablesSeparator;
			this.VarsPosition = varsPosition;
			this.Colon = colon;
		}

		public TextPartFilterItem(int languageID, string part, int startLineFeeds, int endLineFeeds)
		{
			this.LanguageID = languageID;
			this.Part = part;
			this.EndLineFeeds = EndLineFeeds;
			this.StartLineFeeds = startLineFeeds;
		}

		public TextPartFilterItem(int languageID, string part, bool colon, List<string> variables, string variablesSeparator, VariablePosition varsPosition, int startLineFeeds, int endLineFeeds)
		{
			this.LanguageID = languageID;
			this.Part = part;
			this.EndLineFeeds = EndLineFeeds;
			this.StartLineFeeds = startLineFeeds;
			this.Variables = variables;
			this.VariablesSeparator = variablesSeparator;
			this.VarsPosition = varsPosition;
			this.Colon = colon;
		}

		#endregion// Constructors
	}

	public enum VariablePosition
	{
		Start = 0,
		End
	}
}
