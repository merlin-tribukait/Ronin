using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Media;
using log4net;
using RichTextBox = System.Windows.Controls.RichTextBox;
using log4net.Appender;
using log4net.Repository.Hierarchy;

namespace Ronin.Utilities
{
    public class TextBoxAppender : AppenderSkeleton
    {
        private RichTextBox _textBox;
        public RichTextBox AppenderTextBox
        {
            get
            {
                return _textBox;
            }
            set
            {
                _textBox = value;
            }
        }
        public string FormName { get; set; }
        public string TextBoxName { get; set; }

        private Control FindControlRecursive(Control root, string textBoxName)
        {
            if (root.Name == textBoxName) return root;
            foreach (Control c in root.Controls)
            {
                Control t = FindControlRecursive(c, textBoxName);
                if (t != null) return t;
            }
            return null;
        }

        protected override void Append(log4net.Core.LoggingEvent loggingEvent)
        {
            MainWindow form = MainWindow.pSelf;
            if (_textBox == null)
            {
                //if (String.IsNullOrEmpty(FormName) ||
                //    String.IsNullOrEmpty(TextBoxName))
                //    return;



                if (form == null)
                    return;

                _textBox = MainWindow.pSelf.textBoxLog;
                if (_textBox == null)
                    return;

                form.Closing += (s, e) => _textBox = null;
            }

            System.Drawing.Color _textColor;

            switch (loggingEvent.Level.DisplayName.ToUpper())
            {
                case "FATAL":
                    _textColor = System.Drawing.Color.Magenta;
                    break;

                case "ERROR":
                    _textColor = System.Drawing.Color.Red;
                    break;

                case "WARN":
                    _textColor = System.Drawing.Color.Tan;
                    break;

                case "INFO":
                    _textColor = System.Drawing.Color.Black;
                    break;

                case "DEBUG":
                    _textColor = System.Drawing.Color.SteelBlue;
                    break;

                default:
                    _textColor = System.Drawing.Color.Black;
                    break;
            }



            form.Dispatcher.BeginInvoke((MethodInvoker)delegate
            {
                TextRange tr = new TextRange(_textBox.Document.ContentEnd, _textBox.Document.ContentEnd);

                StringBuilder sb = new StringBuilder();

                sb.Append(loggingEvent.TimeStamp.ToString("hh:mm:ss.fff"));
                sb.AppendLine(" " + loggingEvent.Level.ToString() + " - " + loggingEvent.RenderedMessage);

                tr.Text = sb.ToString();

                tr.ApplyPropertyValue(TextElement.ForegroundProperty,
                    new SolidColorBrush(System.Windows.Media.Color.FromRgb(_textColor.R, _textColor.G, _textColor.B)));
            });
        }
    }
}
