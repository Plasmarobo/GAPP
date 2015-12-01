using System;
using System.Collections.Generic;
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

namespace GAPPDebugger.Controls
{
    /// <summary>
    /// Interaction logic for AssemblerProgress.xaml
    /// </summary>
    public partial class AssemblerProgress : Window
    {
        public AssemblerProgress()
        {
            InitializeComponent();
        }

        public void UpdateProgress(int amount, int max)
        {
            Progress.Maximum = max;
            Progress.Value = amount;
        }

        public void PrintLine(String message)
        {
            this.ConsoleWin.Text += message + "\n";
        }
    }
}
