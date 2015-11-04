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
    /// Interaction logic for GBMemory.xaml
    /// </summary>
    public partial class GBMemory : UserControl
    {

        private List<MemoryRow> memory;

        public GBMemory()
        {
            InitializeComponent();
            memory = new List<MemoryRow>(0x10000);
            for(int i = 0; i < 0x10000; ++i)
            {
                MemoryRow r = new MemoryRow();
                r.Location = (ushort)i;
                memory.Add(r);
            }
        }

        public void SetMemory(Dictionary<UInt16,Byte> dump)
        {
           foreach(KeyValuePair<UInt16, Byte> kvp in dump)
           {
               memory[kvp.Key].Current = kvp.Value;
           }
        }

        public List<MemoryRow> Filter(String filter)
        {
            
            List<MemoryRow> result = new List<MemoryRow>();
            if (filter != null && filter.Length > 0)
            {
                HashSet<UInt16> indices = new HashSet<ushort>();
                String[] selectors = filter.Split(',');

                foreach (String sel in selectors)
                {
                    //Match 0x#, #, and -
                    if (sel.Contains("-"))
                    {
                        String[] substr = sel.Split('-');
                        UInt16 left = Convert.ToUInt16(substr[0]);
                        UInt16 right = Convert.ToUInt16(substr[1]);
                        for (int i = left; i <= right; ++i)
                        {
                            indices.Add((ushort)i);
                        }
                    }
                    else
                    {
                        indices.Add(Convert.ToUInt16(sel));
                    }
                }
                foreach (UInt16 i in indices)
                {
                    result.Add(memory[i]);
                }
            }
            else
            {
                result = memory;
            }
            return result;
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            MemoryContents.Children.Clear();
            foreach (MemoryRow row in Filter(SearchBox.Text))
            {
                MemoryContents.Children.Add(row);
            }
        }

    }
}
