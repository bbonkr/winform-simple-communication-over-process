using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Deployment.Application;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Messaging;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Messaging.App.Abstractions;

namespace Messaging.App
{
    public class ProcessActivator
    {
        public ProcessActivator()
        {

        }

        public bool Active(string data)
        {
            var processId = Process.GetCurrentProcess().Id;
            string targetProcessName = Constants.ProcessName; // 프로세스 이름

            var process = Process.GetProcesses()
                .Where(p => p.ProcessName == targetProcessName && p.Id != processId)
                .FirstOrDefault();

            if (process != null)
            {
                process.WaitForInputIdle(100);
                process.Refresh();

                //! 메인 윈도우가 숨겨지면 핸들을 찾을 수 없습니다.
                // AppBrowser를 Clickonce 게시 메인 프로젝트로 옮겨야 하는 원인입니다.
                IntPtr ptrWnd = process.MainWindowHandle;
                IntPtr ptrCopyData = IntPtr.Zero;

                try
                {
                    // Create the data structure and fill with data
                    NativeMethods.COPYDATASTRUCT copyData = new NativeMethods.COPYDATASTRUCT();
                    copyData.dwData = new IntPtr(2);    // Just a number to identify the data type
                    copyData.cbData = data.Length + 1;  // One extra byte for the \0 character
                    copyData.lpData = Marshal.StringToHGlobalAnsi(data);

                    // Allocate memory for the data and copy
                    ptrCopyData = Marshal.AllocCoTaskMem(Marshal.SizeOf(copyData));
                    Marshal.StructureToPtr(copyData, ptrCopyData, false);

                    // Send the message 메시지를 전송합니다.
                    NativeMethods.SendMessage(ptrWnd, NativeMethods.WM_COPYDATA, IntPtr.Zero, ptrCopyData);
                }
                catch (Exception ex)
                {
                    throw;
                }
                finally
                {
                    // Free the allocated memory after the contol has been returned
                    if (ptrCopyData != IntPtr.Zero)
                        Marshal.FreeCoTaskMem(ptrCopyData);
                }

                return true;
            }

            return false;
        }

        public string GetParameter()
        {
            var nameValueTable = new NameValueCollection();
            
            if (ApplicationDeployment.IsNetworkDeployed)
            {
                // TODO URL 매개변수 사용을 활성화해야 합니다.
                //! Publish > Options > Manifests > [V] Allow URL parameters to be passed to application
                string queryString = ApplicationDeployment.CurrentDeployment.ActivationUri.Query;

                return queryString;
            }

            return String.Empty;
        }

    }
}
