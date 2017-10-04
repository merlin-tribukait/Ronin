using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace Ronin
{
    /// <summary>
    /// Interaction logic for AdditionalPartyCondtions.xaml
    /// </summary>
    public partial class AdditionalPartyCondtions
    {
        public static AdditionalPartyCondtions Instance;

        public AdditionalPartyCondtions()
        {
            InitializeComponent();
            Instance = this;

            this.Closed += delegate { Instance = null; };
            this.MouseDown += delegate {
                try
                {
                    BindingExpression exp = ((TextBox)Keyboard.FocusedElement).GetBindingExpression(TextBox.TextProperty);
                    exp?.UpdateSource();
                    if (exp?.HasValidationError != null && (bool)exp?.HasValidationError)
                        ((TextBox)Keyboard.FocusedElement).Text = "0";

                    exp?.UpdateSource();
                    Keyboard.ClearFocus();
                }
                catch (Exception)
                {
                    // ignored
                }
            };
        }

        private void TwoDigitValidation_TbTextChange(object sender, TextChangedEventArgs e)
        {
            var txtBox = (TextBox)sender;
            txtBox.Text = Regex.Replace(txtBox.Text, "[^0-9]+", String.Empty);
            if (txtBox.Text.Length > 2)
            {
                txtBox.Text = txtBox.Text.Substring(1, 2);
            }

            txtBox.CaretIndex = txtBox.Text.Length;
        }

        private void CommitChanges_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BindingExpression exp = ((TextBox)sender).GetBindingExpression(TextBox.TextProperty);
                exp.UpdateSource();
            }
        }
    }
}
