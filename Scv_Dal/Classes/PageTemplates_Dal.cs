using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Scv_Model;
using System.Xml.Serialization;
using System.Xml;
using System.IO;

namespace Scv_Dal
{
	public class PageTemplates_Dal
	{

		public static List<PageTemplate> GetPageTemplates(string templatePath)
		{
			List<PageTemplate> list = new List<PageTemplate>();
			
			if (File.Exists(templatePath))
			{

				XmlSerializer ser = new XmlSerializer(typeof(List<PageTemplate>), new XmlRootAttribute() { ElementName = "PageTemplates" });

				XmlTextReader reader = new XmlTextReader(templatePath);

				try
				{
					list = (List<PageTemplate>)ser.Deserialize(reader);

				}
				catch (Exception e)
				{
					throw (e);
				}
			}
			else
				throw (new Exception("Template filename not found"));
			//list = list.Where(u => permissions.Contains(u.ID)).ToList();
			return list;
		}

	}
}
