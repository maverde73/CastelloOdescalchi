using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.ComponentModel;
using System.Configuration.Install;
using System.Security.Principal;
using System.Runtime.InteropServices;

namespace Scv_MailService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                using (Scv_MailService service = new Scv_MailService())
                   service.Start(null);

                System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite);
            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[] 
			    { 
				    new Scv_MailService() 
			    };
                ServiceBase.Run(ServicesToRun);
            }
        }
    }
}
