using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Presentation
{
	/// <summary>
	/// Interaction logic for NumericUpDown.xaml
	/// </summary>
	public partial class NumericUpDown : UserControl
	{
		private int _numValue = 0;
		public int NumValue
		{
			get {  return _numValue; }
			set
			{
				_numValue = value;
				txtNum.Text = value.ToString();
			}
		}

		public int Value
		{
			get { return (int)GetValue(ValueProperty); }
			set { SetValue(ValueProperty, value); }
		}

		public static readonly DependencyProperty ValueProperty = 
			DependencyProperty.Register(
			"Value", 
			typeof(int), 
			typeof(NumericUpDown),
			new UIPropertyMetadata(0, new PropertyChangedCallback(NumberChangedCallBack))
			);

		public NumericUpDown()
		{
			InitializeComponent();
			txtNum.Text = _numValue.ToString();
		}

		private void cmdUp_Click(object sender, RoutedEventArgs e)
		{
			NumValue++;
		}

		private void cmdDown_Click(object sender, RoutedEventArgs e)
		{
			NumValue--;
		}

		private void txtNum_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (!int.TryParse(txtNum.Text, out _numValue))
				txtNum.Text = _numValue.ToString();
		}

		static void NumberChangedCallBack(DependencyObject property, DependencyPropertyChangedEventArgs args)
		{
			NumericUpDown nud = (NumericUpDown)property;
			nud.NumValue = (int)args.NewValue;
		}
	}
}
