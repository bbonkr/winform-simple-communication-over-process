using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Messaging.App.Abstractions;

namespace Messaging.App
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            ProcessActivator activator = null;
            try
            {
                activator = new ProcessActivator();
            }
            catch (Exception ex)
            {
                activator = null;
            }

            string data = null;
            if (activator != null)
            {
                try
                {
                    data = activator.GetParameter();
                }
                catch (Exception ex)
                {
                    data = null;
                }
            }

            // 하나의 프로세스만 실행되도록 합니다.
            bool isNewInstance = false;
            Mutex singleMutext = new Mutex(true, Constants.ProcessName, out isNewInstance);
            if (isNewInstance)
            {
                Application.Run(new AppLoader(data));
            }
            else
            {
                activator.Active(data);
            }
        }
    }
}
