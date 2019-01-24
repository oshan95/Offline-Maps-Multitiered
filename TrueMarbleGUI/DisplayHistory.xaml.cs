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
using TrueMarbleBiz;

/*
Author : Oshan Mendis
ID     : 19222071
Last modified date : 13/04/2018
*/

namespace TrueMarbleGUI
{
    /// <summary>
    /// Interaction logic for DisplayHistory.xaml
    /// </summary>
    public partial class DisplayHistory : Window
    {
        private BrowseHistory m_history;
        public DisplayHistory(BrowseHistory bh)
        {
            m_history = bh;
            InitializeComponent();
        }

        //Passing data to the list view when the DisplayHistory() winows is loaded
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            IvwHistory.ItemsSource = m_history.History;
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}
