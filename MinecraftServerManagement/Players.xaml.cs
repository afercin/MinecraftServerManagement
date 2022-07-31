using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static MinecraftServerManagement.OutPutEvent;

namespace MinecraftServerManagement
{
    /// <summary>
    /// Lógica de interacción para Players.xaml
    /// </summary>
    public partial class Players : UserControl
    {
        private string action;
        private List<string> connectedList;

        public event OutputEventHandler OnPlayerSelect;
        public int OnlinePlayers
        {
            get => connectedList.Count;
        }

        public Players()
        {
            InitializeComponent();
            action = string.Empty;
            connectedList = new List<string>();
            ServerInfo.Players = new List<string>();
            ConnectedPlayers.ItemsSource = connectedList;
            AllPlayers.ItemsSource = ServerInfo.Players;
            AddPlayer("Pepe joined");
            AddPlayer("Pepe joined");
            AddPlayer("Pepe joined");
            AddPlayer("Andres joined");
        }

        public void AddPlayer(string text)
        {
            string username = GetUserName(text, "joined");
            if (!connectedList.Contains(username))
                connectedList.Add(username);
            if (!ServerInfo.Players.Contains(username))
                ServerInfo.Players.Add(username);
        }

        public void DeletePlayer(string text)
        {
            string username = GetUserName(text, "left");
            if (connectedList.Contains(username))
                connectedList.Remove(username);
        }

        public string GetUserName(string text, string mode)
        {
            string[] words = text.Split(':');
            return words[words.Length - 1].Split(new[] { mode }, StringSplitOptions.None)[0].Trim().Split()[0];
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            RadioButton currentButton = sender as RadioButton;
            if (action.Length > 0 && action == currentButton.Name)
            {
                action = string.Empty;
                currentButton.IsChecked = false;
            }
            else
                action = currentButton.Name;
        }

        private void DataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            DataGrid grid = sender as DataGrid;
            if (!string.IsNullOrEmpty(action) && grid.SelectedItem != null)
                OnPlayerSelect.Invoke(new OutputEventArgs(new string[] { action + " " + grid.CurrentItem.ToString() }));
        }

        private void DataGrid_MouseUp(object sender, MouseButtonEventArgs e) => (sender as DataGrid).SelectedItem = null;
    }
}
