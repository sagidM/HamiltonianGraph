using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
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

namespace HamiltonianGraph.Benchmark
{
    /// <summary>
    /// Interaction logic for RunGraphWindow.xaml
    /// </summary>
    public partial class RunGraphWindow : Window, INotifyPropertyChanged
    {
        int?[][,] _weights;
        // (iterate<foundCycle>, start, end<elapsed>)
        Action<Action<int[]>, Action, Action<TimeSpan>> findCyclesByBaB;
        Action<Action<int[]>, Action, Action<TimeSpan>> findCyclesByLC;
        public string BaBOutput { get => _baBOutput; set => SetField(ref _baBOutput, value); }
        public string LCOutput { get => _lcOutput; set => SetField(ref _lcOutput, value); }
        public string BaBResult { get => _baBResult; set => SetField(ref _baBResult, value); }
        public string LCResult { get => _lcResult; set => SetField(ref _lcResult, value); }
        public RunGraphWindow(int?[][,] bunchOfWeights)
        {
            InitializeComponent();
            _weights = bunchOfWeights;
            DataContext = this;

            findCyclesByBaB = (onCycleFound, onStart, onEnd) =>
            {
                var sw = new Stopwatch();
                sw.Start();
                for (int i = 0; i < bunchOfWeights.Length; i++)
                {
                    var cycle = new BranchAndBound(bunchOfWeights[i])
                        .GetShortestHamiltonianCycle();
                    if (i == 0)
                        Dispatcher.Invoke(onStart);
                    Dispatcher.BeginInvoke(onCycleFound, cycle);
                }
                Dispatcher.BeginInvoke(onEnd, sw.Elapsed);
            };

            findCyclesByLC = (onCycleFound, onStart, onEnd) =>
            {
                var sw = new Stopwatch();
                sw.Start();
                for (int i = 0; i < bunchOfWeights.Length; i++)
                {
                    var cycle = new LatinComposition(bunchOfWeights[i])
                        .GetShortestHamiltonianCycle();
                    if (i == 0)
                        Dispatcher.Invoke(onStart);
                    onCycleFound(cycle);
                }
                Dispatcher.BeginInvoke(onEnd, sw.Elapsed);
            };

            Closed += (s, e) =>
            {
                if (babThread != null && babThread.IsAlive)
                    babThread.Abort();
                if (lcThread != null && lcThread.IsAlive)
                    lcThread.Abort();
            };
        }


        private void SetField<T>(ref T field, T value, [CallerMemberName]string propertyName = null)
        {
            if (Equals(field, value)) return;
            field = value;
            OnPropertyChanged(propertyName);
        }
        void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;

        Thread babThread;
        Thread lcThread;
        private string _baBOutput;
        private string _lcOutput;
        private string _baBResult;
        private string _lcResult;

        private void RunBaB(object sender, RoutedEventArgs e)
        {
            if (babThread != null && babThread.IsAlive)
                return;
            BaBOutput = "Ждите...";
            babThread = new Thread(() =>
            {
                findCyclesByBaB(cycle =>
                {
                    var path = cycle == null ? "[Путь не найден]" : string.Join("->", cycle);
                    BaBOutput += path + "\n";
                }, () => BaBOutput = BaBResult = "", elapsed => BaBResult = elapsed.ToString());
            });
            babThread.IsBackground = true;
            babThread.Start();
        }
        private void RunLC(object sender, RoutedEventArgs e)
        {
            if (lcThread != null && lcThread.IsAlive)
                return;
            LCOutput = "Ждите...";
            lcThread = new Thread(() =>
            {
                findCyclesByLC(cycle =>
                {
                    var path = cycle == null ? "[Путь не найден]" : string.Join("->", cycle);
                    LCOutput += path + "\n";
                }, () => LCOutput = LCResult = "", elapsed => LCResult = elapsed.ToString());
            });
            lcThread.IsBackground = true;
            lcThread.Start();
        }
    }
}
