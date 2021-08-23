using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Wenskaarten
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //public int aantalballen;
        public MainWindow()
        {
            InitializeComponent();
            New();
          
        }

        private void New()
        {
            balCanvas.Children.Clear();
            balCanvas.Background = null;
            TekstTextBox.Text = "";
            comboBoxKleuren.SelectedIndex = 0;
            LettertypeComboBox.SelectedIndex = 0;
            LabelTekst.Content = "10";
            StatusItem.Content = "Nieuw";
            Kaart1.IsChecked = false;
            Kaart2.IsChecked = false;
            SaveEnAfdruk(false);
        }

        private void SaveEnAfdruk(Boolean actief)
        {
            Afdrukken.IsEnabled = actief;
            Opslaan.IsEnabled = actief;
        }

        private void NewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            New();
        }

        private void CloseExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }

        private void minder_Click(object sender, RoutedEventArgs e)
        {
            int aantal = Convert.ToInt16(LabelTekst.Content);
            if (aantal > 10)
                aantal--;
            LabelTekst.Content = aantal.ToString();
        }

        private void meer_Click(object sender, RoutedEventArgs e)
        {
            int aantal = Convert.ToInt16(LabelTekst.Content);
            if (aantal < 40)
                aantal++;
            LabelTekst.Content = aantal.ToString();
        }
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (MessageBox.Show("Wilt u het programma sluiten ?", "Afsluiten", MessageBoxButton.YesNo,
            MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.No)
                e.Cancel = true;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SortDescription sd = new SortDescription("Source", ListSortDirection.Ascending);
            LettertypeComboBox.Items.SortDescriptions.Add(sd);
            LettertypeComboBox.SelectedItem = new FontFamily("Arial");

            foreach (PropertyInfo info in typeof(Colors).GetProperties())
            {
                comboBoxKleuren.Items.Add(info.Name);

            }
        }
        
        private void Kaart_Click(object sender, RoutedEventArgs e)
        {
            ImageBrush img = new ImageBrush();

            if (Kaart1.IsChecked == true)
            {
                img.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/kerstkaart.jpg", UriKind.Absolute));
                balCanvas.Background = img;
                Kaart2.IsChecked = false;
                SaveEnAfdruk(true);
            }

            if (Kaart2.IsChecked == true)
            {
                img.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/geboortekaart.jpg", UriKind.Absolute));
                balCanvas.Background = img;
                Kaart1.IsChecked = false;
                SaveEnAfdruk(true);
            }
        }

        private void Ellipse_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Ellipse gesleepteEllipse = (Ellipse)e.OriginalSource;
                DataObject sleepBal = new DataObject("sleepBal", gesleepteEllipse);
                DragDrop.DoDragDrop(gesleepteEllipse, sleepBal, DragDropEffects.Move);

            }
        }
        private void Ellipse_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("sleepBal"))
            {
                Ellipse gesleepteEllipse = (Ellipse)e.Data.GetData("sleepBal");
                Ellipse nieuweBal = new Ellipse();
                nieuweBal.Fill = gesleepteEllipse.Fill;

                var positie = e.GetPosition(balCanvas);
                Canvas.SetLeft(nieuweBal, positie.X - 20);
                Canvas.SetTop(nieuweBal, positie.Y - 20);
                balCanvas.Children.Add(nieuweBal);

                if(gesleepteEllipse.Parent != null)
                {
                    balCanvas.Children.Remove(gesleepteEllipse);
                }
            }
        }

        private void EllipseVanCanvas_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("sleepBal"))
            {
                Ellipse gesleepteEllipse = (Ellipse)e.Data.GetData("sleepBal");
                balCanvas.Children.Remove(gesleepteEllipse);
            }
        }
        private void OpenExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                OpenFileDialog dlg = new OpenFileDialog();
                dlg.FileName = "";
                dlg.DefaultExt = ".txt";
                dlg.Filter = "Text documents |*.txt";

                if (dlg.ShowDialog() == true)
                {
                    using (StreamReader bestand = new StreamReader(dlg.FileName))
                    {
                        balCanvas.Children.Clear();
                        int aantalballen = int.Parse(bestand.ReadLine());
                        for (int i = 0; i < aantalballen; i++)
                        {
                            Ellipse bal = new Ellipse();
                            bal.Fill = (Brush)new BrushConverter().ConvertFromString(bestand.ReadLine());
                            double x = double.Parse(bestand.ReadLine());
                            double y = double.Parse(bestand.ReadLine());
                            Canvas.SetLeft(bal, x);
                            Canvas.SetTop(bal, y);
                            balCanvas.Children.Add(bal);
                        }

                        ImageBrush img = new ImageBrush();
                        img.ImageSource = new BitmapImage(new Uri(bestand.ReadLine(), UriKind.Absolute));
                        balCanvas.Background = img;

                        LettertypeComboBox.SelectedValue = new FontFamily(bestand.ReadLine());
                        TekstTextBox.FontSize = int.Parse(bestand.ReadLine());
                        TekstTextBox.Text = bestand.ReadLine();
                        LabelTekst.Content = bestand.ReadLine();
                    }
                    StatusItem.Content = dlg.FileName;
                    SaveEnAfdruk(true);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("openen mislukt: " + ex.Message);
            }
        }
        private void SaveExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                SaveFileDialog dlg = new SaveFileDialog();
                ImageBrush img = (ImageBrush)balCanvas.Background;
                if (img.ImageSource.ToString() == "pack://application:,,,/Images/kerstkaart.jpg")
                    dlg.FileName = "Kaart1";
                else
                    dlg.FileName = "Kaart2";

                dlg.DefaultExt = ".txt";
                dlg.Filter = "Text documents |*.txt";
                if (dlg.ShowDialog() == true)
                {
                    using (StreamWriter bestand = new StreamWriter(dlg.FileName))
                    {
                        bestand.WriteLine(balCanvas.Children.Count);
                        foreach (Ellipse bal in balCanvas.Children)
                        {
                            var x = Canvas.GetLeft(bal);
                            var y = Canvas.GetTop(bal);
                            bestand.WriteLine(bal.Fill);
                            bestand.WriteLine(x.ToString());
                            bestand.WriteLine(y.ToString());
                        }

                        bestand.WriteLine(img.ImageSource.ToString());
                        bestand.WriteLine(LettertypeComboBox.SelectedValue);
                        bestand.WriteLine(TekstTextBox.FontSize.ToString());
                        bestand.WriteLine(TekstTextBox.Text);
                        bestand.WriteLine(LabelTekst.Content.ToString());
                    }
                    StatusItem.Content = dlg.FileName;
                    New();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("opslaan mislukt: " + ex.Message);
            }
        }
    }
}

