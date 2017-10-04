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
using Cryptlex;

namespace Ronin
{
    /// <summary>
    /// Interaction logic for LoginForm.xaml
    /// </summary>
    public partial class LoginForm 
    {
        public LoginForm()
        {
            InitializeComponent();
        }

        private void Activate_Click(object sender, RoutedEventArgs e)
        {
            int status;
            status = LexActivator.SetProductKey(keyTb.Text.Trim());
            if (status == LexActivator.LA_OK)
            {

            }
            else
            {
                MessageBox.Show("Incorrect key.");
                return;
            }

            status = LexActivator.ActivateProduct();
            if (status == LexActivator.LA_OK)
            {
                MainWindow.legit = true;
                Close();
            }
            else if (status == LexActivator.LA_EXPIRED)
            {
                MessageBox.Show("Incorrect key.");
            }
            else
            {
                MessageBox.Show("Incorrect key.");
            }
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!MainWindow.legit)
            {
                Environment.Exit(0);
            }
        }

        //private void TrialButton_Click(object sender, RoutedEventArgs e)
        //{
        //    int status;
        //    status = LexActivator.SetTrialKey("A54D455F-19CDAC7D-F49499B9-22318226-AE1EBC78");
        //    if (status != LexActivator.LA_OK)
        //    {
        //        MessageBox.Show("Corrupted data.");
        //        return;
        //    }

        //    status = LexActivator.ActivateTrial();
        //    if (status == LexActivator.LA_OK)
        //    {
        //        MessageBox.Show("Activated trial.");
        //        MainWindow.legit = true;
        //        Close();
        //    }
        //    else
        //    {
        //        MessageBox.Show("You are not eligible for trial.");
        //    }
        //}
    }
}
