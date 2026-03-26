using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Globalization;
using System.IO;

namespace Presentation
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		protected override void OnStartup(StartupEventArgs e)
		{
			System.Threading.Thread.CurrentThread.CurrentUICulture = new CultureInfo("it-IT");
		}

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            LogExc(GetRecursiveInnerException(e.Exception));
        }


        #region Error Logging


        private Exception GetRecursiveInnerException(Exception exc)
        {
            if (exc.InnerException != null)
                return GetRecursiveInnerException(exc.InnerException);
            else
                return exc;
        }

        static object c = new object();
        private void LogExc(Exception ex, string message = "")
        {
            lock (c)
            {
                var logFile = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "\\SCV_Log.txt";
                var oldLogFile = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "\\SCV_Log_old.txt";

                try
                {
                    long len = 0;
                    using (FileStream fs = new FileStream(logFile, FileMode.OpenOrCreate, FileAccess.Read))
                        len = fs.Length;
                    if (File.Exists(logFile) && new FileInfo(logFile).Length > 1048576)
                    {
                        File.Copy(logFile, oldLogFile, true);
                        File.Delete(logFile);
                    }
                }
                catch { }
                try
                {
                    using (FileStream fs = new FileStream(logFile, FileMode.Append, FileAccess.Write))
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        sw.WriteLine(string.Format("{0}{1}{2}{3}Message:{4}{5}source:{6}{7}Stack Trace:{8}{9}TargetSite:{10}{11}"
                                                                                 , "---------------------------------------------------------"
                                                                                 , System.Environment.NewLine
                                                                                 , DateTime.Now.ToString()
                                                                                 , System.Environment.NewLine
                                                                                 , ex.Message
                                                                                 , System.Environment.NewLine
                                                                                 , ex.Source
                                                                                 , System.Environment.NewLine
                                                                                 , ex.StackTrace
                                                                                 , System.Environment.NewLine
                                                                                 , ex.StackTrace
                                                                                 , System.Environment.NewLine));


                    }
                }
                catch { }
            }
        }
        #endregion
	}
}
