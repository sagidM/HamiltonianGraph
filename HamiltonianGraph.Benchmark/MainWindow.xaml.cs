using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HamiltonianGraph.Benchmark
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private const string SeedPath = "seed.txt";
        Random rand;
        int?[][,] _bunchOfWeights;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            if (File.Exists(SeedPath) && int.TryParse(File.ReadAllText(SeedPath), out var seed))
            {
                rand = new Random(seed);
            }
            else
            {
                rand = new Random();
            }
        }


        #region Ranges

        private int _vertexMinumum = 2;
        public int VertexMinimum
        {
            get => _vertexMinumum;
            set
            {
                if (value < 1) return;
                if (value > VertexMaximum)
                    SetField(ref _vertexMaximum, value, nameof(VertexMaximum));
                SetField(ref _vertexMinumum, value);
            }
        }

        private int _vertexMaximum = 10;
        public int VertexMaximum
        {
            get => _vertexMaximum;
            set
            {
                if (value < VertexMinimum)
                    SetField(ref _vertexMinumum, value, nameof(VertexMinimum));
                SetField(ref _vertexMaximum, value);
            }
        }

        private int _weightMinumum = 1;
        public int WeightMinimum
        {
            get => _weightMinumum;
            set
            {
                if (value < 1) return;
                if (value > WeightMaximum)
                    SetField(ref _weightMaximum, value, nameof(WeightMaximum));
                SetField(ref _weightMinumum, value);
            }
        }

        private int _weightMaximum = 1000;
        public int WeightMaximum
        {
            get => _weightMaximum;
            set
            {
                if (value < WeightMinimum)
                    SetField(ref _weightMinumum, value, nameof(WeightMinimum));
                SetField(ref _weightMaximum, value);
            }
        }


        private double _density = 1;
        public double Density
        {
            get => _density;
            set
            {
                //if (value > 1 || value < 0) return;
                SetField(ref _density, value);
            }
        }

        private int _graphCount = 1;
        public int GraphCount
        {
            get => _graphCount;
            set
            {
                if (value < 1 || value > 1000) return;
                SetField(ref _graphCount, value);
            }
        }


        #endregion

        private string _adjacencyMatrixText;
        public string AdjacencyMatrixText
        {
            get => _adjacencyMatrixText;
            set => SetField(ref _adjacencyMatrixText, value);
        }

        private int?[,] GetRandomWeights()
        {
            int n = rand.Next(VertexMinimum, VertexMaximum + 1);
            int?[,] weights = new int?[n, n];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (i != j && rand.NextDouble() < Density)
                        weights[i, j] = rand.Next(WeightMinimum, WeightMaximum + 1);
                }
            }
            return weights;
        }

        private void GenerateGraphs(object sender, RoutedEventArgs e)
        {
            var bunchOfWeights = new int?[_graphCount][,];
            var sb = new StringBuilder(_graphCount * 100);
            int lastIndex = bunchOfWeights.Length - 1;
            for (int i = 0; i < lastIndex; i++)
            {
                bunchOfWeights[i] = GetRandomWeights();
                sb.AppendLine(Utils.GraphUtil.ToMatrixFormat(bunchOfWeights[i]));
            }
            // Not to append a new line
            bunchOfWeights[lastIndex] = GetRandomWeights();
            sb.Append(Utils.GraphUtil.ToMatrixFormat(bunchOfWeights[lastIndex]));

            AdjacencyMatrixText = sb.ToString();
            _bunchOfWeights = bunchOfWeights;
            InitializeThreads();
        }

        #region Run graphs

        // (iterate<foundCycle>, start, end<elapsed>)
        Action<Action<int[]>, Action, Action<TimeSpan>> findCyclesByBaB;
        Action<Action<int[]>, Action, Action<TimeSpan>> findCyclesByLC;
        public string BaBOutput { get => _baBOutput; set => SetField(ref _baBOutput, value); }
        public string LCOutput { get => _lcOutput; set => SetField(ref _lcOutput, value); }
        public string BaBResult { get => _baBResult; set => SetField(ref _baBResult, value); }
        public string LCResult { get => _lcResult; set => SetField(ref _lcResult, value); }

        Thread babThread;
        Thread lcThread;
        private string _baBOutput;
        private string _lcOutput;
        private string _baBResult;
        private string _lcResult;
        private const string InputGraph = "input_graph.txt";
        private const string ResultPath = "result_path.txt";

        private void InitializeThreads()
        {
            if (babThread != null && babThread.IsAlive)
                babThread.Abort();
            if (lcThread != null && lcThread.IsAlive)
                lcThread.Abort();

            findCyclesByBaB = (onCycleFound, onStart, onEnd) =>
            {
                var sw = new Stopwatch();
                sw.Start();
                int[] cycle = null;
                for (int i = 0; i < _bunchOfWeights.Length; i++)
                {
                    cycle = new BranchAndBound(_bunchOfWeights[i])
                        .GetShortestHamiltonianCycle();
                    if (i == 0)
                        Dispatcher.Invoke(onStart);
                    Dispatcher.BeginInvoke(onCycleFound, cycle);
                }
                Dispatcher.BeginInvoke(onEnd, sw.Elapsed);
                if (_bunchOfWeights.Length == 1)
                {
                    var graph = Utils.GraphUtil.ToMatrixFormat(_bunchOfWeights[0]);
                    File.WriteAllText(InputGraph, graph);
                    File.WriteAllText(ResultPath, string.Join("-", cycle));
                }
            };

            findCyclesByLC = (onCycleFound, onStart, onEnd) =>
            {
                var sw = new Stopwatch();
                sw.Start();
                for (int i = 0; i < _bunchOfWeights.Length; i++)
                {
                    var cycle = new LatinComposition(_bunchOfWeights[i])
                        .GetShortestHamiltonianCycle();
                    if (i == 0)
                        Dispatcher.Invoke(onStart);
                    onCycleFound(cycle);
                }
                Dispatcher.BeginInvoke(onEnd, sw.Elapsed);
            };
        }

        private void RunBaB(object sender, RoutedEventArgs e)
        {
            if ((babThread != null && babThread.IsAlive) || findCyclesByBaB == null)
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
            if ((lcThread != null && lcThread.IsAlive) || findCyclesByLC == null)
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
        #endregion


        private void SetField<T>(ref T field, T value, [CallerMemberName]string propertyName = null)
        {
            if (Equals(field, value)) return;
            Console.WriteLine($"{propertyName} ({field}) = {value}");
            field = value;
            OnPropertyChanged(propertyName);
        }
        void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
