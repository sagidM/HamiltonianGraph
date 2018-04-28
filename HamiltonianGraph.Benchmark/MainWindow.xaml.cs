using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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

namespace HamiltonianGraph.Benchmark
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }


        static Random rand = new Random(42);


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
            for (int i = 0; i < bunchOfWeights.Length; i++)
            {
                bunchOfWeights[i] = GetRandomWeights();
            }
            new RunGraphWindow(bunchOfWeights).Show();
        }


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
