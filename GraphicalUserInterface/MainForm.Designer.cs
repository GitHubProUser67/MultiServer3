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
            richTextBoxLog.Location = new Point(0, 757);
            richTextBoxLog.Name = "richTextBoxLog";
            richTextBoxLog.Size = new Size(1009, 232);
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
            groupBoxWebServerManagement.Location = new Point(11, 485);
            groupBoxWebServerManagement.Name = "groupBoxWebServerManagement";
            groupBoxWebServerManagement.Size = new Size(486, 252);
            groupBoxWebServerManagement.TabIndex = 1;
            groupBoxWebServerManagement.TabStop = false;
            groupBoxWebServerManagement.Text = "Web Server Management";
            // 
            // buttonStopTycoon
            // 
            buttonStopTycoon.Location = new Point(166, 147);
            buttonStopTycoon.Name = "buttonStopTycoon";
            buttonStopTycoon.Size = new Size(106, 29);
            buttonStopTycoon.TabIndex = 9;
            buttonStopTycoon.Text = "Stop Tycoon";
            buttonStopTycoon.UseVisualStyleBackColor = true;
            buttonStopTycoon.Click += buttonStopTycoon_Click;
            // 
            // buttonStartTycoon
            // 
            buttonStartTycoon.Location = new Point(33, 147);
            buttonStartTycoon.Name = "buttonStartTycoon";
            buttonStartTycoon.Size = new Size(114, 29);
            buttonStartTycoon.TabIndex = 8;
            buttonStartTycoon.Text = "Start Tycoon";
            buttonStartTycoon.UseVisualStyleBackColor = true;
            buttonStartTycoon.Click += buttonStartTycoon_Click;
            // 
            // labelAdministratorRequired
            // 
            labelAdministratorRequired.AutoSize = true;
            labelAdministratorRequired.Location = new Point(272, 185);
            labelAdministratorRequired.Name = "labelAdministratorRequired";
            labelAdministratorRequired.Size = new Size(213, 20);
            labelAdministratorRequired.TabIndex = 7;
            labelAdministratorRequired.Text = "- Admin Required on Windows";
            // 
            // buttonStopSVO
            // 
            buttonStopSVO.Location = new Point(166, 181);
            buttonStopSVO.Name = "buttonStopSVO";
            buttonStopSVO.Size = new Size(106, 29);
            buttonStopSVO.TabIndex = 6;
            buttonStopSVO.Text = "Stop SVO";
            buttonStopSVO.UseVisualStyleBackColor = true;
            buttonStopSVO.Click += buttonStopSVO_Click;
            // 
            // buttonStopSSFW
            // 
            buttonStopSSFW.Location = new Point(166, 111);
            buttonStopSSFW.Name = "buttonStopSSFW";
            buttonStopSSFW.Size = new Size(106, 29);
            buttonStopSSFW.TabIndex = 5;
            buttonStopSSFW.Text = "Stop SSFW";
            buttonStopSSFW.UseVisualStyleBackColor = true;
            buttonStopSSFW.Click += buttonStopSSFW_Click;
            // 
            // buttonStopHTTP
            // 
            buttonStopHTTP.Location = new Point(166, 76);
            buttonStopHTTP.Name = "buttonStopHTTP";
            buttonStopHTTP.Size = new Size(106, 29);
            buttonStopHTTP.TabIndex = 4;
            buttonStopHTTP.Text = "Stop HTTP";
            buttonStopHTTP.UseVisualStyleBackColor = true;
            buttonStopHTTP.Click += buttonStopHTTP_Click;
            // 
            // buttonStopHTTPS
            // 
            buttonStopHTTPS.Location = new Point(166, 41);
            buttonStopHTTPS.Name = "buttonStopHTTPS";
            buttonStopHTTPS.Size = new Size(106, 29);
            buttonStopHTTPS.TabIndex = 3;
            buttonStopHTTPS.Text = "Stop HTTPS";
            buttonStopHTTPS.UseVisualStyleBackColor = true;
            buttonStopHTTPS.Click += buttonStopHTTPS_Click;
            // 
            // buttonStartHTTPS
            // 
            buttonStartHTTPS.Location = new Point(33, 41);
            buttonStartHTTPS.Name = "buttonStartHTTPS";
            buttonStartHTTPS.Size = new Size(114, 29);
            buttonStartHTTPS.TabIndex = 2;
            buttonStartHTTPS.Text = "Start HTTPS";
            buttonStartHTTPS.UseVisualStyleBackColor = true;
            buttonStartHTTPS.Click += buttonStartHTTPS_Click;
            // 
            // buttonStartSVO
            // 
            buttonStartSVO.Location = new Point(33, 181);
            buttonStartSVO.Name = "buttonStartSVO";
            buttonStartSVO.Size = new Size(114, 29);
            buttonStartSVO.TabIndex = 0;
            buttonStartSVO.Text = "Start SVO";
            buttonStartSVO.UseVisualStyleBackColor = true;
            buttonStartSVO.Click += buttonStartSVO_Click;
            // 
            // buttonStartSSFW
            // 
            buttonStartSSFW.Location = new Point(33, 111);
            buttonStartSSFW.Name = "buttonStartSSFW";
            buttonStartSSFW.Size = new Size(114, 29);
            buttonStartSSFW.TabIndex = 1;
            buttonStartSSFW.Text = "Start SSFW";
            buttonStartSSFW.UseVisualStyleBackColor = true;
            buttonStartSSFW.Click += buttonStartSSFW_Click;
            // 
            // buttonStartHTTP
            // 
            buttonStartHTTP.Location = new Point(33, 76);
            buttonStartHTTP.Name = "buttonStartHTTP";
            buttonStartHTTP.Size = new Size(114, 29);
            buttonStartHTTP.TabIndex = 1;
            buttonStartHTTP.Text = "Start HTTP";
            buttonStartHTTP.UseVisualStyleBackColor = true;
            buttonStartHTTP.Click += buttonStartHTTP_Click;
            // 
            // buttonStartDNS
            // 
            buttonStartDNS.Location = new Point(33, 76);
            buttonStartDNS.Name = "buttonStartDNS";
            buttonStartDNS.Size = new Size(128, 29);
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
            groupBoxAuxiliaryServerManagement.Location = new Point(509, 3);
            groupBoxAuxiliaryServerManagement.Name = "groupBoxAuxiliaryServerManagement";
            groupBoxAuxiliaryServerManagement.Size = new Size(491, 252);
            groupBoxAuxiliaryServerManagement.TabIndex = 2;
            groupBoxAuxiliaryServerManagement.TabStop = false;
            groupBoxAuxiliaryServerManagement.Text = "Auxiliary Server Management";
            // 
            // buttonStopQuazal
            // 
            buttonStopQuazal.Location = new Point(181, 180);
            buttonStopQuazal.Name = "buttonStopQuazal";
            buttonStopQuazal.Size = new Size(123, 29);
            buttonStopQuazal.TabIndex = 15;
            buttonStopQuazal.Text = "Stop Quazal";
            buttonStopQuazal.UseVisualStyleBackColor = true;
            buttonStopQuazal.Click += buttonStopQuazal_Click;
            // 
            // buttonStartQuazal
            // 
            buttonStartQuazal.Location = new Point(33, 180);
            buttonStartQuazal.Name = "buttonStartQuazal";
            buttonStartQuazal.Size = new Size(128, 29);
            buttonStartQuazal.TabIndex = 14;
            buttonStartQuazal.Text = "Start Quazal";
            buttonStartQuazal.UseVisualStyleBackColor = true;
            buttonStartQuazal.Click += buttonStartQuazal_Click;
            // 
            // buttonStopSRVEmu
            // 
            buttonStopSRVEmu.Location = new Point(181, 145);
            buttonStopSRVEmu.Name = "buttonStopSRVEmu";
            buttonStopSRVEmu.Size = new Size(123, 29);
            buttonStopSRVEmu.TabIndex = 13;
            buttonStopSRVEmu.Text = "Stop SRVEmu";
            buttonStopSRVEmu.UseVisualStyleBackColor = true;
            buttonStopSRVEmu.Click += buttonStopSRVEmu_Click;
            // 
            // buttonStartSRVEmu
            // 
            buttonStartSRVEmu.Location = new Point(33, 145);
            buttonStartSRVEmu.Name = "buttonStartSRVEmu";
            buttonStartSRVEmu.Size = new Size(128, 29);
            buttonStartSRVEmu.TabIndex = 12;
            buttonStartSRVEmu.Text = "Start SRVEmu";
            buttonStartSRVEmu.UseVisualStyleBackColor = true;
            buttonStartSRVEmu.Click += buttonStartSRVEmu_Click;
            // 
            // buttonStopMultiSpy
            // 
            buttonStopMultiSpy.Location = new Point(181, 111);
            buttonStopMultiSpy.Name = "buttonStopMultiSpy";
            buttonStopMultiSpy.Size = new Size(123, 29);
            buttonStopMultiSpy.TabIndex = 11;
            buttonStopMultiSpy.Text = "Stop MultiSpy";
            buttonStopMultiSpy.UseVisualStyleBackColor = true;
            buttonStopMultiSpy.Click += buttonStopMultiSpy_Click;
            // 
            // buttonStartMultiSpy
            // 
            buttonStartMultiSpy.Location = new Point(33, 111);
            buttonStartMultiSpy.Name = "buttonStartMultiSpy";
            buttonStartMultiSpy.Size = new Size(128, 29);
            buttonStartMultiSpy.TabIndex = 10;
            buttonStartMultiSpy.Text = "Start MultiSpy";
            buttonStartMultiSpy.UseVisualStyleBackColor = true;
            buttonStartMultiSpy.Click += buttonStartMultiSpy_Click;
            // 
            // buttonStopDNS
            // 
            buttonStopDNS.Location = new Point(181, 76);
            buttonStopDNS.Name = "buttonStopDNS";
            buttonStopDNS.Size = new Size(123, 29);
            buttonStopDNS.TabIndex = 7;
            buttonStopDNS.Text = "Stop DNS";
            buttonStopDNS.UseVisualStyleBackColor = true;
            buttonStopDNS.Click += buttonStopDNS_Click;
            // 
            // buttonStopHorizon
            // 
            buttonStopHorizon.Location = new Point(181, 41);
            buttonStopHorizon.Name = "buttonStopHorizon";
            buttonStopHorizon.Size = new Size(123, 29);
            buttonStopHorizon.TabIndex = 6;
            buttonStopHorizon.Text = "Stop Horizon";
            buttonStopHorizon.UseVisualStyleBackColor = true;
            buttonStopHorizon.Click += buttonStopHorizon_Click;
            // 
            // buttonStartHorizon
            // 
            buttonStartHorizon.Location = new Point(33, 41);
            buttonStartHorizon.Name = "buttonStartHorizon";
            buttonStartHorizon.Size = new Size(128, 29);
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
            pictureBoxPSMSImage.Name = "pictureBoxPSMSImage";
            pictureBoxPSMSImage.Size = new Size(1009, 476);
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
            tableLayoutPanelMain.Location = new Point(0, 483);
            tableLayoutPanelMain.Name = "tableLayoutPanelMain";
            tableLayoutPanelMain.RowCount = 1;
            tableLayoutPanelMain.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanelMain.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanelMain.Size = new Size(1007, 261);
            tableLayoutPanelMain.TabIndex = 4;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1009, 989);
            Controls.Add(groupBoxWebServerManagement);
            Controls.Add(tableLayoutPanelMain);
            Controls.Add(pictureBoxPSMSImage);
            Controls.Add(richTextBoxLog);
            DoubleBuffered = true;
            MaximizeBox = false;
            MinimumSize = new Size(1025, 1023);
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
