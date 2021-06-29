using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        public MainWindow()
        {
            InitializeComponent();
            constraintListBox.ItemsSource = constraints;
        }

        private void GoButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ExpressionContext context = new ExpressionContext();
                foreach (var item in constraints)
                {
                    context.SetSymbolAssignment(context.Symbol(item.name),
                        ExpressionParser.Parse(item.value, context));
                }
                context.AnalyseConstraints();

                IExpression ep = ExpressionParser.Parse(InputTextBox.Text, context);
                ep = ep.WithAssignment().Simplify();
                OutputTextBlock.Text = ep.ToString();
                OutputFormula.Formula = ep.ToLaTeX();

            }
            catch (Exception exc)
            {
                OutputTextBlock.Text = exc.Message + "\n" + exc.StackTrace;
            }
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
            try
            {
                ExpressionContext context = new ExpressionContext();
                IExpression ep = ExpressionParser.Parse(InputTextBox.Text, context);
                InputFormula.Formula = ep.ToLaTeX();
                InputTextBox.FontWeight = FontWeights.Normal;
                InputTextBox.Foreground = Brushes.Black;
            }
            catch
            {
                InputTextBox.FontWeight = FontWeights.Heavy;
                InputTextBox.Foreground = Brushes.OrangeRed;
            }
        }
    }
}
