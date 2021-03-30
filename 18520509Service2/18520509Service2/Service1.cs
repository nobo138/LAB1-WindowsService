using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.IO;

namespace _18520509Service2
{
    public partial class Service1 : ServiceBase
    {
        Timer timer = new Timer();
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            // TODO: Add code here to start your service.
            WriteToFile("Service is started at " + DateTime.Now);
            timer.Elapsed += new ElapsedEventHandler(CheckProcess);
            timer.Interval = 5000; //number in milisecinds
            timer.Enabled = true;
        }
        private void OnElapsedTime(object source, ElapsedEventArgs e)
        {
            CheckProcess(source,e);
        }
        protected override void OnStop()
        {
            // TODO: Add code here to perform any tear-down necessary to stop your service.
            WriteToFile("Service is stopped at " + DateTime.Now);
        }
        public void CheckProcess(object source, ElapsedEventArgs e)
        {
            Process[] processes = null;//create array proccess
            processes = Process.GetProcessesByName("notepad");//get process which have name "notepad"
            
            foreach (Process process in processes)
            {
                process.Kill();//Kill the processes if these processes running
            }
            if (processes.Length == 0) //check the number of process
            {
                Process.Start("notepad.exe"); //Start process notepad.exe if process don't running
                WriteToFile("Service is recall at " + DateTime.Now);
            }
        }

        public void WriteToFile(string Message)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\Logs";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string filepath = AppDomain.CurrentDomain.BaseDirectory +
            "\\Logs\\ServiceLog_" + DateTime.Now.Date.ToShortDateString().Replace('/', '_') +
            ".txt";
            if (!File.Exists(filepath))
            {
                // Create a file to write to.
                using (StreamWriter sw = File.CreateText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
        }
    }
}
