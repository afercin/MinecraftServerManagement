using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Timers;
using System.Windows;
using static MinecraftServerManagement.OutPutEvent;

namespace MinecraftServerManagement
{
    class MinecraftServer
    {
        private class map
        {
            public string output;
        }
        private class map2
        {
            public string version;
        }

        private class serverInfo
        {
            public string status = "NaN";
            public string cpu = "0";
            public string ram = "0";
        }

        public static string status = "NaN";
        public static float cpu = 0;
        public static float ram = 0;

        private readonly string apiUrl;
        private serverInfo server;
        private Timer timer;
        private int lastOutput;

        public event OutputEventHandler OnOutputReceived;

        public MinecraftServer(string serverIp, int refreshRate)
        {
            apiUrl = "http://" + serverIp + "/api/v1/";
            server = new serverInfo();
            timer = new Timer(refreshRate);
        }

        public void initialize()
        {
            CheckServer();
            timer.Elapsed += Timer_Elapsed;
            timer.AutoReset = true;
            timer.Enabled = true;
        }

        public void Dispose()
        {
            if (timer.Enabled)
                timer.Dispose();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e) => CheckServer();

        private void CheckServer()
        {
            try
            {
                GetServerStatus();
                status = server.status;
                cpu = float.Parse(server.cpu.Replace(".", ","));
                ram = float.Parse(server.ram.Replace(".", ","));
                if (server.status != "Closed")
                {
                    string[] output = GetServerOutput().Split('\n');
                    if (output.Length != lastOutput)
                    {
                        for (int i = lastOutput; i < output.Length; i++)
                            if (output[i].Length > 0)
                                OnOutputReceived.Invoke(new OutputEventArgs(output[i]));
                        lastOutput = output.Length;
                    }
                }
            }
            catch { }
        }

        private string get(string endPoint, string args = "")
        {
            string url = apiUrl + endPoint;
            if (args.Length > 0) url += "?" + args;

            return sendRequest(url, "GET");
        }

        private string post(string endPoint, string args = "")
        {
            string url = apiUrl + endPoint;
            if (args.Length > 0) url += "?" + args;

            return sendRequest(url, "POST");
        }

        private string sendRequest(string url, string method)
        { 
            string html = string.Empty;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.AutomaticDecompression = DecompressionMethods.GZip;
                request.Method = method;
                request.Timeout = 10000;

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    html = reader.ReadToEnd();
                }
            }
            catch { }

            return html;
        }

        public void Start() => get("server/start");

        public void Stop() => get("server/stop");

        private void GetServerStatus() => server = JsonConvert.DeserializeObject<serverInfo>(get("server/status"));

        public void SendCommand(string command) => post("server/command", "command=" + command);

        public string GetForgeVersion() => JsonConvert.DeserializeObject<map2>(get("server/forgeVersion")).version;

        public string[] GetModList() => JsonConvert.DeserializeObject<string[]>(get("server/mods/list"));

        private string GetServerOutput() => JsonConvert.DeserializeObject<map>(get("server/output")).output;

        public string GetPlayerList() => get("server/players");

        public void DownloadMod(string modsFolder, string mod)
        {
            using (WebClient wc = new WebClient())
            {
                wc.DownloadFile(
                    new Uri(apiUrl + "server/mods/download/" + mod),
                    modsFolder + "\\" + mod
                );
            }
        }
    }
}
