using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scv_Model
{
	public class PageTemplate
	{

		public PageTemplate()
		{
		}

		public int ID { get; set; }

		public string ContentPageUrl { get; set; }

		public string ContentPageTitle { get; set; }

		public string ContentPageName { get; set; }

		public string MenuPageUrl { get; set; }

		public string ToolBarPageUrl { get; set; }

		public string ButtonText { get; set; }

		public string ButtonIconFileName { get; set; }

		public int Dynamic { get; set; }

		public static PageTemplate GetPageTemplate(List<PageTemplate> list, string name)
		{
			PageTemplate pageTemplate = null;

			foreach (PageTemplate p in list)
			{
				if (p.ContentPageName == name)
				{
					pageTemplate = p;
					break;
				}
			}

			return pageTemplate;
		}

		public static PageTemplate GetPageTemplate(List<PageTemplate> list, int ID)
		{
			PageTemplate pageTemplate = null;

			foreach (PageTemplate p in list)
			{
				if (p.ID == ID)
				{
					pageTemplate = p;
					break;
				}
			}

			return pageTemplate;
		}

	}
}
