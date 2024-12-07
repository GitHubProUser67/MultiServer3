using Ionic.Crc;
using System.Text;

namespace RemoteControl
{
    public partial class FormMain : Form
    {
        private Dictionary<int, ControlWriter> _writersList = new();

        private const string exeExtension = ".exe";

        private readonly int httpCRC = CRC32.Compute(Encoding.ASCII.GetBytes("HTTPServer" + exeExtension));
        private readonly int httpsCRC = CRC32.Compute(Encoding.ASCII.GetBytes("HTTPSecureServerLite" + exeExtension));
        private readonly int dnsCRC = CRC32.Compute(Encoding.ASCII.GetBytes("MitmDNS" + exeExtension));
        private readonly int horizonCRC = CRC32.Compute(Encoding.ASCII.GetBytes("Horizon" + exeExtension));
        private readonly int multisocksCRC = CRC32.Compute(Encoding.ASCII.GetBytes("MultiSocks" + exeExtension));
        private readonly int multispyCRC = CRC32.Compute(Encoding.ASCII.GetBytes("MultiSpy" + exeExtension));
        private readonly int quazalCRC = CRC32.Compute(Encoding.ASCII.GetBytes("QuazalServer" + exeExtension));
        private readonly int ssfwCRC = CRC32.Compute(Encoding.ASCII.GetBytes("SSFWServer" + exeExtension));
        private readonly int svoCRC = CRC32.Compute(Encoding.ASCII.GetBytes("SVO" + exeExtension));
        private readonly int edenCRC = CRC32.Compute(Encoding.ASCII.GetBytes("EdenServer" + exeExtension));

        public FormMain()
        {
            InitializeComponent();

            // Settings programs path.
            textBoxHTTPPath.Text = Program.currentDir + "/HTTPServer" + exeExtension;
            textBoxHTTPSPath.Text = Program.currentDir + "/HTTPSecureServerLite" + exeExtension;
            textBoxDNSPath.Text = Program.currentDir + "/MitmDNS" + exeExtension;
            textBoxHorizonPath.Text = Program.currentDir + "/Horizon" + exeExtension;
            textBoxMultisocksPath.Text = Program.currentDir + "/MultiSocks" + exeExtension;
            textBoxMultispyPath.Text = Program.currentDir + "/MultiSpy" + exeExtension;
            textBoxQuazalserverPath.Text = Program.currentDir + "/QuazalServer" + exeExtension;
            textBoxSSFWServerPath.Text = Program.currentDir + "/SSFWServer" + exeExtension;
            textBoxSVOPath.Text = Program.currentDir + "/SVO" + exeExtension;
            textBoxEdenserverPath.Text = Program.currentDir + "/EdenServer" + exeExtension;

            _writersList.Add(httpCRC, new ControlWriter(richTextBoxHTTPLog));
            _writersList.Add(httpsCRC, new ControlWriter(richTextBoxHTTPSLog));
            _writersList.Add(dnsCRC, new ControlWriter(richTextBoxDNSLog));
            _writersList.Add(horizonCRC, new ControlWriter(richTextBoxHorizonLog));
            _writersList.Add(multisocksCRC, new ControlWriter(richTextBoxMultisocksLog));
            _writersList.Add(multispyCRC, new ControlWriter(richTextBoxMultispyLog));
            _writersList.Add(quazalCRC, new ControlWriter(richTextBoxQuazalserverLog));
            _writersList.Add(ssfwCRC, new ControlWriter(richTextBoxSSFWServerLog));
            _writersList.Add(svoCRC, new ControlWriter(richTextBoxSVOLog));
            _writersList.Add(edenCRC, new ControlWriter(richTextBoxEdenserverLog));

            richTextBoxInformation.Text = $"Remote Control started at: {Program.timeStarted}\n";

            Console.SetOut(new MultiTextWriter(new ControlWriter(richTextBoxInformation), Console.Out));

            // Attach the event handler to the FormClosing event
            FormClosing += MainForm_FormClosing;
        }

        private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            DialogResult result = MessageBox.Show("Do you really want to close the application?\nShutdown can take a little while if servers are running.", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            // If the user clicks "No", cancel the form closing
            if (result == DialogResult.No)
                e.Cancel = true;
            else
            {
                Console.SetOut(TextWriter.Null); // Avoids a race-condition when stalling apps try to write the info to the disposed richtextbox.

                Parallel.ForEach(ProcessManager.Processes.Keys, guid =>
                {
                    try
                    {
                        ProcessManager.ShutdownProcess(guid);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, $"Application with guid:{guid} thrown an exception while being closed.");
                    }
                });
            }
        }

        private void buttonBrowseHTTPPath_Click(object sender, EventArgs e)
        {
            Utils.OpenExecutableFile(textBoxHTTPPath);
        }

        private void buttonBrowseHTTPSPath_Click(object sender, EventArgs e)
        {
            Utils.OpenExecutableFile(textBoxHTTPSPath);
        }

        private void buttonBrowseDNSPath_Click(object sender, EventArgs e)
        {
            Utils.OpenExecutableFile(textBoxDNSPath);
        }

        private void buttonBrowseHorizonPath_Click(object sender, EventArgs e)
        {
            Utils.OpenExecutableFile(textBoxHorizonPath);
        }

        private void buttonBrowseMultisocksPath_Click(object sender, EventArgs e)
        {
            Utils.OpenExecutableFile(textBoxMultisocksPath);
        }

        private void buttonBrowseMultispyPath_Click(object sender, EventArgs e)
        {
            Utils.OpenExecutableFile(textBoxMultispyPath);
        }

        private void buttonBrowseQuazalserverPath_Click(object sender, EventArgs e)
        {
            Utils.OpenExecutableFile(textBoxQuazalserverPath);
        }

        private void buttonBrowseSSFWServerPath_Click(object sender, EventArgs e)
        {
            Utils.OpenExecutableFile(textBoxSSFWServerPath);
        }

        private void buttonBrowseSVOPath_Click(object sender, EventArgs e)
        {
            Utils.OpenExecutableFile(textBoxSVOPath);
        }

        private void buttonBrowseEdenserverPath_Click(object sender, EventArgs e)
        {
            Utils.OpenExecutableFile(textBoxEdenserverPath);
        }

        private void buttonStartHTTP_Click(object sender, EventArgs e)
        {
            _ = Task.Run(() =>
            {
                string prefix = "HTTP";

                try
                {
                    _writersList[httpCRC].Flush();
                    ProcessManager.StartupProgram(_writersList[httpCRC], textBoxHTTP, groupBoxHTTP, prefix, textBoxHTTPPath.Text, httpCRC);
                    textBoxHTTP.Invoke(new Action(() =>
                    {
                        textBoxHTTP.Text = "Running";
                    }));
                    groupBoxHTTP.Invoke(new Action(() =>
                    {
                        groupBoxHTTP.BackColor = Color.Green;
                    }));
                    Console.WriteLine($"[{prefix}] - Server started at: {DateTime.Now}!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{prefix}] - An assertion was thrown while starting the server! (Exception: {ex})");
                    textBoxHTTP.Invoke(new Action(() =>
                    {
                        textBoxHTTP.Text = "Stopped";
                    }));
                    groupBoxHTTP.Invoke(new Action(() =>
                    {
                        groupBoxHTTP.BackColor = Color.Red;
                    }));
                }
            });
        }

        private void buttonStartHTTPS_Click(object sender, EventArgs e)
        {
            _ = Task.Run(() =>
            {
                string prefix = "HTTPS";

                try
                {
                    _writersList[httpsCRC].Flush();
                    ProcessManager.StartupProgram(_writersList[httpsCRC], textBoxHTTPS, groupBoxHTTPS, prefix, textBoxHTTPSPath.Text, httpsCRC);
                    textBoxHTTPS.Invoke(new Action(() =>
                    {
                        textBoxHTTPS.Text = "Running";
                    }));
                    groupBoxHTTPS.Invoke(new Action(() =>
                    {
                        groupBoxHTTPS.BackColor = Color.Green;
                    }));
                    Console.WriteLine($"[{prefix}] - Server started at: {DateTime.Now}!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{prefix}] - An assertion was thrown while starting the server! (Exception: {ex})");
                    textBoxHTTPS.Invoke(new Action(() =>
                    {
                        textBoxHTTPS.Text = "Stopped";
                    }));
                    groupBoxHTTPS.Invoke(new Action(() =>
                    {
                        groupBoxHTTPS.BackColor = Color.Red;
                    }));
                }
            });
        }

        private void buttonStartDNS_Click(object sender, EventArgs e)
        {
            _ = Task.Run(() =>
            {
                string prefix = "MitmDNS";

                try
                {
                    _writersList[dnsCRC].Flush();
                    ProcessManager.StartupProgram(_writersList[dnsCRC], textBoxDNS, groupBoxDNS, prefix, textBoxDNSPath.Text, dnsCRC);
                    textBoxDNS.Invoke(new Action(() =>
                    {
                        textBoxDNS.Text = "Running";
                    }));
                    groupBoxDNS.Invoke(new Action(() =>
                    {
                        groupBoxDNS.BackColor = Color.Green;
                    }));
                    Console.WriteLine($"[{prefix}] - Server started at: {DateTime.Now}!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{prefix}] - An assertion was thrown while starting the server! (Exception: {ex})");
                    textBoxDNS.Invoke(new Action(() =>
                    {
                        textBoxDNS.Text = "Stopped";
                    }));
                    groupBoxDNS.Invoke(new Action(() =>
                    {
                        groupBoxDNS.BackColor = Color.Red;
                    }));
                }
            });
        }

        private void buttonStartHorizon_Click(object sender, EventArgs e)
        {
            _ = Task.Run(() =>
            {
                string prefix = "Horizon";

                try
                {
                    _writersList[horizonCRC].Flush();
                    ProcessManager.StartupProgram(_writersList[horizonCRC], textBoxHorizon, groupBoxHorizon, prefix, textBoxHorizonPath.Text, horizonCRC);
                    textBoxHorizon.Invoke(new Action(() =>
                    {
                        textBoxHorizon.Text = "Running";
                    }));
                    groupBoxHorizon.Invoke(new Action(() =>
                    {
                        groupBoxHorizon.BackColor = Color.Green;
                    }));
                    Console.WriteLine($"[{prefix}] - Server started at: {DateTime.Now}!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{prefix}] - An assertion was thrown while starting the server! (Exception: {ex})");
                    textBoxHorizon.Invoke(new Action(() =>
                    {
                        textBoxHorizon.Text = "Stopped";
                    }));
                    groupBoxHorizon.Invoke(new Action(() =>
                    {
                        groupBoxHorizon.BackColor = Color.Red;
                    }));
                }
            });
        }

        private void buttonStartMultisocks_Click(object sender, EventArgs e)
        {
            _ = Task.Run(() =>
            {
                string prefix = "MultiSocks";

                try
                {
                    _writersList[multisocksCRC].Flush();
                    ProcessManager.StartupProgram(_writersList[multisocksCRC], textBoxMultisocks, groupBoxMultisocks, prefix, textBoxMultisocksPath.Text, multisocksCRC);
                    textBoxMultisocks.Invoke(new Action(() =>
                    {
                        textBoxMultisocks.Text = "Running";
                    }));
                    groupBoxMultisocks.Invoke(new Action(() =>
                    {
                        groupBoxMultisocks.BackColor = Color.Green;
                    }));
                    Console.WriteLine($"[{prefix}] - Server started at: {DateTime.Now}!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{prefix}] - An assertion was thrown while starting the server! (Exception: {ex})");
                    textBoxMultisocks.Invoke(new Action(() =>
                    {
                        textBoxMultisocks.Text = "Stopped";
                    }));
                    groupBoxMultisocks.Invoke(new Action(() =>
                    {
                        groupBoxMultisocks.BackColor = Color.Red;
                    }));
                }
            });
        }

        private void buttonStartMultispy_Click(object sender, EventArgs e)
        {
            _ = Task.Run(() =>
            {
                string prefix = "MultiSspy";

                try
                {
                    _writersList[multispyCRC].Flush();
                    ProcessManager.StartupProgram(_writersList[multispyCRC], textBoxMultispy, groupBoxMultispy, prefix, textBoxMultispyPath.Text, multispyCRC);
                    textBoxMultispy.Invoke(new Action(() =>
                    {
                        textBoxMultispy.Text = "Running";
                    }));
                    groupBoxMultispy.Invoke(new Action(() =>
                    {
                        groupBoxMultispy.BackColor = Color.Green;
                    }));
                    Console.WriteLine($"[{prefix}] - Server started at: {DateTime.Now}!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{prefix}] - An assertion was thrown while starting the server! (Exception: {ex})");
                    textBoxMultispy.Invoke(new Action(() =>
                    {
                        textBoxMultispy.Text = "Stopped";
                    }));
                    groupBoxMultispy.Invoke(new Action(() =>
                    {
                        groupBoxMultispy.BackColor = Color.Red;
                    }));
                }
            });
        }

        private void buttonStartQuazalserver_Click(object sender, EventArgs e)
        {
            _ = Task.Run(() =>
            {
                string prefix = "QuazalServer";

                try
                {
                    _writersList[quazalCRC].Flush();
                    ProcessManager.StartupProgram(_writersList[quazalCRC], textBoxQuazalserver, groupBoxQuazalserver, prefix, textBoxQuazalserverPath.Text, quazalCRC);
                    textBoxQuazalserver.Invoke(new Action(() =>
                    {
                        textBoxQuazalserver.Text = "Running";
                    }));
                    groupBoxQuazalserver.Invoke(new Action(() =>
                    {
                        groupBoxQuazalserver.BackColor = Color.Green;
                    }));
                    Console.WriteLine($"[{prefix}] - Server started at: {DateTime.Now}!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{prefix}] - An assertion was thrown while starting the server! (Exception: {ex})");
                    textBoxQuazalserver.Invoke(new Action(() =>
                    {
                        textBoxQuazalserver.Text = "Stopped";
                    }));
                    groupBoxQuazalserver.Invoke(new Action(() =>
                    {
                        groupBoxQuazalserver.BackColor = Color.Red;
                    }));
                }
            });
        }

        private void buttonStartSSFWServer_Click(object sender, EventArgs e)
        {
            _ = Task.Run(() =>
            {
                string prefix = "SSFWServer";

                try
                {
                    _writersList[ssfwCRC].Flush();
                    ProcessManager.StartupProgram(_writersList[ssfwCRC], textBoxSSFWServer, groupBoxSSFWServer, prefix, textBoxSSFWServerPath.Text, ssfwCRC);
                    textBoxSSFWServer.Invoke(new Action(() =>
                    {
                        textBoxSSFWServer.Text = "Running";
                    }));
                    groupBoxSSFWServer.Invoke(new Action(() =>
                    {
                        groupBoxSSFWServer.BackColor = Color.Green;
                    }));
                    Console.WriteLine($"[{prefix}] - Server started at: {DateTime.Now}!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{prefix}] - An assertion was thrown while starting the server! (Exception: {ex})");
                    textBoxSSFWServer.Invoke(new Action(() =>
                    {
                        textBoxSSFWServer.Text = "Stopped";
                    }));
                    groupBoxSSFWServer.Invoke(new Action(() =>
                    {
                        groupBoxSSFWServer.BackColor = Color.Red;
                    }));
                }
            });
        }

        private void buttonStartSVO_Click(object sender, EventArgs e)
        {
            _ = Task.Run(() =>
            {
                string prefix = "SVO";

                try
                {
                    _writersList[svoCRC].Flush();
                    ProcessManager.StartupProgram(_writersList[svoCRC], textBoxSVO, groupBoxSVO, prefix, textBoxSVOPath.Text, svoCRC);
                    textBoxSVO.Invoke(new Action(() =>
                    {
                        textBoxSVO.Text = "Running";
                    }));
                    groupBoxSVO.Invoke(new Action(() =>
                    {
                        groupBoxSVO.BackColor = Color.Green;
                    }));
                    Console.WriteLine($"[{prefix}] - Server started at: {DateTime.Now}!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{prefix}] - An assertion was thrown while starting the server! (Exception: {ex})");
                    textBoxSVO.Invoke(new Action(() =>
                    {
                        textBoxSVO.Text = "Stopped";
                    }));
                    groupBoxSVO.Invoke(new Action(() =>
                    {
                        groupBoxSVO.BackColor = Color.Red;
                    }));
                }
            });
        }

        private void buttonStartEdenserver_Click(object sender, EventArgs e)
        {
            _ = Task.Run(() =>
            {
                string prefix = "EdenServer";

                try
                {
                    _writersList[edenCRC].Flush();
                    ProcessManager.StartupProgram(_writersList[edenCRC], textBoxEdenserver, groupBoxEdenserver, prefix, textBoxEdenserverPath.Text, edenCRC);
                    textBoxEdenserver.Invoke(new Action(() =>
                    {
                        textBoxEdenserver.Text = "Running";
                    }));
                    groupBoxEdenserver.Invoke(new Action(() =>
                    {
                        groupBoxEdenserver.BackColor = Color.Green;
                    }));
                    Console.WriteLine($"[{prefix}] - Server started at: {DateTime.Now}!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{prefix}] - An assertion was thrown while starting the server! (Exception: {ex})");
                    textBoxEdenserver.Invoke(new Action(() =>
                    {
                        textBoxEdenserver.Text = "Stopped";
                    }));
                    groupBoxEdenserver.Invoke(new Action(() =>
                    {
                        groupBoxEdenserver.BackColor = Color.Red;
                    }));
                }
            });
        }

        private void buttonStopHTTP_Click(object sender, EventArgs e)
        {
            if (ProcessManager.ShutdownProcess(httpCRC))
            {
                textBoxHTTP.Invoke(new Action(() =>
                {
                    textBoxHTTP.Text = "Stopped";
                }));
                groupBoxHTTP.Invoke(new Action(() =>
                {
                    groupBoxHTTP.BackColor = Color.Red;
                }));
                Console.WriteLine($"[HTTP] - Server stopped at: {DateTime.Now}!");
            }
            else
                Utils.ShowNoProcessMessageBox();
        }

        private void buttonStopHTTPS_Click(object sender, EventArgs e)
        {
            if (ProcessManager.ShutdownProcess(httpsCRC))
            {
                textBoxHTTPS.Invoke(new Action(() =>
                {
                    textBoxHTTPS.Text = "Stopped";
                }));
                groupBoxHTTPS.Invoke(new Action(() =>
                {
                    groupBoxHTTPS.BackColor = Color.Red;
                }));
                Console.WriteLine($"[HTTPS] - Server stopped at: {DateTime.Now}!");
            }
            else
                Utils.ShowNoProcessMessageBox();
        }

        private void buttonStopDNS_Click(object sender, EventArgs e)
        {
            if (ProcessManager.ShutdownProcess(dnsCRC))
            {
                textBoxDNS.Invoke(new Action(() =>
                {
                    textBoxDNS.Text = "Stopped";
                }));
                groupBoxDNS.Invoke(new Action(() =>
                {
                    groupBoxDNS.BackColor = Color.Red;
                }));
                Console.WriteLine($"[MitmDNS] - Server stopped at: {DateTime.Now}!");
            }
            else
                Utils.ShowNoProcessMessageBox();
        }

        private void buttonStopHorizon_Click(object sender, EventArgs e)
        {
            if (ProcessManager.ShutdownProcess(horizonCRC))
            {
                textBoxHorizon.Invoke(new Action(() =>
                {
                    textBoxHorizon.Text = "Stopped";
                }));
                groupBoxHorizon.Invoke(new Action(() =>
                {
                    groupBoxHorizon.BackColor = Color.Red;
                }));
                Console.WriteLine($"[Horizon] - Server stopped at: {DateTime.Now}!");
            }
            else
                Utils.ShowNoProcessMessageBox();
        }

        private void buttonStopMultisocks_Click(object sender, EventArgs e)
        {
            if (ProcessManager.ShutdownProcess(multisocksCRC))
            {
                textBoxMultisocks.Invoke(new Action(() =>
                {
                    textBoxMultisocks.Text = "Stopped";
                }));
                groupBoxMultisocks.Invoke(new Action(() =>
                {
                    groupBoxMultisocks.BackColor = Color.Red;
                }));
                Console.WriteLine($"[MultiSocks] - Server stopped at: {DateTime.Now}!");
            }
            else
                Utils.ShowNoProcessMessageBox();
        }

        private void buttonStopMultispy_Click(object sender, EventArgs e)
        {
            if (ProcessManager.ShutdownProcess(multispyCRC))
            {
                textBoxMultispy.Invoke(new Action(() =>
                {
                    textBoxMultispy.Text = "Stopped";
                }));
                groupBoxMultispy.Invoke(new Action(() =>
                {
                    groupBoxMultispy.BackColor = Color.Red;
                }));
                Console.WriteLine($"[MultiSpy] - Server stopped at: {DateTime.Now}!");
            }
            else
                Utils.ShowNoProcessMessageBox();
        }

        private void buttonStopQuazalserver_Click(object sender, EventArgs e)
        {
            if (ProcessManager.ShutdownProcess(quazalCRC))
            {
                textBoxQuazalserver.Invoke(new Action(() =>
                {
                    textBoxQuazalserver.Text = "Stopped";
                }));
                groupBoxQuazalserver.Invoke(new Action(() =>
                {
                    groupBoxQuazalserver.BackColor = Color.Red;
                }));
                Console.WriteLine($"[QuazalServer] - Server stopped at: {DateTime.Now}!");
            }
            else
                Utils.ShowNoProcessMessageBox();
        }

        private void buttonStopSSFWServer_Click(object sender, EventArgs e)
        {
            if (ProcessManager.ShutdownProcess(ssfwCRC))
            {
                textBoxSSFWServer.Invoke(new Action(() =>
                {
                    textBoxSSFWServer.Text = "Stopped";
                }));
                groupBoxSSFWServer.Invoke(new Action(() =>
                {
                    groupBoxSSFWServer.BackColor = Color.Red;
                }));
                Console.WriteLine($"[SSFWServer] - Server stopped at: {DateTime.Now}!");
            }
            else
                Utils.ShowNoProcessMessageBox();
        }

        private void buttonStopSVO_Click(object sender, EventArgs e)
        {
            if (ProcessManager.ShutdownProcess(svoCRC))
            {
                textBoxSVO.Invoke(new Action(() =>
                {
                    textBoxSVO.Text = "Stopped";
                }));
                groupBoxSVO.Invoke(new Action(() =>
                {
                    groupBoxSVO.BackColor = Color.Red;
                }));
                Console.WriteLine($"[SVO] - Server stopped at: {DateTime.Now}!");
            }
            else
                Utils.ShowNoProcessMessageBox();
        }

        private void buttonStopEdenserver_Click(object sender, EventArgs e)
        {
            if (ProcessManager.ShutdownProcess(edenCRC))
            {
                textBoxEdenserver.Invoke(new Action(() =>
                {
                    textBoxEdenserver.Text = "Stopped";
                }));
                groupBoxEdenserver.Invoke(new Action(() =>
                {
                    groupBoxEdenserver.BackColor = Color.Red;
                }));
                Console.WriteLine($"[EdenServer] - Server stopped at: {DateTime.Now}!");
            }
            else
                Utils.ShowNoProcessMessageBox();
        }
    }
}
