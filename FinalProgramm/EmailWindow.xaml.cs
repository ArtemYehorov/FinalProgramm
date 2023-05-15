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

namespace FinalProgramm
{
    /// <summary>
    /// Логика взаимодействия для EmailWindow.xaml
    /// </summary>
    using System.Windows;

    public partial class EmailWindow : Window
    {
        public string Email { get; private set; }

        public EmailWindow()
        {
            InitializeComponent();
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            Email = EmailTextBox.Text;
            Close();
        }
    }
}
