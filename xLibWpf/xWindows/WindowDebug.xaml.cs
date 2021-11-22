using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace xLib
{
    /// <summary>
    /// Логика взаимодействия для WindowDebug.xaml
    /// </summary>
    public partial class WindowDebug : Window
    {
        private const int StringInterpretationTabItemIndex = 0;

        public static ObservableCollection<string> MessageList { get; set; } = new ObservableCollection<string>();

        private static uint MessageCount = 0;
        private static bool Pause = false;
        public WindowDebug()
        {
            InitializeComponent();

            DataContext = this;
        }

        public static void Message(string str)
        {
            if (!Pause)
            {
                MessageList.Insert(0, MessageCount.ToString() + ": " + str);
                if (MessageList.Count > 5000) MessageList.RemoveAt(MessageList.Count - 1);
                MessageCount++;
            }
        }

        private void ClearBut_Click(object sender, RoutedEventArgs e)
        {
            MessageList.Clear();
        }

        private void PauseCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Pause = true;
        }

        private void PauseCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            Pause = false;
        }
    }
}
