﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MathematicalExpressionCalculator
{
    /// <summary>
    /// EditAssignmentWindow.xaml 的交互逻辑
    /// </summary>
    public partial class EditAssignmentWindow : Window
    {
        public string SymbolName { get; private set; }
        public string SymbolValue { get; private set; }
        public EditAssignmentWindow()
        {
            InitializeComponent();
        }
        public EditAssignmentWindow(string symbolName, string symbolValue)
        {
            InitializeComponent();
            NameTextBox.Text = SymbolName = symbolName;
            ValueTextBox.Text = SymbolValue = symbolValue;
        }
        private bool CheckSymbolName(string name)
        {
            return NameTextBox.Text.Trim().Length == 0;
        }
        private bool ParseExpression(string value, out IExpression ep)
        {
            try {
                ExpressionContext context = new ExpressionContext();
                ep = ExpressionParser.Parse(value, context);
                return true;
            }
            catch
            {
                ep = null;
                return false;
            }
        }
        private void OkBtn_Click(object sender, RoutedEventArgs e)
        {
            if (CheckSymbolName(NameTextBox.Text))
            {
                MessageBox.Show($"请输入变量名",
                        "错误",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
            }
            else if (!ParseExpression(ValueTextBox.Text, out var ep))
            {
                MessageBox.Show($"请输入合法的变量值表达式",
                        "错误",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
            }
            else
            {
                SymbolName = NameTextBox.Text;
                SymbolValue = ValueTextBox.Text;
                DialogResult = true;
            }
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void syValueTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                ExpressionContext context = new ExpressionContext();
                IExpression ep = ExpressionParser.Parse(ValueTextBox.Text, context);
                InputFormula.Formula = ep.ToLaTeX();
                ValueTextBox.FontWeight = FontWeights.Normal;
                ValueTextBox.Foreground = Brushes.Black;
            }
            catch
            {
                ValueTextBox.FontWeight = FontWeights.Heavy;
                ValueTextBox.Foreground = Brushes.OrangeRed;
            }
        }
    }
}
