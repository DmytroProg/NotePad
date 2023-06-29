using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;

namespace Notepad
{
    /*
     TODO
     
     */

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool isSaved = false;
        private string path = null;
        private string fileName = "Без імені";
        private int mashtab = 100;

        private SaveFileDialog saveFileDialog;
        private OpenFileDialog openFileDialog;
        private PrintDialog printDialog;
        private System.Windows.Forms.FontDialog fontDialog;
        private readonly string filter = "Text files (*.txt) |*.txt| All files (*.*) |*.*";

        public MainWindow()
        {
            InitializeComponent();
            Setup();
        }

        private void Setup()
        {
            saveFileDialog = new SaveFileDialog();
            openFileDialog = new OpenFileDialog();
            printDialog = new PrintDialog();
            fontDialog = new System.Windows.Forms.FontDialog();
            saveFileDialog.Filter = openFileDialog.Filter = filter;
            if(Properties.Settings.Default.Position.Height != 0 && Properties.Settings.Default.Position.Width != 0)
            {
                this.Height = Properties.Settings.Default.Position.Height;
                this.Width = Properties.Settings.Default.Position.Width;
                this.Top = Properties.Settings.Default.Position.Y;
                this.Left = Properties.Settings.Default.Position.X;
                fontDialog.Font = Properties.Settings.Default.Font;
                SetFont();
            }
        }

        private void NewCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
        }

        private void OpenCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if(openFileDialog.ShowDialog() == true)
            {
                path = openFileDialog.FileName;
                using(var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    using(var sr = new StreamReader(fs))
                    {
                        mainTextBox.Text = sr.ReadToEnd();
                    }
                }
                isSaved = true;
            }
        }

        private void PrintCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            printDialog.ShowDialog();
        }

        private void DeleteCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            int index = mainTextBox.CaretIndex;
            mainTextBox.Text = mainTextBox.Text.Remove(mainTextBox.SelectionStart, mainTextBox.SelectionLength);
            mainTextBox.CaretIndex = index;
        }

        private void DeleteCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = mainTextBox.SelectionLength != 0;
        }

        private void SaveCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (path != null)
            {
                SaveFile();
                isSaved = true;
            }
            else
            {
                SaveAsCommand_Executed(sender, e);
            }
        }

        private void SaveAsCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if(saveFileDialog.ShowDialog() == true)
            {
                path = saveFileDialog.FileName;
                FileInfo fileInfo = new FileInfo(path);
                fileName = fileInfo.Name;
                this.Title = fileName + ": Блокнот";
                SaveFile();
                isSaved = true;
            }
        }

        private void SaveFile()
        {
            using(var fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write))
            {
                using(var sw = new StreamWriter(fs))
                {
                    sw.WriteLine(mainTextBox.Text);
                }
            }
            this.Title = fileName + ": Блокнот";
        }

        private bool TextChanged { get => !isSaved 
                && !String.IsNullOrEmpty(mainTextBox.Text) 
                && !String.IsNullOrWhiteSpace(mainTextBox.Text);
        }

        private void SaveCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = TextChanged;
        }

        private void FindCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SearchWindow searchWindow = new SearchWindow();
            searchWindow.Show(mainTextBox);
        }

        private void MashtabUp(object sender, RoutedEventArgs e)
        {
            if (mashtab == 500) return;
            mainTextBox.FontSize = (int)(mainTextBox.FontSize * 100 / 90);
            mashtab += 10;
            mashStatusBarItem.Content = mashtab.ToString() + "%";
        }

        private void MashtabDown(object sender, RoutedEventArgs e)
        {
            if (mashtab == 10) return;
            mainTextBox.FontSize = (int)(mainTextBox.FontSize * 100 / 110);
            mashtab -= 10;
            mashStatusBarItem.Content = mashtab.ToString() + "%";
        }

        private void ReMashtab(object sender, RoutedEventArgs e)
        {
            mainTextBox.FontSize = 14;
            mashtab = 100;
            mashStatusBarItem.Content = mashtab.ToString() + "%";
        }

        private void SetFont()
        {
            mainTextBox.FontFamily = new System.Windows.Media.FontFamily(fontDialog.Font.Name);
            mainTextBox.FontSize = fontDialog.Font.Size * 96.0 / 72.0;
            mainTextBox.FontWeight = fontDialog.Font.Bold ? FontWeights.Bold : FontWeights.Regular;
            mainTextBox.FontStyle = fontDialog.Font.Italic ? FontStyles.Italic : FontStyles.Normal;

            TextDecorationCollection tdc = new TextDecorationCollection();
            if (fontDialog.Font.Underline) tdc.Add(TextDecorations.Underline);
            if (fontDialog.Font.Strikeout) tdc.Add(TextDecorations.Strikethrough);
            mainTextBox.TextDecorations = tdc;
        }

        private void OpenFontDialog(object sender, RoutedEventArgs e)
        {
            var result = fontDialog.ShowDialog();
            if(result == System.Windows.Forms.DialogResult.OK)
            {
                SetFont();   
            }
        }

        private void FindCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !String.IsNullOrEmpty(mainTextBox.Text)
                && !String.IsNullOrWhiteSpace(mainTextBox.Text);
        }

        private void ReplaceCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ReplaceWindow replaceWindow = new ReplaceWindow();
            replaceWindow.Show(mainTextBox);
        }

        private void InsertDate(object sender, RoutedEventArgs e)
        {
            string text = mainTextBox.Text.Substring(0, mainTextBox.CaretIndex);
            text += DateTime.Now.ToString();
            text += mainTextBox.Text.Substring(mainTextBox.CaretIndex, mainTextBox.Text.Length - mainTextBox.CaretIndex);
            int index = mainTextBox.CaretIndex;
            mainTextBox.Text = text;
            mainTextBox.CaretIndex = index;
        }

        private void UseWrap(object sender, RoutedEventArgs e)
        {
            mainTextBox.TextWrapping = TextWrapping.WrapWithOverflow;
        }

        private void LoseWrap(object sender, RoutedEventArgs e)
        {
            mainTextBox.TextWrapping = TextWrapping.NoWrap;
        }

        private void ShowStatusBar(object sender, RoutedEventArgs e)
        {
            statusBar.Visibility = statusBar.Visibility == Visibility.Visible? Visibility.Hidden : Visibility.Visible;
        }

        private void OpenLink(object sender, RoutedEventArgs e)
        {
            Process.Start("chrome.exe", "https://support.microsoft.com/uk-ua/windows/%D0%B4%D0%BE%D0%B2%D1%96%D0%B4%D0%BA%D0%B0-%D1%83-%D0%BF%D1%80%D0%BE%D0%B3%D1%80%D0%B0%D0%BC%D1%96-%D0%B1%D0%BB%D0%BE%D0%BA%D0%BD%D0%BE%D1%82-4d68c388-2ff2-0e7f-b706-35fb2ab88a8c");
        }

        private void SaveSettings()
        {
            Properties.Settings.Default.Position = this.RestoreBounds;
            Properties.Settings.Default.Font = fontDialog.Font;
            Properties.Settings.Default.Save();
        }

        private void CloseApp(object sender, RoutedEventArgs e)
        {
            SaveSettings();
            if (!TextChanged) this.Close();
            var result = MessageBox.Show($"Файл {this.fileName} не збережено. Бажаєте зберегти зміни?", "Блокнот",
                MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                SaveFile();
            }
            else if (result == MessageBoxResult.Cancel)
            {
                return;
            }
            this.Close();
        }

        private void OpenAboutWindow(object sender, RoutedEventArgs e)
        {
            AboutWondow aboutWondow = new AboutWondow();
            aboutWondow.ShowDialog();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveSettings();
            if (!TextChanged) return;
            var result = MessageBox.Show($"Файл {this.fileName} не збережено. Бажаєте зберегти зміни?", "Блокнот",
                MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
            if(result == MessageBoxResult.Yes)
            {
                SaveFile();
            }
            else if(result == MessageBoxResult.Cancel) 
            {
                e.Cancel = true;
            }
        }

        private void mainTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.Title = "*" + fileName + ": Блокнот";
            isSaved = false;
            posStatusBarItem.Content = $"Рд {mainTextBox.GetLineIndexFromCharacterIndex(mainTextBox.CaretIndex)+1}" +
                $", ствп {mainTextBox.CaretIndex - mainTextBox.GetCharacterIndexFromLineIndex(mainTextBox.GetLineIndexFromCharacterIndex(mainTextBox.CaretIndex)) +1}";

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            mainTextBox.Focus();
        }
    }
}
