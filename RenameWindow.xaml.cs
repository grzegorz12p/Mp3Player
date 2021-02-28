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

namespace Projekt
{
    /// <summary>
    /// Logika interakcji dla klasy RenameWindow.xaml
    /// </summary>
    public partial class RenameWindow : Window
    {


        private bool isClicked;
        public event Action<string> Check;

        public RenameWindow()
        {
            InitializeComponent();
            isClicked = false;
        }

        private void RenameName(object sender, RoutedEventArgs e)
        {
            if(newName.Text != null && newName.Text.Length > 1)
            {
                isClicked = true;
                Check = delegate (string newName)
                {
                    Console.WriteLine(newName);
                };
                Check(newName.Text);
                if (Check != null)
                {
                    ((MainWindow)Application.Current.MainWindow).folderName = newName.Text;
                    Close();
                }
            }
        }


        public Button GetButton()
        {
            return this.renameButton;
        }


        public bool IsClicked()
        {
            return isClicked;
        }
    }
}
