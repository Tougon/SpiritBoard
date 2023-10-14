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
using System.Windows.Shapes;

namespace SpiritBoard
{
    /// <summary>
    /// Interaction logic for DropdownPopup.xaml
    /// </summary>
    public partial class DropdownPopup : Window
	{
		public DropdownPopup(Dictionary<ulong, SpiritValueDisplay.SpiritID> data, string defaultPrompt = "")
		{
			this.InitializeComponent();
			dropdown.ItemsSource = data.Values;
			lblQuestion.Content = defaultPrompt;
		}

		private void btnDialogOk_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = true;
		}

		private void Window_ContentRendered(object sender, EventArgs e)
		{
			dropdown.SelectedIndex = 0;
		}
	}
}
