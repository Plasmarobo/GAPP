
using GBASMAssembler;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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

    class AssemblyThread
    {
        protected AssemblerProgress window;
        protected String source;
        public String Source
        {
            get
            {
                return source;
            }
            set
            {
                source = value;
            }
        }
        
        protected Assembler assembler;
        protected List<Byte> result;

        public void Init()
        {
            window = new AssemblerProgress();
            assembler = new Assembler();
            assembler.MessagePrinted += ConsoleOutput;
            assembler.ProgressUpdated += UpdateProgress;
            assembler.AssemblerError += ConsoleOutput;
            window.Show();
        }

        public void AssembleString()
        {
            result = assembler.AssembleString(source);
        }

        public void AssembleFile()
        {
            result = assembler.AssembleFile(source);
        }

        //Matches on progress updated handler
        public void UpdateProgress(int amount, int max)
        {
            if (window != null)
            {
                window.Dispatcher.Invoke(
                    new Action(() => window.UpdateProgress(amount, max)));
            }
        }

        public void ConsoleOutput(String message)
        {
            if (window != null)
            {
                window.Dispatcher.Invoke(
                    new Action(() => window.PrintLine(message))
                    );
            }
        }

        public void CompletionCallback(OnAssemblyComplete callback)
        {
            if (callback != null)
            {
                assembler.AssemblyComplete += callback;
            }
        }

        public Assembler GetAssembler()
        {
            return assembler;
        }
        

    }
    /// <summary>
    /// Interaction logic for GBAssembler.xaml
    /// </summary>
    public partial class GBAssembler : Window
    {
        private AssemblyThread at;
        public GBAssembler()
        {
            InitializeComponent();
        }

        private void CompileAndRun(object sender, RoutedEventArgs e)
        {
          
            at = new AssemblyThread();
            at.Init();
            at.Source = Assembly.Text;
            at.CompletionCallback(ShowAssemblerResult);
            Thread assemblyThread = new Thread(new ThreadStart(at.AssembleString));
            assemblyThread.Start();
           
        }

        public void ShowAssemblerResult(List<Byte> rom)
        {
            Assembler asm = at.GetAssembler();
            this.Dispatcher.Invoke(() => {
                int lines = asm.GetLength();
                Assembly.Clear();
                Execution.Clear();
                for (int i = 0; i < lines; ++i)
                {
                    Assembly.Text += asm.GetLine(i) + "\n";
                    Execution.Text += asm.GetByteString(i) + "\n";
                }
            });
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
