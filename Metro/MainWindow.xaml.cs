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
using System.Windows.Media.Animation;

namespace Metro
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        KyivMetroData metro = new KyivMetroData();

        List<Ellipse> selected = new List<Ellipse>();
        List<Ellipse> path = new List<Ellipse>();
        int selectedCount = 0;

        SolidColorBrush idleColorBrush = new SolidColorBrush(Color.FromRgb(255, 255, 255));
        SolidColorBrush focusedColorBrush = new SolidColorBrush(Color.FromRgb(255, 216, 170));
        SolidColorBrush selectedColorBrush = new SolidColorBrush(Color.FromRgb(255, 216, 0));
        SolidColorBrush pathColorBrush = new SolidColorBrush(Color.FromRgb(255, 216, 0));

        public MainWindow()
        {
            InitializeComponent();
            
            Canvas.Focus();

            //Adding events to all stations ellipses and adding them to canvas
            foreach (var el in metro.stations)
            {
                el.Value.ellipse.MouseEnter += StationMouseEnter;
                el.Value.ellipse.MouseLeave += StationMouseLeave;
                el.Value.ellipse.MouseLeftButtonUp += StationMouseClick;
                Canvas.Children.Add(el.Value.ellipse);
            }

            //Checking "Dont show anymore" property of entry help
            if (Properties.Settings.Default.ShowHelp == "True")
            {
                Properties.Settings.Default.ShowHelp = (!StartHelpShow()).ToString();
                Properties.Settings.Default.Save();
            }
                                    
        }

        //Making own MessegeBox with checkbox "Dont show anymore"
        private bool StartHelpShow()
        {
            Window helpWindow = new Window();
            helpWindow.Width = 460;
            helpWindow.Height = 200;
            helpWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            helpWindow.Title = "Довідка";

            StackPanel panel = new StackPanel() { Orientation = Orientation.Vertical };

            Label text = new Label();
            text.Content = "Натисніть ліву клавішу миші на двох станціях, які хочете обрати.\n\nНатисніть праву клавішу миші для скасування вибору.";
            text.Margin = new Thickness(10, 10, 0, 0);

            CheckBox chk = new CheckBox();
            chk.Content = "Не показувати знову";
            chk.Margin = new Thickness(15, 20, 0, 0);

            Button ok = new Button();
            ok.Content = "OK";
            ok.Height = 30;
            ok.Width = 50;
            ok.Margin = new Thickness(30, 15, 0, 0);
            ok.Click += (sender, e) => { helpWindow.Close(); };

            panel.Children.Add(text);
            panel.Children.Add(chk);
            panel.Children.Add(ok);
            helpWindow.Content = panel;

            helpWindow.ShowDialog();
            return (bool)chk.IsChecked;
        }

        private void MenuHelp_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Натисніть ліву клавішу миші на двох станціях, які хочете обрати.\nНатисніть праву клавішу миші для скасування вибору.");
        }

        private void MenuExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void StationMouseEnter(object sender, MouseEventArgs e)
        {
            var obj = (Ellipse)sender;

            //changing station color then mouse enter it
            if (!selected.Contains(obj) && !path.Contains(obj))
            {
                obj.Fill = focusedColorBrush;
            }
        }

        private void StationMouseLeave(object sender, MouseEventArgs e)
        {
            var obj = (Ellipse)sender;

            //changing station color to default then mouse leave it
            if (!selected.Contains(obj) && !path.Contains(obj))
            {
                obj.Fill = idleColorBrush;
            }
        }

        private void StationMouseClick(object sender, MouseButtonEventArgs e)
        {
            var obj = (Ellipse)sender;

            //selecting stations
            if (selectedCount < 2 && !selected.Contains(obj))
            {
                selected.Add(obj);
                selectedCount++;
                obj.Fill = selectedColorBrush;
                
                if (selectedCount == 2)
                {
                    path = metro.FindPath(selected[0], selected[1]);
                    DrawPath();
                }
            }
        }

        private void DrawPath()
        {
            ColorAnimation animation;
            Storyboard storyboard;
            foreach (var el in path)
            {
                el.Fill = pathColorBrush;

                //Animation block
                animation = new ColorAnimation(Color.FromRgb(255, 216, 0), Color.FromRgb(255, 106, 0), 
                    new Duration(TimeSpan.FromSeconds(0.8)));
                animation.RepeatBehavior = RepeatBehavior.Forever;
                animation.AutoReverse = true;
                storyboard = new Storyboard();
                Storyboard.SetTargetProperty(animation, new PropertyPath("(Ellipse.Fill).(SolidColorBrush.Color)") );
                storyboard.Children.Add(animation);
                
                el.BeginStoryboard(storyboard);
            }
        }

        private void UndrawPath()
        {
            foreach (var el in path)
            {
                el.Fill = idleColorBrush;
            }
        }

        private void Canvas_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            foreach (var el in selected)
            {
                ((Ellipse)Canvas.Children[Canvas.Children.IndexOf(el)]).Fill = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            }
            UndrawPath();
            selected.Clear();
            path.Clear();
            selectedCount = 0;
        }
    }
}
