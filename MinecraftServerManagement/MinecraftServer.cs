using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
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
        private Thread check;
        private int lastOutputLenght;

        public event OutputEventHandler OnOutputReceived;

        public MinecraftServer(string serverIp, int refreshRate)
        {
            apiUrl = "http://" + serverIp + "/api/v1/";
            server = new serverInfo();
            check = new Thread(() => CheckServer(refreshRate));
            lastOutputLenght = 0;
        }

        public void initialize() => check.Start();

        public void Dispose() => check?.Abort();

        private void CheckServer(int refreshRate)
        {
            while (true)
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
                        int currentLenght = lastOutputLenght;
                        lastOutputLenght = output.Length;

                        if (currentLenght != output.Length)
                            OnOutputReceived.Invoke(new OutputEventArgs(output.Skip(currentLenght).ToArray()));
                    }
                }
                catch { }
                Thread.Sleep(refreshRate);
            }
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

        public void Start() {
            lastOutputLenght = 0;
            get("server/start");
        }

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
