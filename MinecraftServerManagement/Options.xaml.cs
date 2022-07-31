using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MinecraftServerManagement
{
    /// <summary>
    /// Lógica de interacción para Options.xaml
    /// </summary>
    public partial class Options : Window
    {
        private string[] data;
        private string filePath;
        private bool modificado;

        public Options()
        {
            InitializeComponent();

            LevelType.Items.Add("DEFAULT");
            LevelType.Items.Add("FLAT");
            LevelType.Items.Add("LARGEBIOMES");
            LevelType.Items.Add("AMPLIFIED");
            LevelType.Items.Add("BIOMESOP");

            Difficulty.Items.Add("Pacífico");
            Difficulty.Items.Add("Fácil");
            Difficulty.Items.Add("Normal");
            Difficulty.Items.Add("Difícil");
        }

        private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                try
                {
                    data = File.ReadAllLines(filePath);
                    string[] attribute;
                    for (int i = 2; i < data.Length; i++)
                    {
                        attribute = data[i].Split('=');
                        switch (attribute[0])
                        {
                            case "server-ip": IP.Text = attribute[1]; break;
                            case "server-port": Port.Text = attribute[1]; break;
                            case "max-players": MaxPlayers.Value = int.Parse(attribute[1]); break;
                            case "view-distance": ViewDistance.Value = int.Parse(attribute[1]); break;
                            case "level-type": LevelType.Text = attribute[1].ToUpper(); break;
                            case "difficulty": Difficulty.SelectedIndex = int.Parse(attribute[1]); break;
                            case "allow-flight": Flight.IsChecked = attribute[1] == "true"; break;
                            case "spawn-animals": Animals.IsChecked = attribute[1] == "true"; break;
                            case "allow-nether": Nether.IsChecked = attribute[1] == "true"; break;
                            case "spawn-monsters": Monsters.IsChecked = attribute[1] == "true"; break;
                            case "pvp": PVP.IsChecked = attribute[1] == "true"; break;
                            case "spawn-npcs": NPC.IsChecked = attribute[1] == "true"; break;
                            case "motd": MOTD.Text = attribute[1]; break;
                        }
                    }
                }
                catch { data = null; }
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();

        private void CloseButton_Click(object sender, RoutedEventArgs e) => Hide();

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (data != null)
            {
                modificado = true;
                string[] attribute;
                for (int i = 2; i < data.Length; i++)
                {
                    attribute = data[i].Split('=');
                    switch (attribute[0])
                    {
                        case "server-ip": data[i] = attribute[0] + "=" + IP.Text; break;
                        case "server-port": data[i] = attribute[0] + "=" + Port.Text; break;
                        case "max-players": data[i] = attribute[0] + "=" + MaxPlayers.Value; break;
                        case "view-distance": data[i] = attribute[0] + "=" + ViewDistance.Value; break;
                        case "level-type": data[i] = attribute[0] + "=" + LevelType.Text.ToLower(); break;
                        case "difficulty": data[i] = attribute[0] + "=" + Difficulty.SelectedIndex; break;
                        case "allow-flight": data[i] = attribute[0] + "=" + Flight.IsChecked.ToString().ToLower(); break;
                        case "spawn-animals": data[i] = attribute[0] + "=" + Animals.IsChecked.ToString().ToLower(); break;
                        case "allow-nether": data[i] = attribute[0] + "=" + Nether.IsChecked.ToString().ToLower().ToString(); break;
                        case "spawn-monsters": data[i] = attribute[0] + "=" + Monsters.IsChecked.ToString().ToLower(); break;
                        case "pvp": data[i] = attribute[0] + "=" + PVP.IsChecked.ToString().ToLower(); break;
                        case "spawn-npcs": data[i] = attribute[0] + "=" + NPC.IsChecked.ToString().ToLower(); break;
                        case "motd": data[i] = attribute[0] + "=" + MOTD.Text; break;
                    }
                }
                File.WriteAllLines(filePath, data);
            }
            Hide();
        }

        public bool Show(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("[Internal INFO] No ha sido posible encontrar el fichero \"server.properties\" asegurese que el programa esté en la ruta correcta.");
            this.filePath = filePath;
            modificado = false;
            Show();
            return modificado;
        }
    }
}
