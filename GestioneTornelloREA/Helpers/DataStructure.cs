using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace GestioneTornelloREA.Helpers
{
    public class DataStructure
    {
        private static XmlDeclaration _declaration { get; set; }

        private const string ADDR = "1";
        private const string PROTOCOL = "1.0";

        public static byte[] GetResponseTest(string id)
        {
            var _document = new XmlDocument();
            _declaration = _document.CreateXmlDeclaration("1.0", "UTF-8", null);
            var documentElement = _document.DocumentElement;
            _document.InsertBefore(_declaration, documentElement);
            var xmlElement = _document.CreateElement("cmf");
            _document.AppendChild(xmlElement);
            xmlElement.SetAttribute("id", id);
            xmlElement.SetAttribute("addr", ADDR);
            xmlElement.SetAttribute("protocol", PROTOCOL);
            var xmlElement1 = _document.CreateElement("return");
            xmlElement.AppendChild(xmlElement1);
            var str = _document.CreateElement("act");
            XmlElement xmlElement2 = _document.CreateElement("cnt");
            XmlElement xmlElement3 = _document.CreateElement("txt");
            xmlElement1 = _document.CreateElement("return");
            xmlElement.AppendChild(xmlElement1);
            str = _document.CreateElement("time");
            DateTime now = DateTime.Now;
            str.InnerText = now.ToString("yyyyMMddHHmmss");
            xmlElement1.AppendChild(str);
            var stringWriter = new StringWriter();
            var xmlTextWriter = new XmlTextWriter(stringWriter);
            _document.WriteTo(xmlTextWriter);
            var str1 = stringWriter.ToString();
            xmlTextWriter.Close();
            return Encoding.UTF8.GetBytes(str1);
        }

        public static byte[] GetResponseEntry(string id, string direction, string textToShow, string passage)
        {
            var xmlDocument = new XmlDocument();
            _declaration = xmlDocument.CreateXmlDeclaration("1.0", "UTF-8", null);
            var documentElement = xmlDocument.DocumentElement;
            xmlDocument.InsertBefore(_declaration, documentElement);

            var xmlElement = xmlDocument.CreateElement("cmf");
            xmlDocument.AppendChild(xmlElement);
            xmlElement.SetAttribute("id", id);
            xmlElement.SetAttribute("addr", ADDR);
            xmlElement.SetAttribute("protocol", PROTOCOL);

            var xmlElement1 = xmlDocument.CreateElement("return");
            xmlElement.AppendChild(xmlElement1);
            var str = xmlDocument.CreateElement("act");
            XmlElement xmlElement2 = xmlDocument.CreateElement("cnt");
            XmlElement xmlElement3 = xmlDocument.CreateElement("txt");
            str.InnerText = direction;
            xmlElement3.InnerText = textToShow;
            xmlElement2.InnerText = passage;
            xmlElement1.AppendChild(str);
            xmlElement1.AppendChild(xmlElement2);
            xmlElement1.AppendChild(xmlElement3);

            var stringWriter = new StringWriter();
            var xmlTextWriter = new XmlTextWriter(stringWriter);
            xmlDocument.WriteTo(xmlTextWriter);
            var str1 = stringWriter.ToString();
            xmlTextWriter.Close();
            return Encoding.UTF8.GetBytes(str1);
        }

        public static byte[] GetRequestOpen(string id, string direction, string textToShow, string passage)
        {
            var xmlDocument = new XmlDocument();
            _declaration = xmlDocument.CreateXmlDeclaration("1.0", "UTF-8", null);
            var documentElement = xmlDocument.DocumentElement;
            xmlDocument.InsertBefore(_declaration, documentElement);

            var xmlElement = xmlDocument.CreateElement("cmf");
            xmlDocument.AppendChild(xmlElement);
            xmlElement.SetAttribute("id", 10000+id);
            xmlElement.SetAttribute("addr", ADDR);
            xmlElement.SetAttribute("protocol", PROTOCOL);
            
            var xmlElement1 = xmlDocument.CreateElement("open");
            xmlElement.AppendChild(xmlElement1);
            var str = xmlDocument.CreateElement("act");
            XmlElement xmlElement2 = xmlDocument.CreateElement("cnt");
            XmlElement xmlElement3 = xmlDocument.CreateElement("txt");
            str.InnerText = direction;
            xmlElement3.InnerText = textToShow;
            xmlElement2.InnerText = passage;
            xmlElement1.AppendChild(str);
            xmlElement1.AppendChild(xmlElement2);
            xmlElement1.AppendChild(xmlElement3);

            var stringWriter = new StringWriter();
            var xmlTextWriter = new XmlTextWriter(stringWriter);
            xmlDocument.WriteTo(xmlTextWriter);
            var str1 = stringWriter.ToString();
            xmlTextWriter.Close();
            return Encoding.UTF8.GetBytes(str1);
        }

        public static byte[] GetResponsePass(string id)
        {
            var xmlDocument = new XmlDocument();
            _declaration = xmlDocument.CreateXmlDeclaration("1.0", "UTF-8", null);
            var documentElement = xmlDocument.DocumentElement;
            xmlDocument.InsertBefore(_declaration, documentElement);
            var xmlElement = xmlDocument.CreateElement("cmf");
            xmlDocument.AppendChild(xmlElement);
            xmlElement.SetAttribute("id", id);
            xmlElement.SetAttribute("addr", ADDR);
            xmlElement.SetAttribute("protocol", PROTOCOL);
            var xmlElement1 = xmlDocument.CreateElement("return");
            xmlElement1.InnerText = "true";
            xmlElement.AppendChild(xmlElement1);
            var stringWriter = new StringWriter();
            var xmlTextWriter = new XmlTextWriter(stringWriter);
            xmlDocument.WriteTo(xmlTextWriter);
            var str1 = stringWriter.ToString();
            xmlTextWriter.Close();
            return Encoding.UTF8.GetBytes(str1);
        }

        public static string GetAttributeFromNode(string text, string node, string attribute)
        {
            string ret = null;
            var _document = new XmlDocument();
            _document.LoadXml(text);
            XmlNode xmlNodes = _document.SelectSingleNode(node);

            for (int i = 0; i < xmlNodes.Attributes.Count; i++)
            {
                string name = xmlNodes.Attributes[i].Name;
                string str3 = name;
                if (name != null)
                {
                    if (str3 == attribute)
                    {
                        ret = xmlNodes.Attributes[i].Value;
                    }
                }
            }
            return string.IsNullOrEmpty(ret) ? "1" : ret;
        }

        public static string GetInnerFromNode(string text, string parent, string child)
        {
            string ret = null;
            var _document = new XmlDocument();
            _document.LoadXml(text);
            XmlNode xmlNodes = _document.SelectSingleNode(parent);
            var childNodes = xmlNodes.ChildNodes;
            foreach (XmlNode node in childNodes)
            {
                if (node.Name == child)
                    ret = node.InnerText;
            }

            return ret;
        }

        public static bool NodeHasChild(string text, string node, string toCompare)
        {
            var ret = false;

            var _document = new XmlDocument();
            _document.LoadXml(text);
            XmlNode xmlNodes = _document.SelectSingleNode(node);
            XmlNodeList childNodes = xmlNodes.ChildNodes;
            for (int i = 0; i < childNodes.Count; i++)
            {
                if (childNodes.Item(i).Name == toCompare)
                    ret = true;
            }

            return ret;
        }



    }
}
