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

namespace AlgebraicExpressionSimplifier
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void GoBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string text = epTextBox.Text;
                IExpression ep = ExpressionParser.Parse(text);
                if (ep is ExpressionTree)
                {
                    ep = ExpressionSimplifier.Simplify((ExpressionTree)ep);
                }
                resTextBlock.Text = ep.ToString();
            }
            catch (Exception exc)
            {
                resTextBlock.Text = exc.Message + "\n" + exc.StackTrace;
            }
        }
    }
}
