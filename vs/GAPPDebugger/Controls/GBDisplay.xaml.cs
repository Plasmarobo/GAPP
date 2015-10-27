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
using System.Windows.Navigation;
using System.Windows.Shapes;
using GBLibWrapper;

namespace GAPPDebugger.Controls
{
    /// <summary>
    /// Interaction logic for GBDisplay.xaml
    /// </summary>
    public partial class GBDisplay : UserControl
    {

        public GBLib gbemu;

        public GBDisplay(GBLib emu)
        {
            InitializeComponent();
            gbemu = emu;
            gbemu.onDisplayUpdate += OnDisplayUpdate;
        }

        public void OnDisplayUpdate(byte[] rgba_data, uint width, uint height)
        {
            try
            {
                MemoryStream stream = new MemoryStream(rgba_data);
                BitmapImage source = new BitmapImage();
                source.BeginInit();
                source.StreamSource = stream;
                source.DecodePixelWidth = 160;
                source.DecodePixelHeight = 144;
                source.EndInit();
                this.DisplayFrame.Source = source;
            }
            catch(Exception ex)
            {
                Console.Write(ex.Message);
            }
        }

        private void DisplayFrame_KeyDown(object sender, KeyEventArgs e)
        {
            gbemu.Keydown((uint)e.Key);
        }

        private void DisplayFrame_KeyUp(object sender, KeyEventArgs e)
        {
            gbemu.Keyup((uint)e.Key);
        }
    }
}
