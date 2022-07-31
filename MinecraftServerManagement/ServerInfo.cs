using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace MinecraftServerManagement
{
    class ServerInfo
    {
        internal class BackUp
        {
            public static bool Enabled;
            public static string Folder;
            public static int Interval;
            public static int MaxBackUps;
        }
        public static string ServerFolder = Environment.CurrentDirectory;
        public static string Arguments;
        public static List<string> Players;
        public static DateTime Apagado;
        public static DateTime Inicio;

        public static void ReadServerInfo()
        {
            try
            {
                string[] data = File.ReadAllLines("MinecarftServerManagement.data");
                int d = 0;
                Arguments = data[d++].Split('=')[1];
                BackUp.Enabled = data[d++].Split('=')[1] == "true";
                BackUp.Folder = data[d++].Split('=')[1];
                BackUp.Interval = int.Parse(data[d++].Split('=')[1]);
                BackUp.MaxBackUps = int.Parse(data[d++].Split('=')[1]);
                d++;
                Players = new List<string>();
                for (int i = d; i < data.Length; i++)
                    Players.Add(data[i]);
            }
            catch
            {
                Arguments = "-Xmx5G -Xms6G";
                BackUp.Enabled = true;
                BackUp.Folder = ServerFolder + "\\Backups";
                BackUp.Interval = 60;
                BackUp.MaxBackUps = 10;
                Apagado = DateTime.MinValue;
                Inicio = DateTime.MinValue;
                Players = new List<string>();
            }
        }

        public static void WriteServerInfo()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Arguments=" + Arguments);
            sb.AppendLine("Enable_Backups=" + BackUp.Enabled.ToString().ToLower());
            sb.AppendLine("Backup_Folder=" + BackUp.Folder);
            sb.AppendLine("Backup_Interval=" + BackUp.Interval);
            sb.AppendLine("Max_Backups=" + BackUp.MaxBackUps);
            sb.AppendLine("###Players");
            foreach (string player in Players)
                sb.AppendLine(player);
            File.WriteAllText(ServerFolder + @"\MinecarftServerManagement.data", sb.ToString());
        }

        private static string GetFormatedTimeStamp()
        {
            DateTime current = DateTime.Now;
            string output = current.Year.ToString() + "-" + current.Month.ToString() + "-" + current.Day.ToString();
            output += "_" + current.Hour.ToString() + "-" + current.Minute.ToString() + "-" + current.Second.ToString();
            return output;
        }

        public static string GetJarFileName()
        {
            try
            {
                List<string> FolderFiles = Directory.GetFiles(ServerFolder, "*.jar", SearchOption.TopDirectoryOnly).ToList().ConvertAll<string>(x => new FileInfo(x).Name);
                return (from Files in FolderFiles
                        where (Files.ToLower().Contains("forge") ||
                        Files.ToLower().Contains("server") ||
                        Files.ToLower().Contains("bukkit") ||
                        Files.ToLower().Contains("tekkit") ||
                        Files.ToLower().Contains("minecraft"))
                        select new FileInfo(Files).Name).First();
            }
            catch
            {
                throw new FileNotFoundException("No ha sido posible encontrar el archivo \".jar\" del servidor en la carpeta actual. Compruebe que el programa esté en la misma carpeta que el \".jar\" y que su nombre sea correcto (ej. Tekkit.jar, craftbukkit.jar, minecraft_server.jar, forge.jar).");
            }
        }
    }
}
