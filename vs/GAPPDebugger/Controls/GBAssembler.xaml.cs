using Microsoft.Win32;
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

namespace GAPPDebugger.Controls
{
    /// <summary>
    /// Interaction logic for GBAssembler.xaml
    /// </summary>
    public partial class GBAssembler : Window
    {
        Assembler assembler;
        public GBAssembler()
        {
            InitializeComponent();
            assembler = new Assembler();
        }

        private void CompileAndRun(object sender, RoutedEventArgs e)
        {
            assembler.AssembleString(Assembly.Text);
        }

        private void SaveASM(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.FileName = "Gameboy";
            dialog.DefaultExt = ".asm";
            dialog.Filter = "Assembly (.asm)|*.asm";
            bool? res = dialog.ShowDialog();
            if(res == true)
            {
                File.WriteAllText(dialog.FileName, Assembly.Text.ToString());
            }
        }

        private void LoadASM(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.FileName = "";
            dialog.DefaultExt = ".asm";
            dialog.Filter = "Assembly (.asm)|*.asm|Text Files (.txt)|*.txt";
            bool? res = dialog.ShowDialog();
            if(res == true)
            {
                StreamReader f = new StreamReader(dialog.OpenFile());
                Assembly.Text = f.ReadToEnd();
                f.Close();
            }
        }

      
    }
}
