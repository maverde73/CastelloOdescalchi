using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Scv_Entities;
using Scv_Model;

namespace Scv_Dal
{
	public class LK_TestoStandard_Dal
	{
		public LK_TestoStandard GetText(int languageID, string part)
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				LK_TestoStandard ts =  _context.LK_TestoStandard.FirstOrDefault(x => x.Id_Lingua == languageID && x.Parte == part);
				ts.Testo = ts.Testo.Trim();
				return ts;
			}
		}

        public LK_TestoStandard GetText(int languageID, string part, IN_VIAEntities _context)
        {
            LK_TestoStandard ts = _context.LK_TestoStandard.FirstOrDefault(x => x.Id_Lingua == languageID && x.Parte == part);
            ts.Testo = ts.Testo.Trim();
            return ts;
        }

		public LK_TestoRisposta GetTestoRisposta(int languageID, int responseID)
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				return _context.LK_TestoRisposta.FirstOrDefault(x => x.Id_Lingua == languageID && x.Id_TipoRisposta == responseID);
			}
		}

        private LK_TestoRisposta GetTestoRisposta(int languageID, int responseID, IN_VIAEntities _context)
        {
            return _context.LK_TestoRisposta.FirstOrDefault(x => x.Id_Lingua == languageID && x.Id_TipoRisposta == responseID);
        }

		public string GetBody(int languageID, int responseID, int petitionerID, List<TextPartFilterItem> filter, bool addHeader = true, bool visitorBold = true)
		{
			string bodyPart = string.Empty;

			EmailBody obj = new EmailBody();

			obj.Mittente = GetText(languageID, "050").Testo;
			obj.TipoRisposta = GetTestoRisposta(languageID, responseID).Testo;

			Richiedente r = new Richiedente_Dal().GetPetitionerByID(petitionerID);
			obj.Richiedente = (r.LK_Titolo != null ? r.LK_Titolo.Sigla + " " : string.Empty) + (visitorBold ? "<b>" : string.Empty) + r.Cognome + " " + r.Nome + (visitorBold ? "</b>" : string.Empty);

			if (addHeader)
			{
				bodyPart += obj.TipoRisposta;

				bodyPart += "<br/><br/>" + obj.Richiedente + "<br/>";
			}

			bodyPart += AddParts(filter);

			return bodyPart;
		}

        public string GetBody(int languageID, int responseID, int petitionerID, List<TextPartFilterItem> filter,IN_VIAEntities _context, bool addHeader = true, bool visitorBold = true)
        {
            string bodyPart = string.Empty;

            EmailBody obj = new EmailBody();

            obj.Mittente = GetText(languageID, "050", _context).Testo;
            obj.TipoRisposta = GetTestoRisposta(languageID, responseID, _context).Testo;

            Richiedente r = _context.Richiedenti.FirstOrDefault(rx => rx.Id_Richiedente == petitionerID);
            
            obj.Richiedente = (r.LK_Titolo != null ? r.LK_Titolo.Sigla + " " : string.Empty) + (visitorBold ? "<b>" : string.Empty) + r.Cognome + " " + r.Nome + (visitorBold ? "</b>" : string.Empty);

            if (addHeader)
            {
                bodyPart += obj.TipoRisposta;

                bodyPart += "<br/><br/>" + obj.Richiedente + "<br/>";
            }

            bodyPart += AddParts(filter, _context);

            return bodyPart;
        }

		public string AddParts(List<TextPartFilterItem> filter, bool removeInnerLineBreaks = false, bool removeInnerLineFeeds = false)
		{
			string bodyPart = string.Empty;

			foreach (TextPartFilterItem f in filter)
			{
				if (f.StartLineFeeds == 0)
					bodyPart += " ";
				else
					for (int i = 0; i < f.StartLineFeeds; i++)
						bodyPart += "<br />";

				if (f.VarsPosition == VariablePosition.Start)
					bodyPart += " " + AddVariables(f.Variables, f.VariablesSeparator);

				bodyPart += GetText(f.LanguageID, f.Part).Testo + (f.Colon ? ":" : string.Empty);

				if (removeInnerLineFeeds)
					bodyPart = bodyPart.Replace("<br /><br />", "<br />").Replace("<br/><br/>", "<br/>").Replace("<br>", "<br>");

				if (removeInnerLineBreaks)
					bodyPart = bodyPart.Replace("<br />", " ").Replace("<br/>", " ").Replace("<br>", " ");

				if (f.VarsPosition == VariablePosition.End)
					bodyPart += " " + AddVariables(f.Variables, f.VariablesSeparator);

				for (int i = 0; i < f.EndLineFeeds; i++)
					bodyPart += "<br />";
			}

			return bodyPart.Trim();
		}

        public string AddParts(List<TextPartFilterItem> filter,IN_VIAEntities _context, bool removeInnerLineBreaks = false, bool removeInnerLineFeeds = false)
        {
            string bodyPart = string.Empty;

            foreach (TextPartFilterItem f in filter)
            {
                if (f.StartLineFeeds == 0)
                    bodyPart += " ";
                else
                    for (int i = 0; i < f.StartLineFeeds; i++)
                        bodyPart += "<br />";

                if (f.VarsPosition == VariablePosition.Start)
                    bodyPart += " " + AddVariables(f.Variables, f.VariablesSeparator);

                bodyPart += GetText(f.LanguageID, f.Part, _context).Testo + (f.Colon ? ":" : string.Empty);

                if (removeInnerLineFeeds)
                    bodyPart = bodyPart.Replace("<br /><br />", "<br />").Replace("<br/><br/>", "<br/>").Replace("<br>", "<br>");

                if (removeInnerLineBreaks)
                    bodyPart = bodyPart.Replace("<br />", " ").Replace("<br/>", " ").Replace("<br>", " ");

                if (f.VarsPosition == VariablePosition.End)
                    bodyPart += " " + AddVariables(f.Variables, f.VariablesSeparator);

                for (int i = 0; i < f.EndLineFeeds; i++)
                    bodyPart += "<br />";
            }

            return bodyPart.Trim();
        }

		private string AddVariables(List<string> variables, string variablesSeparator)
		{
			string vars = string.Empty;

			foreach (string v in variables)
			{
				if (vars.Length > 0)
					vars += variablesSeparator + " ";
				vars += v;
			}

			return vars.Trim();
		}

	}
}
