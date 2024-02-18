namespace GraphicalUserInterface
{
    public partial class MainForm : Form
    {
        private static string httpsguid = string.Empty;
        private static string httpguid = string.Empty;
        private static string ssfwguid = string.Empty;
        private static string svoguid = string.Empty;
        private static string tycoonguid = string.Empty;
        private static string horizonguid = string.Empty;
        private static string dnsguid = string.Empty;
        private static string quazalguid = string.Empty;
        private static string eaemuguid = string.Empty;

        public MainForm()
        {
            InitializeComponent();
            Console.SetOut(new MultiTextWriter(new ControlWriter(richTextBoxLog), Console.Out));
            // Attach the event handler to the FormClosing event
            FormClosing += MainForm_FormClosing;
        }

        private async void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            // Your code to execute when the form is closing
            // For example, you can show a confirmation dialog
            DialogResult result = MessageBox.Show("Do you really want to close the application?\nShutdown can take a little while if servers are running.", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            // If the user clicks "No", cancel the form closing
            if (result == DialogResult.No)
                e.Cancel = true;
            else
            {
                Console.SetOut(TextWriter.Null); // Avoids a race-condition when stalling apps try to write the info to the disposed richtextbox.

                try
                {
                    if (!string.IsNullOrEmpty(httpsguid))
                        // Stop the program
                        await ProcessManager.ShutdownProcess(httpsguid);
                    if (!string.IsNullOrEmpty(httpguid))
                        // Stop the program
                        await ProcessManager.ShutdownProcess(httpguid);
                    if (!string.IsNullOrEmpty(ssfwguid))
                        // Stop the program
                        await ProcessManager.ShutdownProcess(ssfwguid);
                    if (!string.IsNullOrEmpty(svoguid))
                        // Stop the program
                        await ProcessManager.ShutdownProcess(svoguid);
                    if (!string.IsNullOrEmpty(tycoonguid))
                        // Stop the program
                        await ProcessManager.ShutdownProcess(tycoonguid);
                    if (!string.IsNullOrEmpty(horizonguid))
                        // Stop the program
                        await ProcessManager.ShutdownProcess(horizonguid);
                    if (!string.IsNullOrEmpty(dnsguid))
                        // Stop the program
                        await ProcessManager.ShutdownProcess(dnsguid);
                    if (!string.IsNullOrEmpty(quazalguid))
                        // Stop the program
                        await ProcessManager.ShutdownProcess(quazalguid);
                    if (!string.IsNullOrEmpty(eaemuguid))
                        // Stop the program
                        await ProcessManager.ShutdownProcess(eaemuguid);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Application thrown an exception while closing.");
                }
            }
        }

        private async void buttonStartHTTPS_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(httpsguid))
                // Stop the program
                await ProcessManager.ShutdownProcess(httpsguid);

            httpsguid = Guid.NewGuid().ToString();
            // Start the program
            _ = ProcessManager.StartupProgram("HTTPSecureServerLite.exe", httpsguid);
        }

        private async void buttonStartHTTP_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(httpguid))
                // Stop the program
                await ProcessManager.ShutdownProcess(httpguid);

            httpguid = Guid.NewGuid().ToString();
            // Start the program
            _ = ProcessManager.StartupProgram("HTTPServer.exe", httpguid);
        }

        private async void buttonStartSSFW_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(ssfwguid))
                // Stop the program
                await ProcessManager.ShutdownProcess(ssfwguid);

            ssfwguid = Guid.NewGuid().ToString();
            // Start the program
            _ = ProcessManager.StartupProgram("SSFWServer.exe", ssfwguid);
        }

        private async void buttonStartSVO_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(svoguid))
                // Stop the program
                await ProcessManager.ShutdownProcess(svoguid);

            svoguid = Guid.NewGuid().ToString();
            // Start the program
            _ = ProcessManager.StartupProgram("SVO.exe", svoguid);
        }

        private async void buttonStartTycoon_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(tycoonguid))
                // Stop the program
                await ProcessManager.ShutdownProcess(tycoonguid);

            tycoonguid = Guid.NewGuid().ToString();
            // Start the program
            _ = ProcessManager.StartupProgram("TycoonServer.exe", tycoonguid);
        }

        private async void buttonStartHorizon_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(horizonguid))
                // Stop the program
                await ProcessManager.ShutdownProcess(horizonguid);

            horizonguid = Guid.NewGuid().ToString();
            // Start the program
            _ = ProcessManager.StartupProgram("Horizon.exe", horizonguid);
        }

        private async void buttonStartDNS_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(dnsguid))
                // Stop the program
                await ProcessManager.ShutdownProcess(dnsguid);

            dnsguid = Guid.NewGuid().ToString();
            // Start the program
            _ = ProcessManager.StartupProgram("MitmDNS.exe", dnsguid);
        }

        private async void buttonStartQuazal_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(quazalguid))
                // Stop the program
                await ProcessManager.ShutdownProcess(quazalguid);

            quazalguid = Guid.NewGuid().ToString();
            // Start the program
            _ = ProcessManager.StartupProgram("QuazalServer.exe", quazalguid);
        }


        private async void buttonStarteaEmu_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(eaemuguid))
                // Stop the program
                await ProcessManager.ShutdownProcess(eaemuguid);

            eaemuguid = Guid.NewGuid().ToString();
            // Start the program
            _ = ProcessManager.StartupProgram("eaEmuServer.exe", eaemuguid);
        }

        private async void buttonStopHTTPS_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(httpsguid))
            {
                // Stop the program
                await ProcessManager.ShutdownProcess(httpsguid);
                httpsguid = string.Empty;
            }
            else
                Console.WriteLine("No Process started for this server");
        }

        private async void buttonStopHTTP_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(httpguid))
            {
                // Stop the program
                await ProcessManager.ShutdownProcess(httpguid);
                httpguid = string.Empty;
            }
            else
                Console.WriteLine("No Process started for this server");
        }

        private async void buttonStopSSFW_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(ssfwguid))
            {
                // Stop the program
                await ProcessManager.ShutdownProcess(ssfwguid);
                ssfwguid = string.Empty;
            }
            else
                Console.WriteLine("No Process started for this server");
        }

        private async void buttonStopSVO_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(svoguid))
            {
                // Stop the program
                await ProcessManager.ShutdownProcess(svoguid);
                svoguid = string.Empty;
            }
            else
                Console.WriteLine("No Process started for this server");
        }

        private async void buttonStopTycoon_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(tycoonguid))
            {
                // Stop the program
                await ProcessManager.ShutdownProcess(tycoonguid);
                tycoonguid = string.Empty;
            }
            else
                Console.WriteLine("No Process started for this server");
        }

        private async void buttonStopHorizon_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(horizonguid))
            {
                // Stop the program
                await ProcessManager.ShutdownProcess(horizonguid);
                horizonguid = string.Empty;
            }
            else
                Console.WriteLine("No Process started for this server");
        }

        private async void buttonStopDNS_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(dnsguid))
            {
                // Stop the program
                await ProcessManager.ShutdownProcess(dnsguid);
                dnsguid = string.Empty;
            }
            else
                Console.WriteLine("No Process started for this server");
        }

        private async void buttonStopQuazal_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(quazalguid))
            {
                // Stop the program
                await ProcessManager.ShutdownProcess(quazalguid);
                quazalguid = string.Empty;
            }
            else
                Console.WriteLine("No Process started for this server");
        }

        private async void buttonStopeaEmu_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(eaemuguid))
            {
                // Stop the program
                await ProcessManager.ShutdownProcess(eaemuguid);
                eaemuguid = string.Empty;
            }
            else
                Console.WriteLine("No Process started for this server");
        }
    }
}
