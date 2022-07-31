using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows.Forms;
using OxyPlot;
using OxyPlot.Legends;
using OxyPlot.Series;

namespace MinecraftServerManagement
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Main

        private MinecraftServer server;
        private Dictionary<string, string> config;
        private Thread firstCheck;
        private double _xValue = 1;
        private DispatcherTimer timer;

        public PlotModel MyPlotModel { get; private set; }

        public MainWindow()
        {
            config = new Dictionary<string, string>();
            foreach (var row in File.ReadAllLines(AppDomain.CurrentDomain.BaseDirectory + @"\launcher.properties"))
                config.Add(row.Split('=')[0], string.Join("=", row.Split('=').Skip(1).ToArray()));

            MyPlotModel = new PlotModel {
                Title = "Server status: NaN",
                TextColor = OxyColors.White
            };
            MyPlotModel.Series.Add(new LineSeries { Title = "CPU%" });
            MyPlotModel.Series.Add(new LineSeries { Title = "RAM%" });
            MyPlotModel.Legends.Add(new Legend()
            {
                LegendTitle = "Usage",
                LegendPosition = LegendPosition.RightMiddle,
            });

            server = new MinecraftServer(config["SERVER_IP"], int.Parse(config["REFRESH_RATE"]) * 1000);
            server.OnOutputReceived += Server_OnOutputReceived;

            timer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 1) };
            timer.Tick += (o, e) => {
                Dispatcher.CurrentDispatcher.Invoke(() =>
                {
                    if (MinecraftServer.status != "NaN" )
                    {
                        MyPlotModel.Title = "Server status: " + MinecraftServer.status;
                        (MyPlotModel.Series[0] as LineSeries).Points.Add(new DataPoint(_xValue, MinecraftServer.cpu));
                        (MyPlotModel.Series[1] as LineSeries).Points.Add(new DataPoint(_xValue, MinecraftServer.ram));

                        if ((MyPlotModel.Series[0] as LineSeries).Points.Count > 10)
                        {
                            (MyPlotModel.Series[0] as LineSeries).Points.RemoveAt(0);
                            (MyPlotModel.Series[1] as LineSeries).Points.RemoveAt(0);
                        }

                        MyPlotModel.InvalidatePlot(true);
                        _xValue++;

                        Dispatcher.Invoke(() =>
                        {
                            if (InitButton != null)
                                InitButton.Visibility = MinecraftServer.status == "Closed" ? Visibility.Visible : Visibility.Hidden;
                            if (ShutdownButton != null)
                                ShutdownButton.Visibility = MinecraftServer.status == "Ready" ? Visibility.Visible : Visibility.Hidden;
                        });
                    }
                });                
            };
            timer.Start();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        { 
            firstCheck = new Thread(() => CheckMods());
            firstCheck.Start();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (firstCheck.IsAlive)
                firstCheck.Abort();
            timer.Stop();
            server.Dispose();
            System.Windows.Application.Current.Shutdown();
        }

        #endregion

        #region Buttons

        private void MinimizeButton_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

        private void MaximizeButton_Click(object sender, RoutedEventArgs e) => WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;

        private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

        private void InitButton_Click(object sender, RoutedEventArgs e)
        {
            switch (MinecraftServer.status)
            {
                case "Ready":
                    Console.AppendText("[Internal INFO] El servidor ya ha sido iniciado.");
                    server.Stop();
                    break;
                case "Starting":
                    Console.AppendText("[Internal INFO] El servidor ya está siendo iniciado.");
                    break;
                case "Closing":
                    Console.AppendText("[Internal INFO] No es posible iniciar el servidor mientras se está apagando.");
                    break;
                case "Closed":
                    Console.Clear();
                    Console.AppendText("[Internal INFO] Iniciando el servidor...\r\n");
                    server.Start();
                    break;
            }
        }

        private void ShutdownButton_Click(object sender, RoutedEventArgs e)
        {
            switch (MinecraftServer.status)
            {
                case "Ready":
                    Console.AppendText("[Internal INFO] Apagando el servidor...\r\n");
                    server.Stop();
                    break;
                case "Starting":
                    Console.AppendText("[Internal INFO] No es posible apagar el servidor mientras se inicia.");
                    break;
                case "Closing":
                    Console.AppendText("[Internal INFO] El servidor ya está siendo apagado.");
                    break;
                case "Closed":
                    Console.AppendText("[Internal INFO] El servidor ya ha sido apagado.");
                    break;
            }
        }

        private void ForgeButton_Click(object sender, RoutedEventArgs e)
        {
            string forgeVersion = server.GetForgeVersion();
            System.Diagnostics.Process.Start("https://maven.minecraftforge.net/net/minecraftforge/forge/" + forgeVersion + "/forge-" + forgeVersion + "-installer.jar");
        }

        #endregion

        #region Comunication

        private void CheckMods()
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    Console.AppendText("[Internal INFO] Comprobando lista de mods...");
                    if (config["MODS_FOLDER"].Length == 0)
                    {
                        using (var fbd = new FolderBrowserDialog())
                        {
                            fbd.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                            DialogResult result = fbd.ShowDialog();

                            if (result == (DialogResult)1 && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                            {
                                config["MODS_FOLDER"] = fbd.SelectedPath;
                                string lines = 
                                    "SERVER_IP=" + config["SERVER_IP"] + "\n" +
                                    "REFRESH_RATE=" + config["REFRESH_RATE"] + "\n" +
                                    "MODS_FOLDER=" + config["MODS_FOLDER"];
                                File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + @"\launcher.properties", lines);
                            }
                        }
                    }
                });
                IEnumerable<string> localMods = from modFilePath in Directory.GetFiles(config["MODS_FOLDER"]) select Path.GetFileName(modFilePath);
                string[] remoteMods = server.GetModList();
                Array.Sort(remoteMods);
                foreach (string mod in remoteMods)
                {
                    if (!localMods.Contains(mod))
                    {
                        Console.AppendText("[Internal INFO] Descargando " + mod + "...");
                        server.DownloadMod(config["MODS_FOLDER"], mod);
                        Console.AppendText("[Internal INFO] Descargando " + mod + "... [DONE]");
                    }
                }
                Console.AppendText("[Internal INFO] Recibiendo información del server...");
                server.initialize();
            }
            catch (Exception e)
            {
                Console.AppendText("[Internal INFO] Error al comprobar la lista de mods: " + e.Message);
            }
        }

        private void Server_OnOutputReceived(OutPutEvent.OutputEventArgs e)
        {
            foreach (string line in e.Lines)
                if (line.Length > 0)
                    Console.AppendText(line.Replace("\r", ""));
        }

        private void Controls_OnCommandSend(OutPutEvent.CommandEventArgs e)
        {
            if (MinecraftServer.status == "Ready")
            {
                Console.AppendText("[Internal INFO] Enviando el siguiente comando al servidor: " + e.Output);
                server.SendCommand(e.Output);
            }
            else
                Console.AppendText("[Internal INFO] No se pueden enviar comandos mientras el servidor no esté activo.");
        }

        #endregion
    }
}
