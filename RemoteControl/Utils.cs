using NetworkLibrary.Extension;
using System.Text;

namespace RemoteControl
{
    public static class Utils
    {
        public static void OpenExecutableFile(this TextBox textBox)
        {
            OpenFileDialog ofd = new()
            {
                Title = "Please select an exe file.",
                InitialDirectory = Program.currentDir,
                Filter = "Executable files (*.exe)|*.exe"
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                string fileName = ofd.FileName;

                if (Encoding.ASCII.GetString(FileSystemUtils.ReadFileChunck(fileName, 2)) == "MZ")
                    textBox.Text = fileName;
                else
                    MessageBox.Show("File is not a valid Executable!", "ERROR");
            }
        }

        public static void ShowNoProcessMessageBox()
        {
            MessageBox.Show("No Process running for this server!", "ERROR");
        }
    }
}
