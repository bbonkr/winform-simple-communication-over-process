using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Messaging.App.Abstractions;

namespace Messaging.App
{
    public partial class AppLoader : Form
    {
        public AppLoader(string data)
            : this()
        {
            this.data = data;
        }

        public AppLoader()
        {
            InitializeComponent();

            this.Shown += (s, e) => LoadMainType();

            this.StartPosition = FormStartPosition.CenterScreen;
        }

        private void LoadMainType()
        {
            ShowProcessInfo();

            NativeMethods.CHANGEFILTERSTRUCT changeFilter = new NativeMethods.CHANGEFILTERSTRUCT();
            changeFilter.size = (uint)Marshal.SizeOf(changeFilter);
            changeFilter.info = 0;
            if (!NativeMethods.ChangeWindowMessageFilterEx(
                this.Handle,
                NativeMethods.WM_COPYDATA,
                NativeMethods.ChangeWindowMessageFilterExAction.Allow,
                ref changeFilter))
            {
                int error = Marshal.GetLastWin32Error();
                MessageBox.Show(String.Format("The error {0} occurred.", error));
            }

            if (!String.IsNullOrEmpty(data))
            {
                SendMessage(data);
            }
            // 윈도우가 보이지 않으면 핸들을 가져오지 못합니다.
            //this.Hide();
        }

        private void SendMessage(string data)
        {
            this.textBox1.AppendText(data);
            this.textBox1.AppendText(Environment.NewLine);
        }
                
        protected override void WndProc(ref Message m)
        {
            // 전송된 메시지를 처리합니다.
            if (m.Msg == NativeMethods.WM_COPYDATA)
            {
                // Extract the file name
                NativeMethods.COPYDATASTRUCT copyData = (NativeMethods.COPYDATASTRUCT)Marshal.PtrToStructure(m.LParam, typeof(NativeMethods.COPYDATASTRUCT));
                int dataType = (int)copyData.dwData;
                if (dataType == 2)
                {
                    string data = Marshal.PtrToStringAnsi(copyData.lpData);

                    SendMessage(data); 
                }
                else
                {
                    MessageBox.Show(String.Format("Unrecognized data type = {0}.", dataType), "SendMessageDemo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                base.WndProc(ref m);
            }
        }

        private void ShowProcessInfo()
        {
            var proc = Process.GetCurrentProcess();

            var display = $"{ proc.ProcessName } : { proc.Id }";

            this.textBox2.Text = display;
        }

        private readonly string data;
    }
}
