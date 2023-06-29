using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
using static System.Net.Mime.MediaTypeNames;

namespace Notepad
{
    /// <summary>
    /// Interaction logic for SearchWindow.xaml
    /// </summary>
    public partial class SearchWindow : Window
    {
        private TextBox mainTextBox;
        private List<int> indexes;
        private int currentIndex = 0;

        public SearchWindow()
        {
            InitializeComponent();
            indexes = new List<int>();
        }

        public void Show(TextBox textBox)
        {
            this.mainTextBox = textBox;
            this.Show();
        }

        private void FindAll()
        {
            string text = registerCheckBox.IsChecked == true ? mainTextBox.Text : mainTextBox.Text.ToLower();
            text = text.Replace("\r\n", "  ");
            string search = registerCheckBox.IsChecked == true ? searchTextBox.Text : searchTextBox.Text.ToLower();
            search = textFloatCheckBox.IsChecked == true ? search : $" {search.Trim()} ";

            for (int i = 0; i < text.Length - search.Length; i++)
            {
                if (text.Substring(i, search.Length) == search)
                {
                    if (search[0] == ' ')
                        indexes.Add(i + 1);
                    else indexes.Add(i);
                }
            }
            if (upRadioBtn.IsChecked == true)
            {
                currentIndex = 0;
            }
            else if(downRadioBtn.IsChecked == true)
            {
                currentIndex = indexes.Count - 1;
            }
        }

        private void searchBtn_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(searchTextBox.Text) || String.IsNullOrWhiteSpace(searchTextBox.Text))
                return;
            if (indexes.Count == 0)
                FindAll();
            if(currentIndex < indexes.Count && currentIndex >= 0)
            {
                mainTextBox.SelectionStart = indexes[currentIndex];
                mainTextBox.SelectionLength = searchTextBox.Text.Length;
                currentIndex += upRadioBtn.IsChecked == true ? 1 : -1;
                if (currentIndex >= indexes.Count)
                    currentIndex = 0;
                else if(currentIndex < 0)
                    currentIndex = indexes.Count - 1;
                mainTextBox.Focus();
            }
        }

        private void searchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            indexes.Clear();
        }

        private void cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Grid_Checked(object sender, RoutedEventArgs e)
        {
            if(indexes != null)
                indexes.Clear();
        }
    }
}
