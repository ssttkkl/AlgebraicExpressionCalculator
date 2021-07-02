using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MathematicalExpressionCalculator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private struct SymbolConstraint
        {
            public string name;
            public string value;

            public override string ToString()
            {
                return $"{name} = {value}";
            }
        }

        private ObservableCollection<SymbolConstraint> constraints = new ObservableCollection<SymbolConstraint>();

        private readonly BackgroundWorker bw = new BackgroundWorker();
        public MainWindow()
        {
            InitializeComponent();
            constraintListBox.ItemsSource = constraints;

            bw.DoWork += Bw_DoWork;
            bw.RunWorkerCompleted += Bw_RunWorkerCompleted;
            bw.WorkerSupportsCancellation = true;
        }

        private void Bw_DoWork(object sender, DoWorkEventArgs e)
        {
            (string input, SymbolConstraint[] constraints) arg = ((string, SymbolConstraint[]))e.Argument;

            ExpressionContext context = new ExpressionContext();
            foreach (var item in arg.constraints)
            {
                context.SetSymbolAssignment(context.Symbol(item.name),
                    ExpressionParser.Parse(item.value, context));
            }
            context.AnalyseConstraints();

            IExpression ep = ExpressionParser.Parse(arg.input, context);
            ep = ep.WithAssignment().Simplify();

            string output = ep.ToString(), formula = ep.ToLaTeX();
            e.Result = (output, formula);
        }

        private void Bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                OutputTextBlock.Text = "出错：" + e.Error.Message;
            }
            else if (e.Result != null)
            {
                (string output, string formula) result = ((string, string))e.Result;
                OutputTextBlock.Text = result.output;
                OutputFormula.Formula = result.formula;
            }
            GoButton.IsEnabled = true;
        }

        private void GoButton_Click(object sender, RoutedEventArgs e)
        {
            OutputTextBlock.Text = "计算中……";
            GoButton.IsEnabled = false;
            bw.RunWorkerAsync((InputTextBox.Text, constraints.ToArray()));
        }

        private void AddConstraintBtn_Click(object sender, RoutedEventArgs e)
        {
            EditAssignmentWindow w = new EditAssignmentWindow();
            if (w.ShowDialog() ?? false)
            {
                string name = w.SymbolName;
                string value = w.SymbolValue;
                if (constraints.All(it => it.name != name))
                {
                    constraints.Add(new SymbolConstraint { name = name, value = value });
                }
                else
                {
                    MessageBox.Show($"已经存在变量{name}的赋值",
                        "错误",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        private void EditConstraintBtn_Click(object sender, RoutedEventArgs e)
        {
            int idx = constraintListBox.SelectedIndex;
            if (idx != -1)
            {
                var item = constraints[idx];
                EditAssignmentWindow w = new EditAssignmentWindow(item.name, item.value);
                if (w.ShowDialog() ?? false)
                {
                    string name = w.SymbolName;
                    string value = w.SymbolValue;
                    constraints[idx] = new SymbolConstraint { name = name, value = value };
                }
            }
        }

        private void RemoveConstraintBtn_Click(object sender, RoutedEventArgs e)
        {
            int idx = constraintListBox.SelectedIndex;
            if (idx != -1)
                constraints.RemoveAt(idx);
        }

        private void ConstraintListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool enabled = (sender as ListBox).SelectedItem != null;
            editConstraintBtn.IsEnabled = removeConstraintBtn.IsEnabled = enabled;
        }

        private void InputTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var input = InputTextBox.Text;
            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    ExpressionContext context = new ExpressionContext();
                    IExpression ep = ExpressionParser.Parse(input, context);
                    string formula = ep.ToLaTeX();
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
                    {
                        InputFormula.Formula = formula;
                        InputTextBox.FontWeight = FontWeights.Normal;
                        InputTextBox.Foreground = Brushes.Black;
                    });
                }
                catch
                {
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
                    {
                        InputTextBox.FontWeight = FontWeights.Heavy;
                        InputTextBox.Foreground = Brushes.OrangeRed;
                    });
                }
            });
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(OutputTextBlock.Text);
        }
    }
}
