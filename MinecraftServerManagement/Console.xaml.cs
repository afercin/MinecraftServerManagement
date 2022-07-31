using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using static MinecraftServerManagement.OutPutEvent;

namespace MinecraftServerManagement
{
    /// <summary>
    /// Lógica de interacción para Console.xaml
    /// </summary>
    public partial class Console : UserControl
    {
        public event OutputEventHandler OnConsoleCommand;
        public Console()
        {
            InitializeComponent();
        }

        delegate void ParametrizedMethodInvoker5(string text);
        public void AppendText(string text)
        {
            if (!Dispatcher.CheckAccess())
                Dispatcher.Invoke(new ParametrizedMethodInvoker5(AppendText), text);
            else
            {
                TextRange tr = new TextRange(console.Document.ContentEnd, console.Document.ContentEnd);
                SolidColorBrush color = Brushes.White;

                if (text.Contains("[Internal INFO]"))
                    color = new BrushConverter().ConvertFrom("#3dbcf1") as SolidColorBrush;
                else if (text.Contains("INFO"))
                    color = Brushes.DarkCyan;
                else if (text.Contains("WARN"))
                    color = Brushes.Yellow;
                else if (text.Contains("ERROR") || text.Contains("Exception") || text.EndsWith("}") || (text.Contains("...") && text.Contains("more")))
                    color = Brushes.Red;

                tr.Text = text + "\r\n";
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, color);

                console.ScrollToEnd();
            }
        }

        public void Clear() => console.Document.Blocks.Clear();

        private void Command_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !string.IsNullOrEmpty(command.Text))
            {
                OnConsoleCommand.Invoke(new OutputEventArgs(command.Text));
                command.Clear();
            }
        }
    }
}
