namespace GraphicalUserInterface
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            richTextBoxLog = new RichTextBox();
            groupBoxWebServerManagement = new GroupBox();
            buttonStopTycoon = new Button();
            buttonStartTycoon = new Button();
            labelAdministratorRequired = new Label();
            buttonStopSVO = new Button();
            buttonStopSSFW = new Button();
            buttonStopHTTP = new Button();
            buttonStopHTTPS = new Button();
            buttonStartHTTPS = new Button();
            buttonStartSVO = new Button();
            buttonStartSSFW = new Button();
            buttonStartHTTP = new Button();
            buttonStartDNS = new Button();
            groupBoxAuxiliaryServerManagement = new GroupBox();
            buttonStopQuazal = new Button();
            buttonStartQuazal = new Button();
            buttonStopSRVEmu = new Button();
            buttonStartSRVEmu = new Button();
            buttonStopMultiSpy = new Button();
            buttonStartMultiSpy = new Button();
            buttonStopDNS = new Button();
            buttonStopHorizon = new Button();
            buttonStartHorizon = new Button();
            pictureBoxPSMSImage = new PictureBox();
            tableLayoutPanelMain = new TableLayoutPanel();
            groupBoxWebServerManagement.SuspendLayout();
            groupBoxAuxiliaryServerManagement.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxPSMSImage).BeginInit();
            tableLayoutPanelMain.SuspendLayout();
            SuspendLayout();
            // 
            // richTextBoxLog
            // 
            richTextBoxLog.Dock = DockStyle.Bottom;
            richTextBoxLog.Location = new Point(0, 567);
            richTextBoxLog.Margin = new Padding(3, 2, 3, 2);
            richTextBoxLog.Name = "richTextBoxLog";
            richTextBoxLog.Size = new Size(883, 175);
            richTextBoxLog.TabIndex = 0;
            richTextBoxLog.Text = "";
            // 
            // groupBoxWebServerManagement
            // 
            groupBoxWebServerManagement.Anchor = AnchorStyles.Top;
            groupBoxWebServerManagement.Controls.Add(buttonStopTycoon);
            groupBoxWebServerManagement.Controls.Add(buttonStartTycoon);
            groupBoxWebServerManagement.Controls.Add(labelAdministratorRequired);
            groupBoxWebServerManagement.Controls.Add(buttonStopSVO);
            groupBoxWebServerManagement.Controls.Add(buttonStopSSFW);
            groupBoxWebServerManagement.Controls.Add(buttonStopHTTP);
            groupBoxWebServerManagement.Controls.Add(buttonStopHTTPS);
            groupBoxWebServerManagement.Controls.Add(buttonStartHTTPS);
            groupBoxWebServerManagement.Controls.Add(buttonStartSVO);
            groupBoxWebServerManagement.Controls.Add(buttonStartSSFW);
            groupBoxWebServerManagement.Controls.Add(buttonStartHTTP);
            groupBoxWebServerManagement.Location = new Point(10, 364);
            groupBoxWebServerManagement.Margin = new Padding(3, 2, 3, 2);
            groupBoxWebServerManagement.Name = "groupBoxWebServerManagement";
            groupBoxWebServerManagement.Padding = new Padding(3, 2, 3, 2);
            groupBoxWebServerManagement.Size = new Size(425, 189);
            groupBoxWebServerManagement.TabIndex = 1;
            groupBoxWebServerManagement.TabStop = false;
            groupBoxWebServerManagement.Text = "Web Server Management";
            // 
            // buttonStopTycoon
            // 
            buttonStopTycoon.Location = new Point(145, 110);
            buttonStopTycoon.Margin = new Padding(3, 2, 3, 2);
            buttonStopTycoon.Name = "buttonStopTycoon";
            buttonStopTycoon.Size = new Size(93, 22);
            buttonStopTycoon.TabIndex = 9;
            buttonStopTycoon.Text = "Stop Tycoon";
            buttonStopTycoon.UseVisualStyleBackColor = true;
            buttonStopTycoon.Click += buttonStopTycoon_Click;
            // 
            // buttonStartTycoon
            // 
            buttonStartTycoon.Location = new Point(29, 110);
            buttonStartTycoon.Margin = new Padding(3, 2, 3, 2);
            buttonStartTycoon.Name = "buttonStartTycoon";
            buttonStartTycoon.Size = new Size(100, 22);
            buttonStartTycoon.TabIndex = 8;
            buttonStartTycoon.Text = "Start Tycoon";
            buttonStartTycoon.UseVisualStyleBackColor = true;
            buttonStartTycoon.Click += buttonStartTycoon_Click;
            // 
            // labelAdministratorRequired
            // 
            labelAdministratorRequired.AutoSize = true;
            labelAdministratorRequired.Location = new Point(238, 139);
            labelAdministratorRequired.Name = "labelAdministratorRequired";
            labelAdministratorRequired.Size = new Size(170, 15);
            labelAdministratorRequired.TabIndex = 7;
            labelAdministratorRequired.Text = "- Admin Required on Windows";
            // 
            // buttonStopSVO
            // 
            buttonStopSVO.Location = new Point(145, 136);
            buttonStopSVO.Margin = new Padding(3, 2, 3, 2);
            buttonStopSVO.Name = "buttonStopSVO";
            buttonStopSVO.Size = new Size(93, 22);
            buttonStopSVO.TabIndex = 6;
            buttonStopSVO.Text = "Stop SVO";
            buttonStopSVO.UseVisualStyleBackColor = true;
            buttonStopSVO.Click += buttonStopSVO_Click;
            // 
            // buttonStopSSFW
            // 
            buttonStopSSFW.Location = new Point(145, 83);
            buttonStopSSFW.Margin = new Padding(3, 2, 3, 2);
            buttonStopSSFW.Name = "buttonStopSSFW";
            buttonStopSSFW.Size = new Size(93, 22);
            buttonStopSSFW.TabIndex = 5;
            buttonStopSSFW.Text = "Stop SSFW";
            buttonStopSSFW.UseVisualStyleBackColor = true;
            buttonStopSSFW.Click += buttonStopSSFW_Click;
            // 
            // buttonStopHTTP
            // 
            buttonStopHTTP.Location = new Point(145, 57);
            buttonStopHTTP.Margin = new Padding(3, 2, 3, 2);
            buttonStopHTTP.Name = "buttonStopHTTP";
            buttonStopHTTP.Size = new Size(93, 22);
            buttonStopHTTP.TabIndex = 4;
            buttonStopHTTP.Text = "Stop HTTP";
            buttonStopHTTP.UseVisualStyleBackColor = true;
            buttonStopHTTP.Click += buttonStopHTTP_Click;
            // 
            // buttonStopHTTPS
            // 
            buttonStopHTTPS.Location = new Point(145, 31);
            buttonStopHTTPS.Margin = new Padding(3, 2, 3, 2);
            buttonStopHTTPS.Name = "buttonStopHTTPS";
            buttonStopHTTPS.Size = new Size(93, 22);
            buttonStopHTTPS.TabIndex = 3;
            buttonStopHTTPS.Text = "Stop HTTPS";
            buttonStopHTTPS.UseVisualStyleBackColor = true;
            buttonStopHTTPS.Click += buttonStopHTTPS_Click;
            // 
            // buttonStartHTTPS
            // 
            buttonStartHTTPS.Location = new Point(29, 31);
            buttonStartHTTPS.Margin = new Padding(3, 2, 3, 2);
            buttonStartHTTPS.Name = "buttonStartHTTPS";
            buttonStartHTTPS.Size = new Size(100, 22);
            buttonStartHTTPS.TabIndex = 2;
            buttonStartHTTPS.Text = "Start HTTPS";
            buttonStartHTTPS.UseVisualStyleBackColor = true;
            buttonStartHTTPS.Click += buttonStartHTTPS_Click;
            // 
            // buttonStartSVO
            // 
            buttonStartSVO.Location = new Point(29, 136);
            buttonStartSVO.Margin = new Padding(3, 2, 3, 2);
            buttonStartSVO.Name = "buttonStartSVO";
            buttonStartSVO.Size = new Size(100, 22);
            buttonStartSVO.TabIndex = 0;
            buttonStartSVO.Text = "Start SVO";
            buttonStartSVO.UseVisualStyleBackColor = true;
            buttonStartSVO.Click += buttonStartSVO_Click;
            // 
            // buttonStartSSFW
            // 
            buttonStartSSFW.Location = new Point(29, 83);
            buttonStartSSFW.Margin = new Padding(3, 2, 3, 2);
            buttonStartSSFW.Name = "buttonStartSSFW";
            buttonStartSSFW.Size = new Size(100, 22);
            buttonStartSSFW.TabIndex = 1;
            buttonStartSSFW.Text = "Start SSFW";
            buttonStartSSFW.UseVisualStyleBackColor = true;
            buttonStartSSFW.Click += buttonStartSSFW_Click;
            // 
            // buttonStartHTTP
            // 
            buttonStartHTTP.Location = new Point(29, 57);
            buttonStartHTTP.Margin = new Padding(3, 2, 3, 2);
            buttonStartHTTP.Name = "buttonStartHTTP";
            buttonStartHTTP.Size = new Size(100, 22);
            buttonStartHTTP.TabIndex = 1;
            buttonStartHTTP.Text = "Start HTTP";
            buttonStartHTTP.UseVisualStyleBackColor = true;
            buttonStartHTTP.Click += buttonStartHTTP_Click;
            // 
            // buttonStartDNS
            // 
            buttonStartDNS.Location = new Point(29, 57);
            buttonStartDNS.Margin = new Padding(3, 2, 3, 2);
            buttonStartDNS.Name = "buttonStartDNS";
            buttonStartDNS.Size = new Size(112, 22);
            buttonStartDNS.TabIndex = 0;
            buttonStartDNS.Text = "Start DNS";
            buttonStartDNS.UseVisualStyleBackColor = true;
            buttonStartDNS.Click += buttonStartDNS_Click;
            // 
            // groupBoxAuxiliaryServerManagement
            // 
            groupBoxAuxiliaryServerManagement.Anchor = AnchorStyles.Top;
            groupBoxAuxiliaryServerManagement.Controls.Add(buttonStopQuazal);
            groupBoxAuxiliaryServerManagement.Controls.Add(buttonStartQuazal);
            groupBoxAuxiliaryServerManagement.Controls.Add(buttonStopSRVEmu);
            groupBoxAuxiliaryServerManagement.Controls.Add(buttonStartSRVEmu);
            groupBoxAuxiliaryServerManagement.Controls.Add(buttonStopMultiSpy);
            groupBoxAuxiliaryServerManagement.Controls.Add(buttonStartMultiSpy);
            groupBoxAuxiliaryServerManagement.Controls.Add(buttonStopDNS);
            groupBoxAuxiliaryServerManagement.Controls.Add(buttonStopHorizon);
            groupBoxAuxiliaryServerManagement.Controls.Add(buttonStartHorizon);
            groupBoxAuxiliaryServerManagement.Controls.Add(buttonStartDNS);
            groupBoxAuxiliaryServerManagement.Location = new Point(445, 2);
            groupBoxAuxiliaryServerManagement.Margin = new Padding(3, 2, 3, 2);
            groupBoxAuxiliaryServerManagement.Name = "groupBoxAuxiliaryServerManagement";
            groupBoxAuxiliaryServerManagement.Padding = new Padding(3, 2, 3, 2);
            groupBoxAuxiliaryServerManagement.Size = new Size(430, 189);
            groupBoxAuxiliaryServerManagement.TabIndex = 2;
            groupBoxAuxiliaryServerManagement.TabStop = false;
            groupBoxAuxiliaryServerManagement.Text = "Auxiliary Server Management";
            // 
            // buttonStopQuazal
            // 
            buttonStopQuazal.Location = new Point(158, 135);
            buttonStopQuazal.Margin = new Padding(3, 2, 3, 2);
            buttonStopQuazal.Name = "buttonStopQuazal";
            buttonStopQuazal.Size = new Size(108, 22);
            buttonStopQuazal.TabIndex = 15;
            buttonStopQuazal.Text = "Stop Quazal";
            buttonStopQuazal.UseVisualStyleBackColor = true;
            buttonStopQuazal.Click += buttonStopQuazal_Click;
            // 
            // buttonStartQuazal
            // 
            buttonStartQuazal.Location = new Point(29, 135);
            buttonStartQuazal.Margin = new Padding(3, 2, 3, 2);
            buttonStartQuazal.Name = "buttonStartQuazal";
            buttonStartQuazal.Size = new Size(112, 22);
            buttonStartQuazal.TabIndex = 14;
            buttonStartQuazal.Text = "Start Quazal";
            buttonStartQuazal.UseVisualStyleBackColor = true;
            buttonStartQuazal.Click += buttonStartQuazal_Click;
            // 
            // buttonStopSRVEmu
            // 
            buttonStopSRVEmu.Location = new Point(158, 109);
            buttonStopSRVEmu.Margin = new Padding(3, 2, 3, 2);
            buttonStopSRVEmu.Name = "buttonStopSRVEmu";
            buttonStopSRVEmu.Size = new Size(108, 22);
            buttonStopSRVEmu.TabIndex = 13;
            buttonStopSRVEmu.Text = "Stop SRVEmu";
            buttonStopSRVEmu.UseVisualStyleBackColor = true;
            buttonStopSRVEmu.Click += buttonStopSRVEmu_Click;
            // 
            // buttonStartSRVEmu
            // 
            buttonStartSRVEmu.Location = new Point(29, 109);
            buttonStartSRVEmu.Margin = new Padding(3, 2, 3, 2);
            buttonStartSRVEmu.Name = "buttonStartSRVEmu";
            buttonStartSRVEmu.Size = new Size(112, 22);
            buttonStartSRVEmu.TabIndex = 12;
            buttonStartSRVEmu.Text = "Start SRVEmu";
            buttonStartSRVEmu.UseVisualStyleBackColor = true;
            buttonStartSRVEmu.Click += buttonStartSRVEmu_Click;
            // 
            // buttonStopMultiSpy
            // 
            buttonStopMultiSpy.Location = new Point(158, 83);
            buttonStopMultiSpy.Margin = new Padding(3, 2, 3, 2);
            buttonStopMultiSpy.Name = "buttonStopMultiSpy";
            buttonStopMultiSpy.Size = new Size(108, 22);
            buttonStopMultiSpy.TabIndex = 11;
            buttonStopMultiSpy.Text = "Stop MultiSpy";
            buttonStopMultiSpy.UseVisualStyleBackColor = true;
            buttonStopMultiSpy.Click += buttonStopMultiSpy_Click;
            // 
            // buttonStartMultiSpy
            // 
            buttonStartMultiSpy.Location = new Point(29, 83);
            buttonStartMultiSpy.Margin = new Padding(3, 2, 3, 2);
            buttonStartMultiSpy.Name = "buttonStartMultiSpy";
            buttonStartMultiSpy.Size = new Size(112, 22);
            buttonStartMultiSpy.TabIndex = 10;
            buttonStartMultiSpy.Text = "Start MultiSpy";
            buttonStartMultiSpy.UseVisualStyleBackColor = true;
            buttonStartMultiSpy.Click += buttonStartMultiSpy_Click;
            // 
            // buttonStopDNS
            // 
            buttonStopDNS.Location = new Point(158, 57);
            buttonStopDNS.Margin = new Padding(3, 2, 3, 2);
            buttonStopDNS.Name = "buttonStopDNS";
            buttonStopDNS.Size = new Size(108, 22);
            buttonStopDNS.TabIndex = 7;
            buttonStopDNS.Text = "Stop DNS";
            buttonStopDNS.UseVisualStyleBackColor = true;
            buttonStopDNS.Click += buttonStopDNS_Click;
            // 
            // buttonStopHorizon
            // 
            buttonStopHorizon.Location = new Point(158, 31);
            buttonStopHorizon.Margin = new Padding(3, 2, 3, 2);
            buttonStopHorizon.Name = "buttonStopHorizon";
            buttonStopHorizon.Size = new Size(108, 22);
            buttonStopHorizon.TabIndex = 6;
            buttonStopHorizon.Text = "Stop Horizon";
            buttonStopHorizon.UseVisualStyleBackColor = true;
            buttonStopHorizon.Click += buttonStopHorizon_Click;
            // 
            // buttonStartHorizon
            // 
            buttonStartHorizon.Location = new Point(29, 31);
            buttonStartHorizon.Margin = new Padding(3, 2, 3, 2);
            buttonStartHorizon.Name = "buttonStartHorizon";
            buttonStartHorizon.Size = new Size(112, 22);
            buttonStartHorizon.TabIndex = 2;
            buttonStartHorizon.Text = "Start Horizon";
            buttonStartHorizon.UseVisualStyleBackColor = true;
            buttonStartHorizon.Click += buttonStartHorizon_Click;
            // 
            // pictureBoxPSMSImage
            // 
            pictureBoxPSMSImage.Dock = DockStyle.Top;
            pictureBoxPSMSImage.Image = Properties.Resources.multiserver2xplogo;
            pictureBoxPSMSImage.Location = new Point(0, 0);
            pictureBoxPSMSImage.Margin = new Padding(3, 2, 3, 2);
            pictureBoxPSMSImage.Name = "pictureBoxPSMSImage";
            pictureBoxPSMSImage.Size = new Size(883, 357);
            pictureBoxPSMSImage.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBoxPSMSImage.TabIndex = 3;
            pictureBoxPSMSImage.TabStop = false;
            // 
            // tableLayoutPanelMain
            // 
            tableLayoutPanelMain.Anchor = AnchorStyles.Top;
            tableLayoutPanelMain.AutoScroll = true;
            tableLayoutPanelMain.ColumnCount = 2;
            tableLayoutPanelMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanelMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanelMain.Controls.Add(groupBoxAuxiliaryServerManagement, 1, 0);
            tableLayoutPanelMain.Location = new Point(0, 362);
            tableLayoutPanelMain.Margin = new Padding(3, 2, 3, 2);
            tableLayoutPanelMain.Name = "tableLayoutPanelMain";
            tableLayoutPanelMain.RowCount = 1;
            tableLayoutPanelMain.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanelMain.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanelMain.Size = new Size(881, 196);
            tableLayoutPanelMain.TabIndex = 4;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(883, 742);
            Controls.Add(groupBoxWebServerManagement);
            Controls.Add(tableLayoutPanelMain);
            Controls.Add(pictureBoxPSMSImage);
            Controls.Add(richTextBoxLog);
            DoubleBuffered = true;
            Margin = new Padding(3, 2, 3, 2);
            MaximizeBox = false;
            MinimumSize = new Size(899, 777);
            Name = "MainForm";
            Text = "MultiServer Graphical User Interface";
            groupBoxWebServerManagement.ResumeLayout(false);
            groupBoxWebServerManagement.PerformLayout();
            groupBoxAuxiliaryServerManagement.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBoxPSMSImage).EndInit();
            tableLayoutPanelMain.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private RichTextBox richTextBoxLog;
        private GroupBox groupBoxWebServerManagement;
        private Button buttonStartHTTPS;
        private Button buttonStartHTTP;
        private Button buttonStartDNS;
        private GroupBox groupBoxAuxiliaryServerManagement;
        private Button buttonStartHorizon;
        private Button buttonStartSSFW;
        private Button buttonStartSVO;
        private PictureBox pictureBoxPSMSImage;
        private Button buttonStopSVO;
        private Button buttonStopSSFW;
        private Button buttonStopHTTP;
        private Button buttonStopHTTPS;
        private Button buttonStopDNS;
        private Button buttonStopHorizon;
        private Label labelAdministratorRequired;
        private Button buttonStopTycoon;
        private Button buttonStartTycoon;
        private TableLayoutPanel tableLayoutPanelMain;
        private Button buttonStopMultiSpy;
        private Button buttonStartMultiSpy;
        private Button buttonStopSRVEmu;
        private Button buttonStartSRVEmu;
        private Button buttonStopQuazal;
        private Button buttonStartQuazal;
    }
}
