using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Forms;
using PictureMarkingTools;

namespace GiveMarkWpfApp
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


        private string markingFolderPath;
        private string[] imageDirectories;
        private List<string> images;
        private int currentImageIndex = 0;
        private int currentQuestionIndex = 0;
        private BitmapSource currentImage;
        private PictureMarker pictureMarker;

        private void Window_Initialized(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            DialogResult result = dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                markingFolderPath = dialog.SelectedPath;
                imageDirectories = Directory.GetDirectories(markingFolderPath);
                pictureMarker = new PictureMarker(markingFolderPath);

                //New Assignment case
                if (imageDirectories.Length == 0)
                {
                    // Create OpenFileDialog 
                    Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
                    // Set filter for file extension and default file extension 
                    dlg.DefaultExt = ".zip";
                    dlg.Filter = "Zip File (*.zip)|*.zip";

                    // Display OpenFileDialog by calling ShowDialog method 
                    var fileResult = dlg.ShowDialog();
                    // Get the selected file name and display in a TextBox 
                    if (fileResult == true)
                    {
                        pictureMarker.CreateMarkingJobFolderStructure(dlg.FileName);
                        imageDirectories = Directory.GetDirectories(markingFolderPath);
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("You must select a assigment zip from Moodle.");
                        Close();
                    }
                }
                foreach (var item in imageDirectories)
                {
                    Console.WriteLine(item);
                }
                MarkQuestion();
            }
        }

        private void MarkQuestion()
        {
            UpdateQuestionLabel();
            GiveZeroToEmptyScreen();
            images = Directory.EnumerateFiles(imageDirectories[currentQuestionIndex]).ToList();
            UpdateImage();
        }

        private void UpdateQuestionLabel()
        {
            string question = GetQuestionName();
            CurrentQuestionLabel.Content = question;
        }

        private string GetQuestionName()
        {
            return new DirectoryInfo(imageDirectories[currentQuestionIndex]).Name;
        }

        private void GiveZeroToEmptyScreen()
        {
            var dr = new DirectoryInfo(imageDirectories[currentQuestionIndex]);
            var smallFiles = dr.EnumerateFiles("*", SearchOption.TopDirectoryOnly).Where(f => f.Length < 2048);
            foreach (var smallFile in smallFiles)
            {
                string distFile = Path.Combine(imageDirectories[currentQuestionIndex], "0", smallFile.Name);
                MoveFile(smallFile, distFile);
            }
        }

        public static BitmapSource BitmapImageFromFile(string filepath)
        {
            var bi = new BitmapImage();
            using (var fs = new FileStream(filepath, FileMode.Open))
            {
                bi.BeginInit();
                bi.StreamSource = fs;
                bi.CacheOption = BitmapCacheOption.OnLoad;
                bi.EndInit();
            }
            bi.Freeze(); //Important to freeze it, otherwise it will still have minor leaks
            return bi;
        }

        private void UpdateImage()
        {
            if (currentImageIndex < images.Count)
            {
                currentImage = BitmapImageFromFile(images.ElementAt(currentImageIndex));
                ScreenImage.Dispatcher.BeginInvoke(new Action(() => ScreenImage.Source = currentImage));
            }
        }

        private void UnloadImage()
        {
            ScreenImage.Source = null;
        }

        private void MarkFile(int mark)
        {
            UnloadImage();
            var image = new FileInfo(images.ElementAt(currentImageIndex));
            string distFile = Path.Combine(imageDirectories[currentQuestionIndex], mark.ToString(), image.Name);
            MoveFile(image, distFile);
        }

        private static void MoveFile(FileInfo image, string distFile)
        {
            var f = new FileInfo(distFile);
            if (!Directory.Exists(f.DirectoryName))
            {
                Directory.CreateDirectory(f.DirectoryName);
            }
            string sourceFile = image.FullName;
            File.Move(sourceFile, distFile);
        }

        private void Window_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (!(e.Key == Key.NumPad0 || e.Key == Key.NumPad1 || e.Key == Key.Right || e.Key == Key.Left))
            {
                return;
            }
            if (currentImageIndex >= images.Count && currentQuestionIndex >= imageDirectories.Length)
            {
                System.Windows.MessageBox.Show("Finish and no more questions!");
                return;
            }
            if (images.Count == 0 || e.Key == Key.Right)
            {
                currentQuestionIndex++;
                currentImageIndex=0;
                MarkQuestion();
                return;
            }
            if (e.Key == Key.Left)
            {
                if (currentQuestionIndex >= 1)
                    currentQuestionIndex--;
                currentImageIndex = 0;
                MarkQuestion();
                return;
            }
            if (e.Key == Key.NumPad0)
            {
                MarkFile(0);
                currentImageIndex++;
                UpdateImage();
            }
            else if (e.Key == Key.NumPad1)
            {
                MarkFile(1);
                currentImageIndex++;
                UpdateImage();
            }
            if (currentImageIndex >= images.Count)
            {
                //Move to next question!
                MarkNextQuestion();
                System.Windows.MessageBox.Show("Start to mark " + GetQuestionName());
            }
        }

        private void MarkNextQuestion()
        {
            currentImageIndex = 0;
            currentQuestionIndex++;
            MarkQuestion();
        }

        private void GenerateMarksheet_Click(object sender, RoutedEventArgs e)
        {
            pictureMarker.GenerateMarkSheet();
            System.Windows.MessageBox.Show("mark.csv in " + markingFolderPath);
        }
    }
}
