using GAPPDebugger.Controls;
using GBLibWrapper;
using Microsoft.Win32;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace GAPPDebugger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private GBLib emu;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Edit(object sender, RoutedEventArgs e)
        {
            GBAssembler asm_win = new GBAssembler();
            
            asm_win.Show();
            asm_win.Focus();
        }

        private void Run(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.DefaultExt = ".gb";
            dialog.Filter = "Gameboy ROM (.gb)|*.gb";
            bool? res = dialog.ShowDialog();
            if (res == true)
            {
                emu = new GBLib();
                gbdisplay.GBemu = emu;
                emu.onDisplayUpdate += new OnDisplayUpdate(this.gbdisplay.OnDisplayUpdate);
                emu.LoadRom(dialog.FileName);
                emu.Start();
                DispatcherTimer t = new DispatcherTimer();
                Int64 ticks_per_mhz = 10;
                t.Interval = new TimeSpan(ticks_per_mhz);
                t.Tick += HandleTick;
                t.Start();
            }
        }

        private void HandleTick(object sender, object e)
        {
            //Each tick is 100 ns
            //This should fire once every 10 ticks
            emu.Step();
        }

        private void Step(object sender, RoutedEventArgs e)
        {
            emu.DebugStep();
        }

    }
}
