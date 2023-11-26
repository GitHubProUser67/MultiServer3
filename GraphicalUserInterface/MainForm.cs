namespace GraphicalUserInterface
{
    public partial class MainForm : Form
    {
        private static string httpsguid = string.Empty;
        private static string httpguid = string.Empty;
        private static string ssfwguid = string.Empty;
        private static string svoguid = string.Empty;
        private static string horizonguid = string.Empty;
        private static string dnsguid = string.Empty;

        public MainForm()
        {
            InitializeComponent();
            Console.SetOut(new MultiTextWriter(new ControlWriter(richTextBoxLog), Console.Out));
            // Attach the event handler to the FormClosing event
            FormClosing += MainForm_FormClosing;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Your code to execute when the form is closing
            // For example, you can show a confirmation dialog
            DialogResult result = MessageBox.Show("Do you really want to close the application?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            // If the user clicks "No", cancel the form closing
            if (result == DialogResult.No)
                e.Cancel = true;
            else
            {
                if (!string.IsNullOrEmpty(httpsguid))
                    // Stop the program
                    ProcessManager.ShutdownProcess(httpsguid);
                if (!string.IsNullOrEmpty(httpguid))
                    // Stop the program
                    ProcessManager.ShutdownProcess(httpguid);
                if (!string.IsNullOrEmpty(ssfwguid))
                    // Stop the program
                    ProcessManager.ShutdownProcess(ssfwguid);
                if (!string.IsNullOrEmpty(svoguid))
                    // Stop the program
                    ProcessManager.ShutdownProcess(svoguid);
                if (!string.IsNullOrEmpty(horizonguid))
                    // Stop the program
                    ProcessManager.ShutdownProcess(horizonguid);
                if (!string.IsNullOrEmpty(dnsguid))
                    // Stop the program
                    ProcessManager.ShutdownProcess(dnsguid);
            }
        }

        private void buttonStartHTTPS_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(httpsguid))
                // Stop the program
                ProcessManager.ShutdownProcess(httpsguid);

            httpsguid = Guid.NewGuid().ToString();
            // Start the program
            ProcessManager.StartupProgram("HTTPSecureServerLite.exe", httpsguid);
        }

        private void buttonStartHTTP_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(httpguid))
                // Stop the program
                ProcessManager.ShutdownProcess(httpguid);

            httpguid = Guid.NewGuid().ToString();
            // Start the program
            ProcessManager.StartupProgram("HTTPServer.exe", httpguid);
        }

        private void buttonStartSSFW_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(ssfwguid))
                // Stop the program
                ProcessManager.ShutdownProcess(ssfwguid);

            ssfwguid = Guid.NewGuid().ToString();
            // Start the program
            ProcessManager.StartupProgram("SSFWServer.exe", ssfwguid);
        }

        private void buttonStartSVO_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(svoguid))
                // Stop the program
                ProcessManager.ShutdownProcess(svoguid);

            svoguid = Guid.NewGuid().ToString();
            // Start the program
            ProcessManager.StartupProgram("SVO.exe", svoguid);
        }

        private void buttonStartHorizon_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(horizonguid))
                // Stop the program
                ProcessManager.ShutdownProcess(horizonguid);

            horizonguid = Guid.NewGuid().ToString();
            // Start the program
            ProcessManager.StartupProgram("Horizon.exe", horizonguid);
        }

        private void buttonStartDNS_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(dnsguid))
                // Stop the program
                ProcessManager.ShutdownProcess(dnsguid);

            dnsguid = Guid.NewGuid().ToString();
            // Start the program
            ProcessManager.StartupProgram("MitmDNS.exe", dnsguid);
        }

        private void buttonStopHTTPS_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(httpsguid))
            {
                // Stop the program
                ProcessManager.ShutdownProcess(httpsguid);
                httpsguid = string.Empty;
            }
            else
                Console.WriteLine("No Process started for this server");
        }

        private void buttonStopHTTP_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(httpguid))
            {
                // Stop the program
                ProcessManager.ShutdownProcess(httpguid);
                httpguid = string.Empty;
            }
            else
                Console.WriteLine("No Process started for this server");
        }

        private void buttonStopSSFW_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(ssfwguid))
            {
                // Stop the program
                ProcessManager.ShutdownProcess(ssfwguid);
                ssfwguid = string.Empty;
            }
            else
                Console.WriteLine("No Process started for this server");
        }

        private void buttonStopSVO_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(svoguid))
            {
                // Stop the program
                ProcessManager.ShutdownProcess(svoguid);
                svoguid = string.Empty;
            }
            else
                Console.WriteLine("No Process started for this server");
        }

        private void buttonStopHorizon_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(horizonguid))
            {
                // Stop the program
                ProcessManager.ShutdownProcess(horizonguid);
                horizonguid = string.Empty;
            }
            else
                Console.WriteLine("No Process started for this server");
        }

        private void buttonStopDNS_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(dnsguid))
            {
                // Stop the program
                ProcessManager.ShutdownProcess(dnsguid);
                dnsguid = string.Empty;
            }
            else
                Console.WriteLine("No Process started for this server");
        }
    }
}
