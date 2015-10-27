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

namespace GAPPDebugger.Controls
{
    /// <summary>
    /// Interaction logic for MemoryRow.xaml
    /// </summary>
    public partial class MemoryRow : UserControl
    {
        private UInt16 _location;
        public UInt16 Location
        {
            set
            {
                _location = value;
                this.LocationText.Content = String.Format("0x{0:XD4}", _location);
            }
            get
            {
                return _location;
            }
        }
        private byte _current;
        public byte Current {
            set { 
                previous = _current; 
                _current = value; 
                if(previous != _current)
                {
                    changed = true;
                    this.LocationText.Foreground = Brushes.Red;
                    
                }
                this.CurrentText.Content = String.Format("{0} (0x{0:X})", _current);
                this.PreviousText.Content = String.Format("{0} (0x{0:X})", previous);
            } 
            get { return _current; } }

        public byte previous;
        public bool changed;
        

        public MemoryRow()
        {
            InitializeComponent();
        }


    }
}
