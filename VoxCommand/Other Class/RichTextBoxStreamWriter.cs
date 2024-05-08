using System.IO;
using System.Text;
using System.Windows.Forms;

namespace ClimaVoice.Misx_Class
{
    internal class RichTextBoxStreamWriter : TextWriter
    {
        private RichTextBox _richTextBox;

        public RichTextBoxStreamWriter(RichTextBox richTextBox)
        {
            _richTextBox = richTextBox;
        }

        public override void Write(char value)
        {
            _richTextBox.Invoke(new MethodInvoker(() =>
            {
                _richTextBox.AppendText(value.ToString());
                ScrollToBottom(_richTextBox);
            }));
        }

        public override void WriteLine(string value)
        {
            _richTextBox.Invoke(new MethodInvoker(() =>
            {
                _richTextBox.AppendText(value + "\n");
                ScrollToBottom(_richTextBox);
            }));
        }

        public override Encoding Encoding
        {
            get { return Encoding.ASCII; } // Or Encoding.UTF8, depending on your needs
        }

        private void ScrollToBottom(RichTextBox richTextBox)
        {
            richTextBox.SelectionStart = richTextBox.TextLength;
            richTextBox.ScrollToCaret();
        }
    }
}
