using System.Text;
using System.Windows.Forms;

namespace RemoteControl
{
    public class ControlWriter : TextWriter
    {
        private Control textbox;

        public ControlWriter(Control textbox)
        {
            this.textbox = textbox;
        }

        public override void Write(char value)
        {
            if (textbox.InvokeRequired)
            {
                textbox.Invoke(new Action(() =>
                {
                    textbox.Text += value;
                }));
            }
            else
                textbox.Text += value;

            Thread.Sleep(1); // Avoids GUI stalling issues if spammed.
        }

        public override void Write(string? value)
        {
            if (textbox.InvokeRequired)
            {
                textbox.Invoke(new Action(() =>
                {
                    textbox.Text += value;
                }));
            }
            else
                textbox.Text += value;

            Thread.Sleep(1); // Avoids GUI stalling issues if spammed.
        }

        public override Encoding Encoding
        {
            get { return Encoding.ASCII; }
        }
    }
}
