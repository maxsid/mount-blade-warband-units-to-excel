using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MountAndBladeWarbandUnitsToExcel
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
   
    public partial class MainWindow : Window
    {
        MBData mbdata = new MBData();
        Thread thread;
        string folderModule, folderLocalization;
        System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
        public MainWindow()
        {
            InitializeComponent();

            viewboxFirstQuest.Child = GetNotDoneIcon();
            viewboxSecondQuest.Child = GetNotDoneIcon();
            viewboxFourthQuest.Child = GetNotDoneIcon();
        }

        private Canvas GetNotDoneIcon()
        {
            return GetCanvasWithPath("M19,6.41L17.59,5L12,10.59L6.41,5L5,6.41L10.59,12L5,17.59L6.41,19L12,13.41L17.59,19L19" +
                ",17.59L13.41,12L19,6.41Z", Brushes.Red);
        }

        private Canvas GetDoneIcon()
        {
            return GetCanvasWithPath("M21,7L9,19L3.5,13.5L4.91,12.09L9,16.17L19.59,5.59L21,7Z", Brushes.Green);
        }
        
        private Canvas GetCanvasWithPath(string pathData, Brush fill)
        {
            var canvas = new Canvas();
            canvas.Width = 24;
            canvas.Height = 24;
            var path = new System.Windows.Shapes.Path();
            path.Data = Geometry.Parse(pathData);
            path.Fill = fill;
            canvas.Children.Add(path);
            return canvas;
        }

        private string GetPath(string leftSide, string rigthSide)
        {
            return string.Format("{0}\\{1}", leftSide, rigthSide);
        }

        private bool isExistsFiles(string folderPath, params string[] files)
        {
            foreach (var file in files)
            {
                if (!File.Exists(GetPath(folderPath, file)))
                {
                    return false;
                }
            }
            return true;
        }

        private void buttonFirst_Click(object sender, RoutedEventArgs e)
        {
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                folderModule = fbd.SelectedPath;
                if (!isExistsFiles(folderModule, "troops.txt", "factions.txt"))
                    throw new Exception("В папке нет всех нужных файлов.");
                var factions = File.ReadAllLines(GetPath(folderModule, "factions.txt"));
                var troops = File.ReadAllLines(GetPath(folderModule,"troops.txt"));
                mbdata.SetUnits(troops);
                mbdata.SetFactions(factions);

                viewboxFirstQuest.Child = GetDoneIcon();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            thread.Abort();
        }

        private void buttonSecond_Click(object sender, RoutedEventArgs e)
        {
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                folderLocalization = fbd.SelectedPath;
                if (!isExistsFiles(folderLocalization, "troops.csv", "factions.csv"))
                    throw new Exception("В папке нет нужных файлов.");
                var csvFiles = Directory.GetFiles(folderLocalization);
                foreach (var csv in csvFiles)
                    mbdata.AddLocalizationInDictonary(File.ReadAllLines(csv));
                mbdata.FillUnitsAndFactionsLocalization();
                viewboxSecondQuest.Child = GetDoneIcon();
            }
        }

        private void buttonFourth_Click(object sender, RoutedEventArgs e)
        {
            buttonFourth.IsEnabled = false;
            buttonFirst.IsEnabled = false;
            buttonSecond.IsEnabled = false;
            mbdata.progressBar = progressBar;
            thread = new Thread(delegate () 
            {
                mbdata.CreateExcelWorkbook();
                Dispatcher.Invoke(delegate () 
                {
                    viewboxFourthQuest.Child = GetDoneIcon();
                    buttonFourth.IsEnabled = true;
                    buttonFirst.IsEnabled = true;
                    buttonSecond.IsEnabled = true;
                });
            });
            thread.Start();
        }
    }
}
