using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Scv_Entities;
using Scv_Model;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Threading;
using System.IO;
using System.Drawing;

namespace Scv_Dal
{
	public class Helper_Dal
	{
		public static Size MainWindowSize { get; set; }

		public static Size ContentFrameSize { get; set; }

		public static int ContentFrameMarginTop { get; set; }

		public static int ContentFrameMarginBottom { get; set; }

		public static LK_Progressivi GetNewProgressive(string symbol, int year)
		{

			LK_Progressivi_Dal dal = new LK_Progressivi_Dal();
			LK_Progressivi pr = dal.GetProgressiviBySymbol(symbol);
			if (pr != null)
			{
				if (pr.Anno != year)
				{
					pr.Anno = pr.Anno > 0 ? year : 0;
					pr.Progr_UltimoUscito = pr.Anno > 0 ? 1 : pr.Progr_UltimoUscito + 1;
				}
				else
					pr.Progr_UltimoUscito++;
			}

			return pr;
		}

		public static List<YearItem> GetYears(int start, int end)
		{
			List<YearItem> list = new List<YearItem>();

			for (int i = start; i <= end; i++)
				list.Add(new YearItem(i));

			return list;
		}

		public static List<YearItem> GetYears(int start, int end, List<int> exclude)
		{
			List<YearItem> list = new List<YearItem>();

			for (int i = start; i <= end; i++)
				if (!exclude.Contains(i))
					list.Add(new YearItem(i));

			return list;
		}

		public static List<MonthItem> GetMonths()
		{
			List<MonthItem> list = new List<MonthItem>();

			list.Add(new MonthItem(1));
			list.Add(new MonthItem(2));
			list.Add(new MonthItem(3));
			list.Add(new MonthItem(4));
			list.Add(new MonthItem(5));
			list.Add(new MonthItem(6));
			list.Add(new MonthItem(7));
			list.Add(new MonthItem(8));
			list.Add(new MonthItem(9));
			list.Add(new MonthItem(10));
			list.Add(new MonthItem(11));
			list.Add(new MonthItem(12));

			return list;
		}

		public static List<Hour> GetHours(string startTime, string endTime, int intervalInMinutes)
		{
			var culture = CultureInfo.CreateSpecificCulture("it-IT");

			Thread.CurrentThread.CurrentCulture = culture;
			Thread.CurrentThread.CurrentUICulture = culture;

			TimeSpan tsStartTime = TimeSpan.ParseExact(startTime, "g", culture);

			TimeSpan tsEndTime = TimeSpan.ParseExact(endTime, "g", culture);

			List<Hour> hours = new List<Hour>();
			TimeSpan tsItem = tsStartTime;

			string sItem = "";
			while (tsItem <= tsEndTime)
			{
				sItem = string.Format("{0:hh\\:mm}", tsItem);
				hours.Add(new Hour { Time = sItem });
				tsItem = tsItem.Add(TimeSpan.FromMinutes(intervalInMinutes));
			}

			return hours;
		}

		public static string StripHTML(string source)
		{
			try
			{
				string result;

				// Remove HTML Development formatting
				// Replace line breaks with space
				// because browsers inserts space
				result = source.Replace("\r", " ");
				// Replace line breaks with space
				// because browsers inserts space
				result = result.Replace("\n", " ");
				// Remove step-formatting
				result = result.Replace("\t", string.Empty);
				// Remove repeating spaces because browsers ignore them
				result = System.Text.RegularExpressions.Regex.Replace(result,
																	  @"( )+", " ");

				// Remove the header (prepare first by clearing attributes)
				result = System.Text.RegularExpressions.Regex.Replace(result,
						 @"<( )*head([^>])*>", "<head>",
						 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
				result = System.Text.RegularExpressions.Regex.Replace(result,
						 @"(<( )*(/)( )*head( )*>)", "</head>",
						 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
				result = System.Text.RegularExpressions.Regex.Replace(result,
						 "(<head>).*(</head>)", string.Empty,
						 System.Text.RegularExpressions.RegexOptions.IgnoreCase);

				// remove all scripts (prepare first by clearing attributes)
				result = System.Text.RegularExpressions.Regex.Replace(result,
						 @"<( )*script([^>])*>", "<script>",
						 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
				result = System.Text.RegularExpressions.Regex.Replace(result,
						 @"(<( )*(/)( )*script( )*>)", "</script>",
						 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
				//result = System.Text.RegularExpressions.Regex.Replace(result,
				//         @"(<script>)([^(<script>\.</script>)])*(</script>)",
				//         string.Empty,
				//         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
				result = System.Text.RegularExpressions.Regex.Replace(result,
						 @"(<script>).*(</script>)", string.Empty,
						 System.Text.RegularExpressions.RegexOptions.IgnoreCase);

				// remove all styles (prepare first by clearing attributes)
				result = System.Text.RegularExpressions.Regex.Replace(result,
						 @"<( )*style([^>])*>", "<style>",
						 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
				result = System.Text.RegularExpressions.Regex.Replace(result,
						 @"(<( )*(/)( )*style( )*>)", "</style>",
						 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
				result = System.Text.RegularExpressions.Regex.Replace(result,
						 "(<style>).*(</style>)", string.Empty,
						 System.Text.RegularExpressions.RegexOptions.IgnoreCase);

				// insert tabs in spaces of <td> tags
				result = System.Text.RegularExpressions.Regex.Replace(result,
						 @"<( )*td([^>])*>", "\t",
						 System.Text.RegularExpressions.RegexOptions.IgnoreCase);

				// insert line breaks in places of <BR> and <LI> tags
				result = System.Text.RegularExpressions.Regex.Replace(result,
						 @"<( )*br( )*>", "\r",
						 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
				result = System.Text.RegularExpressions.Regex.Replace(result,
						 @"<( )*li( )*>", "\r",
						 System.Text.RegularExpressions.RegexOptions.IgnoreCase);

				// insert line paragraphs (double line breaks) in place
				// if <P>, <DIV> and <TR> tags
				result = System.Text.RegularExpressions.Regex.Replace(result,
						 @"<( )*div([^>])*>", "\r\r",
						 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
				result = System.Text.RegularExpressions.Regex.Replace(result,
						 @"<( )*tr([^>])*>", "\r\r",
						 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
				result = System.Text.RegularExpressions.Regex.Replace(result,
						 @"<( )*p([^>])*>", "\r\r",
						 System.Text.RegularExpressions.RegexOptions.IgnoreCase);

				// Remove remaining tags like <a>, links, images,
				// comments etc - anything that's enclosed inside < >
				result = System.Text.RegularExpressions.Regex.Replace(result,
						 @"<[^>]*>", string.Empty,
						 System.Text.RegularExpressions.RegexOptions.IgnoreCase);

				// replace special characters:
				result = System.Text.RegularExpressions.Regex.Replace(result,
						 @" ", " ",
						 System.Text.RegularExpressions.RegexOptions.IgnoreCase);

				result = System.Text.RegularExpressions.Regex.Replace(result,
						 @"&bull;", " * ",
						 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
				result = System.Text.RegularExpressions.Regex.Replace(result,
						 @"&lsaquo;", "<",
						 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
				result = System.Text.RegularExpressions.Regex.Replace(result,
						 @"&rsaquo;", ">",
						 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
				result = System.Text.RegularExpressions.Regex.Replace(result,
						 @"&trade;", "(tm)",
						 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
				result = System.Text.RegularExpressions.Regex.Replace(result,
						 @"&frasl;", "/",
						 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
				result = System.Text.RegularExpressions.Regex.Replace(result,
						 @"&lt;", "<",
						 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
				result = System.Text.RegularExpressions.Regex.Replace(result,
						 @"&gt;", ">",
						 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
				result = System.Text.RegularExpressions.Regex.Replace(result,
						 @"&copy;", "(c)",
						 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
				result = System.Text.RegularExpressions.Regex.Replace(result,
						 @"&reg;", "(r)",
						 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
				// Remove all others. More can be added, see
				// http://hotwired.lycos.com/webmonkey/reference/special_characters/
				result = System.Text.RegularExpressions.Regex.Replace(result,
						 @"&(.{2,6});", string.Empty,
						 System.Text.RegularExpressions.RegexOptions.IgnoreCase);

				// for testing
				//System.Text.RegularExpressions.Regex.Replace(result,
				//       this.txtRegex.Text,string.Empty,
				//       System.Text.RegularExpressions.RegexOptions.IgnoreCase);

				// make line breaking consistent
				result = result.Replace("\n", "\r");

				// Remove extra line breaks and tabs:
				// replace over 2 breaks with 2 and over 4 tabs with 4.
				// Prepare first to remove any whitespaces in between
				// the escaped characters and remove redundant tabs in between line breaks
				result = System.Text.RegularExpressions.Regex.Replace(result,
						 "(\r)( )+(\r)", "\r\r",
						 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
				result = System.Text.RegularExpressions.Regex.Replace(result,
						 "(\t)( )+(\t)", "\t\t",
						 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
				result = System.Text.RegularExpressions.Regex.Replace(result,
						 "(\t)( )+(\r)", "\t\r",
						 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
				result = System.Text.RegularExpressions.Regex.Replace(result,
						 "(\r)( )+(\t)", "\r\t",
						 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
				// Remove redundant tabs
				result = System.Text.RegularExpressions.Regex.Replace(result,
						 "(\r)(\t)+(\r)", "\r\r",
						 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
				// Remove multiple tabs following a line break with just one tab
				result = System.Text.RegularExpressions.Regex.Replace(result,
						 "(\r)(\t)+", "\r\t",
						 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
				// Initial replacement target string for line breaks
				string breaks = "\r\r\r";
				// Initial replacement target string for tabs
				string tabs = "\t\t\t\t\t";
				for (int index = 0; index < result.Length; index++)
				{
					result = result.Replace(breaks, "\r\r");
					result = result.Replace(tabs, "\t\t\t\t");
					breaks = breaks + "\r";
					tabs = tabs + "\t";
				}

				// That's it.
				//return result;

				return Regex.Replace(source, "<(.|\n)*?>", "");
			}
			catch (Exception e)
			{
				throw (e);
			}
		}

		public static byte[] StoreImageData(string imagePath)
		{
			return ReadFile(imagePath);
		}

		private static byte[] ReadFile(string sPath)
		{
			byte[] data = null;

			FileInfo fInfo = new FileInfo(sPath);
			long numBytes = fInfo.Length;

			FileStream fStream = new FileStream(sPath, FileMode.Open, FileAccess.Read);

			BinaryReader br = new BinaryReader(fStream);

			data = br.ReadBytes((int)numBytes);

			return data;
		}

		public static int GetOptimalGridRows(bool HasGroupPanel, int offset = 0)
		{
			Size screenResolution = System.Windows.Forms.SystemInformation.PrimaryMonitorSize;
			int rows = ((screenResolution.Height - 378 + offset - (HasGroupPanel ? 30 : 0)) / 25);
			rows = (ContentFrameSize.Height - (ContentFrameMarginTop + ContentFrameMarginBottom + 155 + (HasGroupPanel ? 25 : 0)) + offset) / 24;
			return rows > 0 ? rows : 1;
		}

		public static int GetOptimalScrollViewerHeight(bool HasGroupPanel)
		{
			return GetOptimalGridRows(HasGroupPanel) * 26;
		}

		public static string UpperCaseWords(string value)
		{
            if (!string.IsNullOrEmpty(value))
            {
                char[] array = value.ToCharArray();
                if (array.Length >= 1)
                    if (char.IsLower(array[0]))
                        array[0] = char.ToUpper(array[0]);

                for (int i = 1; i < array.Length; i++)
                    if (array[i - 1] == ' ')
                        if (char.IsLower(array[i]))
                            array[i] = char.ToUpper(array[i]);

                return new string(array);
            }
            else
                return value;

		}
	}
}
