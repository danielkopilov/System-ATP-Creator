using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Linq;

namespace System_ATP_creator
{
    public partial class MainForm : Form
    {
        private PictureBox pbLogo;
        private TextBox txtSystemSN;
        private TextBox txtContract;
        private TextBox txtPMName;
        private TextBox txtOrderNumber;
        private ComboBox cmbSystemType;
        private ComboBox cmbVariant;
        private ComboBox cmbFOV;
        private Label lblVariant;
        private Label lblFOV;
        private RoundedGroupBox gbSystemType;
        private RoundedGroupBox gbComponents;
        private RoundedGroupBox gbRadiationSource;
        private RoundedGroupBox gbBBType;
        private RoundedGroupBox gbBBSize;
        private RoundedGroupBox gbISExitAperture;
        private RoundedGroupBox gbLOSLaser;
        private CheckBox cbSourceStage;
        private CheckBox cbRackmount;
        private CheckBox cbGimbal;
        private CheckBox cbLOSAlignmentTarget;
        private CheckBox cbCTE;
        private CheckBox cbDeviceCenter;
        private CheckBox cbNewPortStage;
        private CheckBox cbFocusStage;
        private CheckBox cbVRS;
        private CheckBox cbBB;
        private CheckBox cbIS;
        private CheckBox cbLOSLaser;
        private CheckBox cbBacklight;
        private CheckBox cbQTHLamp;
        private CheckBox cbXYStage;
        private CheckBox cbPowerMeter;
        private CheckBox cbEnergyMeter;
        private CheckBox cbManualChoke;
        private ComboBox cmbBBType;
        private ComboBox cmbBBSize;
        private ComboBox cmbISExitAperture;
        private ComboBox cmbBacklightType;
        private TextBox txtFocusStageFiniteDistance;
        private TextBox txtFocusStageFiniteDistance2;
        private TextBox txtFocusStageFiniteDistance3;
        private TextBox txtGimbalSize;
        private TextBox txtNewPortStageMaxWeight;
        private ComboBox cmbVRS1;
        private ComboBox cmbVRS2;
        private ComboBox cmbVRS3;
        private Button btnUploadPDF;
        private Button btnGenerateATP;
        private Button btnBrowseSavePath;
        private Label lblPDFPath;
        private TextBox txtSavePath;
        private string selectedPDFPath = "";
        private string savePath = "";

        public MainForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "ATP Creator - System Configuration";
            this.Size = new Size(660, 850);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.AutoScroll = true;
            this.Font = new Font("Segoe UI", 9F);










            int yPos = 5;

            // Add Company Logo - large size (3x)
            pbLogo = new PictureBox();
            pbLogo.Location = new Point(30, yPos);
            pbLogo.Size = new Size(200, 35);
            pbLogo.SizeMode = PictureBoxSizeMode.Zoom;
            pbLogo.BackColor = Color.FromArgb(245, 247, 250);
            pbLogo.BorderStyle = BorderStyle.None;
            
            // Try to load logo from file
            string logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logo.png");
            if (File.Exists(logoPath))
            {
                try
                {
                    pbLogo.Image = Image.FromFile(logoPath);
                }
                catch
                {
                    // If loading fails, create a text-based logo
                    CreateTextLogo();
                }
            }
            else
            {
                // Create a text-based logo
                CreateTextLogo();
            }
            
            this.Controls.Add(pbLogo);

            // Add Version Information in upper right corner
            Label lblVersion = new Label();
            lblVersion.Text = "Version: 1.0";
            lblVersion.Font = new Font("Segoe UI", 8F, FontStyle.Regular);
            lblVersion.ForeColor = Color.Black;
            lblVersion.BackColor = Color.FromArgb(245, 247, 250);
            lblVersion.AutoSize = true;
            lblVersion.Location = new Point(this.ClientSize.Width - 160, 10);
            lblVersion.TextAlign = ContentAlignment.TopRight;
            this.Controls.Add(lblVersion);

            Label lblLastUpdate = new Label();
            lblLastUpdate.Text = "Last Update: 21.04.2026";
            lblLastUpdate.Font = new Font("Segoe UI", 8F, FontStyle.Regular);
            lblLastUpdate.ForeColor = Color.Black;
            lblLastUpdate.BackColor = Color.FromArgb(245, 247, 250);
            lblLastUpdate.AutoSize = true;
            lblLastUpdate.Location = new Point(this.ClientSize.Width - 160, 25);
            lblLastUpdate.TextAlign = ContentAlignment.TopRight;
            this.Controls.Add(lblLastUpdate);

            yPos += 35;

            // Add Title Label - No gap between logo and title
            Label lblTitle = new Label();
            lblTitle.Text = "ATP Creator";
            lblTitle.Font = new Font("Segoe UI", 18, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(30, 64, 175);
            lblTitle.Location = new Point(30, yPos);
            lblTitle.Size = new Size(600, 30);
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(lblTitle);

            yPos += 38;

            // PM Name, Order#, System, Contract in one row - all textboxes same size (65px)
            Label lblPMName = new Label();
            lblPMName.Text = "PM Name:";
            lblPMName.Location = new Point(40, yPos + 2);
            lblPMName.Size = new Size(70, 22);
            lblPMName.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            lblPMName.ForeColor = Color.FromArgb(55, 65, 81);
            lblPMName.TextAlign = ContentAlignment.MiddleLeft;
            this.Controls.Add(lblPMName);

            txtPMName = new TextBox();
            txtPMName.Location = new Point(115, yPos);
            txtPMName.Size = new Size(65, 24);
            txtPMName.PlaceholderText = "Daniel K";
            txtPMName.Font = new Font("Segoe UI", 9F);
            txtPMName.BackColor = Color.White;
            txtPMName.ForeColor = Color.FromArgb(31, 41, 55);
            this.Controls.Add(txtPMName);

            Label lblOrderNumber = new Label();
            lblOrderNumber.Text = "Order#:";
            lblOrderNumber.Location = new Point(190, yPos + 2);
            lblOrderNumber.Size = new Size(50, 22);
            lblOrderNumber.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            lblOrderNumber.ForeColor = Color.FromArgb(55, 65, 81);
            lblOrderNumber.TextAlign = ContentAlignment.MiddleLeft;
            this.Controls.Add(lblOrderNumber);

            txtOrderNumber = new TextBox();
            txtOrderNumber.Location = new Point(245, yPos);
            txtOrderNumber.Size = new Size(65, 24);
            txtOrderNumber.PlaceholderText = "22334";
            txtOrderNumber.Font = new Font("Segoe UI", 9F);
            txtOrderNumber.BackColor = Color.White;
            txtOrderNumber.ForeColor = Color.FromArgb(31, 41, 55);
            this.Controls.Add(txtOrderNumber);

            Label lblSystem = new Label();
            lblSystem.Text = "System S/N:";
            lblSystem.Location = new Point(320, yPos + 2);
            lblSystem.Size = new Size(75, 22);
            lblSystem.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            lblSystem.ForeColor = Color.FromArgb(55, 65, 81);
            lblSystem.TextAlign = ContentAlignment.MiddleLeft;
            this.Controls.Add(lblSystem);

            txtSystemSN = new TextBox();
            txtSystemSN.Location = new Point(400, yPos);
            txtSystemSN.Size = new Size(65, 24);
            txtSystemSN.PlaceholderText = "26METS-...";
            txtSystemSN.Font = new Font("Segoe UI", 9F);
            txtSystemSN.BackColor = Color.White;
            txtSystemSN.ForeColor = Color.FromArgb(31, 41, 55);
            this.Controls.Add(txtSystemSN);

            Label lblContract = new Label();
            lblContract.Text = "Contract:";
            lblContract.Location = new Point(475, yPos + 2);
            lblContract.Size = new Size(60, 22);
            lblContract.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            lblContract.ForeColor = Color.FromArgb(55, 65, 81);
            lblContract.TextAlign = ContentAlignment.MiddleLeft;
            this.Controls.Add(lblContract);

            txtContract = new TextBox();
            txtContract.Location = new Point(540, yPos);
            txtContract.Size = new Size(65, 24);
            txtContract.PlaceholderText = "D123456";
            txtContract.Font = new Font("Segoe UI", 9F);
            txtContract.BackColor = Color.White;
            txtContract.ForeColor = Color.FromArgb(31, 41, 55);
            this.Controls.Add(txtContract);

            yPos += 30;

            // System Type Group - dropdown based with Type, Variant, and FOV in one line
            gbSystemType = new RoundedGroupBox();
            gbSystemType.Text = "System Type";
            gbSystemType.Location = new Point(30, yPos);
            gbSystemType.Size = new Size(600, 60);
            gbSystemType.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            gbSystemType.ForeColor = Color.FromArgb(30, 64, 175);
            gbSystemType.BackColor = Color.FromArgb(250, 251, 253);

            Label lblSystemType = new Label();
            lblSystemType.Text = "Type:";
            lblSystemType.Location = new Point(20, 28);
            lblSystemType.Size = new Size(40, 22);
            lblSystemType.Font = new Font("Segoe UI", 9F);
            lblSystemType.ForeColor = Color.FromArgb(55, 65, 81);
            gbSystemType.Controls.Add(lblSystemType);

            cmbSystemType = new ComboBox();
            cmbSystemType.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbSystemType.Location = new Point(60, 26);
            cmbSystemType.Size = new Size(80, 24);
            cmbSystemType.Font = new Font("Segoe UI", 9F);
            cmbSystemType.BackColor = Color.White;
            cmbSystemType.ForeColor = Color.FromArgb(31, 41, 55);
            cmbSystemType.Items.AddRange(new object[] { "METS", "ILET", "WFOV", "CFT" });
            cmbSystemType.SelectedIndexChanged += CmbSystemType_SelectedIndexChanged;
            gbSystemType.Controls.Add(cmbSystemType);

            lblVariant = new Label();
            lblVariant.Text = "Variant:";
            lblVariant.Location = new Point(150, 28);
            lblVariant.Size = new Size(50, 22);
            lblVariant.Font = new Font("Segoe UI", 9F);
            lblVariant.ForeColor = Color.FromArgb(55, 65, 81);
            gbSystemType.Controls.Add(lblVariant);

            cmbVariant = new ComboBox();
            cmbVariant.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbVariant.Location = new Point(200, 26);
            cmbVariant.Size = new Size(90, 24);
            cmbVariant.Font = new Font("Segoe UI", 9F);
            cmbVariant.BackColor = Color.White;
            cmbVariant.ForeColor = Color.FromArgb(31, 41, 55);
            cmbVariant.Enabled = false;
            cmbVariant.SelectedIndexChanged += CmbVariant_SelectedIndexChanged;
            gbSystemType.Controls.Add(cmbVariant);

            lblFOV = new Label();
            lblFOV.Text = "Aperture:";
            lblFOV.Location = new Point(300, 28);
            lblFOV.Size = new Size(60, 22);
            lblFOV.Font = new Font("Segoe UI", 9F);
            lblFOV.ForeColor = Color.FromArgb(55, 65, 81);
            gbSystemType.Controls.Add(lblFOV);

            cmbFOV = new ComboBox();
            cmbFOV.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbFOV.Location = new Point(365, 26);
            cmbFOV.Size = new Size(100, 24);
            cmbFOV.Font = new Font("Segoe UI", 9F);
            cmbFOV.BackColor = Color.White;
            cmbFOV.ForeColor = Color.FromArgb(31, 41, 55);
            cmbFOV.Enabled = false;
            gbSystemType.Controls.Add(cmbFOV);

            this.Controls.Add(gbSystemType);

            yPos += 65;

            // Radiation Source Group
            gbRadiationSource = new RoundedGroupBox();
            gbRadiationSource.Text = "Radiation Source";
            gbRadiationSource.Location = new Point(30, yPos);
            gbRadiationSource.Size = new Size(600, 165);
            gbRadiationSource.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            gbRadiationSource.ForeColor = Color.FromArgb(30, 64, 175);
            gbRadiationSource.BackColor = Color.FromArgb(250, 251, 253);

            // Column positions for alignment
            int col1_Checkbox = 20;
            int col2_Label = 120;
            int col3_Control = 210;

            // B.B - First Row
            cbBB = new CheckBox();
            cbBB.Text = "B.B";
            cbBB.Location = new Point(col1_Checkbox, 26);
            cbBB.Size = new Size(60, 20);
            cbBB.Font = new Font("Segoe UI", 9.5F);
            cbBB.ForeColor = Color.FromArgb(55, 65, 81);
            cbBB.CheckedChanged += CbBB_CheckedChanged;

            Label lblBBType = new Label();
            lblBBType.Text = "Type:";
            lblBBType.Location = new Point(col2_Label, 27);
            lblBBType.Size = new Size(80, 20);
            lblBBType.Font = new Font("Segoe UI", 9F);
            lblBBType.ForeColor = Color.FromArgb(75, 85, 99);
            lblBBType.Enabled = false;
            gbRadiationSource.Controls.Add(lblBBType);

            cmbBBType = new ComboBox();
            cmbBBType.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbBBType.Location = new Point(col3_Control, 24);
            cmbBBType.Size = new Size(100, 24);
            cmbBBType.Font = new Font("Segoe UI", 9F);
            cmbBBType.BackColor = Color.White;
            cmbBBType.ForeColor = Color.FromArgb(31, 41, 55);
            cmbBBType.Items.AddRange(new object[] { "RR", "STD", "SR200N-33" });
            cmbBBType.Enabled = false;
            cmbBBType.SelectedIndexChanged += CmbBBType_SelectedIndexChanged;
            gbRadiationSource.Controls.Add(cmbBBType);

            Label lblBBSize = new Label();
            lblBBSize.Text = "Size:";
            lblBBSize.Location = new Point(350, 27);
            lblBBSize.Size = new Size(35, 20);
            lblBBSize.Font = new Font("Segoe UI", 9F);
            lblBBSize.ForeColor = Color.FromArgb(75, 85, 99);
            lblBBSize.Enabled = false;
            gbRadiationSource.Controls.Add(lblBBSize);

            cmbBBSize = new ComboBox();
            cmbBBSize.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbBBSize.Location = new Point(390, 24);
            cmbBBSize.Size = new Size(80, 24);
            cmbBBSize.Font = new Font("Segoe UI", 9F);
            cmbBBSize.BackColor = Color.White;
            cmbBBSize.ForeColor = Color.FromArgb(31, 41, 55);
            cmbBBSize.Items.AddRange(new object[] { "1D", "2D", "4D", "8D", "12D" });
            cmbBBSize.Enabled = false;
            gbRadiationSource.Controls.Add(cmbBBSize);

            // I.S - Second Row
            cbIS = new CheckBox();
            cbIS.Text = "I.S";
            cbIS.Location = new Point(col1_Checkbox, 56);
            cbIS.Size = new Size(60, 20);
            cbIS.Font = new Font("Segoe UI", 9.5F);
            cbIS.ForeColor = Color.FromArgb(55, 65, 81);
            cbIS.CheckedChanged += CbIS_CheckedChanged;

            Label lblISAperture = new Label();
            lblISAperture.Text = "Exit Aperture:";
            lblISAperture.Location = new Point(col2_Label, 57);
            lblISAperture.Size = new Size(85, 20);
            lblISAperture.Font = new Font("Segoe UI", 9F);
            lblISAperture.ForeColor = Color.FromArgb(75, 85, 99);
            lblISAperture.Enabled = false;
            gbRadiationSource.Controls.Add(lblISAperture);

            cmbISExitAperture = new ComboBox();
            cmbISExitAperture.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbISExitAperture.Location = new Point(col3_Control, 54);
            cmbISExitAperture.Size = new Size(100, 24);
            cmbISExitAperture.Font = new Font("Segoe UI", 9F);
            cmbISExitAperture.BackColor = Color.White;
            cmbISExitAperture.ForeColor = Color.FromArgb(31, 41, 55);
            cmbISExitAperture.Items.AddRange(new object[] { "2\"", "3\"", "4\"", "5\"" });
            cmbISExitAperture.Enabled = false;
            gbRadiationSource.Controls.Add(cmbISExitAperture);

            // Backlight - Third Row
            cbBacklight = new CheckBox();
            cbBacklight.Text = "Backlight";
            cbBacklight.Location = new Point(col1_Checkbox, 86);
            cbBacklight.Size = new Size(95, 24);
            cbBacklight.Font = new Font("Segoe UI", 9.5F);
            cbBacklight.ForeColor = Color.FromArgb(55, 65, 81);
            cbBacklight.CheckedChanged += CbBacklight_CheckedChanged;

            Label lblBacklightType = new Label();
            lblBacklightType.Text = "Type:";
            lblBacklightType.Location = new Point(col2_Label, 87);
            lblBacklightType.Size = new Size(40, 20);
            lblBacklightType.Font = new Font("Segoe UI", 9F);
            lblBacklightType.ForeColor = Color.FromArgb(75, 85, 99);
            lblBacklightType.Enabled = false;  // Disabled by default, matches Exit Aperture appearance
            gbRadiationSource.Controls.Add(lblBacklightType);

            cmbBacklightType = new ComboBox();
            cmbBacklightType.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbBacklightType.Location = new Point(col3_Control, 84);
            cmbBacklightType.Size = new Size(100, 24);
            cmbBacklightType.Font = new Font("Segoe UI", 9F);
            cmbBacklightType.BackColor = Color.White;
            cmbBacklightType.ForeColor = Color.FromArgb(31, 41, 55);
            cmbBacklightType.Items.AddRange(new object[] { "LED", "Fiber Optic" });
            cmbBacklightType.Enabled = false;
            gbRadiationSource.Controls.Add(cmbBacklightType);

            // LOS Laser - Fourth Row (no wavelength input)
            cbLOSLaser = new CheckBox();
            cbLOSLaser.Text = "LOS Laser";
            cbLOSLaser.Location = new Point(col1_Checkbox, 112);
            cbLOSLaser.Size = new Size(95, 20);
            cbLOSLaser.Font = new Font("Segoe UI", 9.5F);
            cbLOSLaser.ForeColor = Color.FromArgb(55, 65, 81);

            // QTH Lamp - Fifth Row
            cbQTHLamp = new CheckBox();
            cbQTHLamp.Text = "QTH Lamp";
            cbQTHLamp.Location = new Point(col1_Checkbox, 138);
            cbQTHLamp.Size = new Size(95, 24);
            cbQTHLamp.Font = new Font("Segoe UI", 9.5F);
            cbQTHLamp.ForeColor = Color.FromArgb(55, 65, 81);
            cbQTHLamp.CheckedChanged += CbQTHLamp_CheckedChanged;

            gbRadiationSource.Controls.Add(cbBB);
            gbRadiationSource.Controls.Add(cbIS);
            gbRadiationSource.Controls.Add(cbLOSLaser);
            gbRadiationSource.Controls.Add(cbBacklight);
            gbRadiationSource.Controls.Add(cbQTHLamp);
            this.Controls.Add(gbRadiationSource);

            yPos += 175;

            // Components Group
            gbComponents = new RoundedGroupBox();
            gbComponents.Text = "System Components";
            gbComponents.Location = new Point(30, yPos);
            gbComponents.Size = new Size(600, 220);
            gbComponents.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            gbComponents.ForeColor = Color.FromArgb(30, 64, 175);
            gbComponents.BackColor = Color.FromArgb(250, 251, 253);

            cbSourceStage = new CheckBox();
            cbSourceStage.Text = "Source Stage";
            cbSourceStage.Location = new Point(20, 26);
            cbSourceStage.Size = new Size(120, 22);
            cbSourceStage.Font = new Font("Segoe UI", 9.5F);
            cbSourceStage.ForeColor = Color.FromArgb(55, 65, 81);
            cbSourceStage.CheckedChanged += CbComponents_CheckedChanged;

            cbRackmount = new CheckBox();
            cbRackmount.Text = "Rackmount";
            cbRackmount.Location = new Point(170, 26);
            cbRackmount.Size = new Size(105, 22);
            cbRackmount.Font = new Font("Segoe UI", 9.5F);
            cbRackmount.ForeColor = Color.FromArgb(55, 65, 81);
            cbRackmount.CheckedChanged += CbComponents_CheckedChanged;

            // Manual Choke - Row 1 (moved from row 3)
            cbManualChoke = new CheckBox();
            cbManualChoke.Text = "Manual Choke";
            cbManualChoke.Location = new Point(305, 26);
            cbManualChoke.Size = new Size(120, 22);
            cbManualChoke.Font = new Font("Segoe UI", 9.5F);
            cbManualChoke.ForeColor = Color.FromArgb(55, 65, 81);
            cbManualChoke.CheckedChanged += CbComponents_CheckedChanged;

            cbCTE = new CheckBox();
            cbCTE.Text = "CTE";
            cbCTE.Location = new Point(20, 52);
            cbCTE.Size = new Size(100, 22);
            cbCTE.Font = new Font("Segoe UI", 9.5F);
            cbCTE.ForeColor = Color.FromArgb(55, 65, 81);
            cbCTE.CheckedChanged += CbComponents_CheckedChanged;

            cbDeviceCenter = new CheckBox();
            cbDeviceCenter.Text = "Device Center";
            cbDeviceCenter.Location = new Point(170, 52);
            cbDeviceCenter.Size = new Size(120, 22);
            cbDeviceCenter.Font = new Font("Segoe UI", 9.5F);
            cbDeviceCenter.ForeColor = Color.FromArgb(55, 65, 81);
            cbDeviceCenter.CheckedChanged += CbComponents_CheckedChanged;

            cbLOSAlignmentTarget = new CheckBox();
            cbLOSAlignmentTarget.Text = "LOS alignment target";
            cbLOSAlignmentTarget.Location = new Point(305, 52);
            cbLOSAlignmentTarget.Size = new Size(160, 22);
            cbLOSAlignmentTarget.Font = new Font("Segoe UI", 9.5F);
            cbLOSAlignmentTarget.ForeColor = Color.FromArgb(55, 65, 81);
            cbLOSAlignmentTarget.CheckedChanged += CbComponents_CheckedChanged;

            // XY Stage - Row 3
            cbXYStage = new CheckBox();
            cbXYStage.Text = "XY Stage";
            cbXYStage.Location = new Point(20, 78);
            cbXYStage.Size = new Size(90, 22);
            cbXYStage.Font = new Font("Segoe UI", 9.5F);
            cbXYStage.ForeColor = Color.FromArgb(55, 65, 81);
            cbXYStage.CheckedChanged += CbComponents_CheckedChanged;

            // Power Meter - Row 3 (aligned with column 2)
            cbPowerMeter = new CheckBox();
            cbPowerMeter.Text = "Power Meter";
            cbPowerMeter.Location = new Point(170, 78);
            cbPowerMeter.Size = new Size(115, 22);
            cbPowerMeter.Font = new Font("Segoe UI", 9.5F);
            cbPowerMeter.ForeColor = Color.FromArgb(55, 65, 81);
            cbPowerMeter.CheckedChanged += CbComponents_CheckedChanged;

            // Energy Meter - Row 3 (aligned with column 3)
            cbEnergyMeter = new CheckBox();
            cbEnergyMeter.Text = "Energy Meter";
            cbEnergyMeter.Location = new Point(305, 78);
            cbEnergyMeter.Size = new Size(120, 22);
            cbEnergyMeter.Font = new Font("Segoe UI", 9.5F);
            cbEnergyMeter.ForeColor = Color.FromArgb(55, 65, 81);
            cbEnergyMeter.CheckedChanged += CbComponents_CheckedChanged;

            // NewPort Stage - Row 4
            cbNewPortStage = new CheckBox();
            cbNewPortStage.Text = "NewPort Stage";
            cbNewPortStage.Location = new Point(20, 108);
            cbNewPortStage.Size = new Size(115, 22);
            cbNewPortStage.Font = new Font("Segoe UI", 9.5F);
            cbNewPortStage.ForeColor = Color.FromArgb(55, 65, 81);
            cbNewPortStage.CheckedChanged += CbNewPortStage_CheckedChanged;

            Label lblMaxWeight = new Label();
            lblMaxWeight.Text = "Max Weight:";
            lblMaxWeight.Location = new Point(145, 109);
            lblMaxWeight.Size = new Size(75, 20);
            lblMaxWeight.Font = new Font("Segoe UI", 9F);
            lblMaxWeight.ForeColor = Color.FromArgb(75, 85, 99);
            lblMaxWeight.Enabled = false;
            gbComponents.Controls.Add(lblMaxWeight);

            txtNewPortStageMaxWeight = new TextBox();
            txtNewPortStageMaxWeight.Location = new Point(230, 106);
            txtNewPortStageMaxWeight.Size = new Size(50, 24);
            txtNewPortStageMaxWeight.Font = new Font("Segoe UI", 9F);
            txtNewPortStageMaxWeight.BackColor = Color.White;
            txtNewPortStageMaxWeight.ForeColor = Color.FromArgb(31, 41, 55);
            txtNewPortStageMaxWeight.Enabled = false;
            gbComponents.Controls.Add(txtNewPortStageMaxWeight);

            Label lblMaxWeightUnit = new Label();
            lblMaxWeightUnit.Text = "[KG]";
            lblMaxWeightUnit.Location = new Point(285, 109);
            lblMaxWeightUnit.Size = new Size(35, 20);
            lblMaxWeightUnit.Font = new Font("Segoe UI", 9F);
            lblMaxWeightUnit.ForeColor = Color.FromArgb(75, 85, 99);
            lblMaxWeightUnit.Enabled = false;
            gbComponents.Controls.Add(lblMaxWeightUnit);

            // Focus Stage - Row 5 (moved from row 3)
            cbFocusStage = new CheckBox();
            cbFocusStage.Text = "Focus Stage";
            cbFocusStage.Location = new Point(20, 134);
            cbFocusStage.Size = new Size(105, 22);
            cbFocusStage.Font = new Font("Segoe UI", 9.5F);
            cbFocusStage.ForeColor = Color.FromArgb(55, 65, 81);
            cbFocusStage.CheckedChanged += CbFocusStage_CheckedChanged;

            Label lblFiniteDistance = new Label();
            lblFiniteDistance.Text = "Finite Distance:";
            lblFiniteDistance.Location = new Point(130, 135);
            lblFiniteDistance.Size = new Size(100, 20);
            lblFiniteDistance.Font = new Font("Segoe UI", 9F);
            lblFiniteDistance.ForeColor = Color.FromArgb(75, 85, 99);
            lblFiniteDistance.Enabled = false;
            gbComponents.Controls.Add(lblFiniteDistance);

            txtFocusStageFiniteDistance = new TextBox();
            txtFocusStageFiniteDistance.Location = new Point(230, 132);
            txtFocusStageFiniteDistance.Size = new Size(50, 24);
            txtFocusStageFiniteDistance.Font = new Font("Segoe UI", 9F);
            txtFocusStageFiniteDistance.BackColor = Color.White;
            txtFocusStageFiniteDistance.ForeColor = Color.FromArgb(31, 41, 55);
            txtFocusStageFiniteDistance.Enabled = false;
            gbComponents.Controls.Add(txtFocusStageFiniteDistance);

            Label lblFiniteDistanceUnit = new Label();
            lblFiniteDistanceUnit.Text = "[m]";
            lblFiniteDistanceUnit.Location = new Point(285, 135);
            lblFiniteDistanceUnit.Size = new Size(40, 20);
            lblFiniteDistanceUnit.Font = new Font("Segoe UI", 9F);
            lblFiniteDistanceUnit.ForeColor = Color.FromArgb(75, 85, 99);
            lblFiniteDistanceUnit.Enabled = false;
            gbComponents.Controls.Add(lblFiniteDistanceUnit);

            // Finite Distance 2
            txtFocusStageFiniteDistance2 = new TextBox();
            txtFocusStageFiniteDistance2.Location = new Point(325, 132);
            txtFocusStageFiniteDistance2.Size = new Size(50, 24);
            txtFocusStageFiniteDistance2.Font = new Font("Segoe UI", 9F);
            txtFocusStageFiniteDistance2.BackColor = Color.White;
            txtFocusStageFiniteDistance2.ForeColor = Color.FromArgb(31, 41, 55);
            txtFocusStageFiniteDistance2.Enabled = false;
            gbComponents.Controls.Add(txtFocusStageFiniteDistance2);

            Label lblFiniteDistanceUnit2 = new Label();
            lblFiniteDistanceUnit2.Text = "[m]";
            lblFiniteDistanceUnit2.Location = new Point(380, 135);
            lblFiniteDistanceUnit2.Size = new Size(40, 20);
            lblFiniteDistanceUnit2.Font = new Font("Segoe UI", 9F);
            lblFiniteDistanceUnit2.ForeColor = Color.FromArgb(75, 85, 99);
            lblFiniteDistanceUnit2.Enabled = false;
            gbComponents.Controls.Add(lblFiniteDistanceUnit2);

            // Finite Distance 3
            txtFocusStageFiniteDistance3 = new TextBox();
            txtFocusStageFiniteDistance3.Location = new Point(420, 132);
            txtFocusStageFiniteDistance3.Size = new Size(50, 24);
            txtFocusStageFiniteDistance3.Font = new Font("Segoe UI", 9F);
            txtFocusStageFiniteDistance3.BackColor = Color.White;
            txtFocusStageFiniteDistance3.ForeColor = Color.FromArgb(31, 41, 55);
            txtFocusStageFiniteDistance3.Enabled = false;
            gbComponents.Controls.Add(txtFocusStageFiniteDistance3);

            Label lblFiniteDistanceUnit3 = new Label();
            lblFiniteDistanceUnit3.Text = "[m]";
            lblFiniteDistanceUnit3.Location = new Point(475, 135);
            lblFiniteDistanceUnit3.Size = new Size(40, 20);
            lblFiniteDistanceUnit3.Font = new Font("Segoe UI", 9F);
            lblFiniteDistanceUnit3.ForeColor = Color.FromArgb(75, 85, 99);
            lblFiniteDistanceUnit3.Enabled = false;
            gbComponents.Controls.Add(lblFiniteDistanceUnit3);

            // VRS - Row 6 (moved from row 4)
            cbVRS = new CheckBox();
            cbVRS.Text = "VRS";
            cbVRS.Location = new Point(20, 160);
            cbVRS.Size = new Size(65, 22);
            cbVRS.Font = new Font("Segoe UI", 9.5F);
            cbVRS.ForeColor = Color.FromArgb(55, 65, 81);
            cbVRS.CheckedChanged += CbVRS_CheckedChanged;

            cmbVRS1 = new ComboBox();
            cmbVRS1.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbVRS1.Location = new Point(90, 158);
            cmbVRS1.Size = new Size(105, 24);
            cmbVRS1.Font = new Font("Segoe UI", 9F);
            cmbVRS1.BackColor = Color.White;
            cmbVRS1.ForeColor = Color.FromArgb(31, 41, 55);
            cmbVRS1.Items.AddRange(new object[] { "NA", "1550 [nm]", "1570 [nm]", "1064 [nm]", "1540 [nm]" });
            cmbVRS1.Enabled = false;
            gbComponents.Controls.Add(cmbVRS1);

            cmbVRS2 = new ComboBox();
            cmbVRS2.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbVRS2.Location = new Point(205, 158);
            cmbVRS2.Size = new Size(105, 24);
            cmbVRS2.Font = new Font("Segoe UI", 9F);
            cmbVRS2.BackColor = Color.White;
            cmbVRS2.ForeColor = Color.FromArgb(31, 41, 55);
            cmbVRS2.Items.AddRange(new object[] { "NA", "1550 [nm]", "1570 [nm]", "1064 [nm]", "1540 [nm]" });
            cmbVRS2.Enabled = false;
            gbComponents.Controls.Add(cmbVRS2);

            cmbVRS3 = new ComboBox();
            cmbVRS3.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbVRS3.Location = new Point(320, 158);
            cmbVRS3.Size = new Size(105, 24);
            cmbVRS3.Font = new Font("Segoe UI", 9F);
            cmbVRS3.BackColor = Color.White;
            cmbVRS3.ForeColor = Color.FromArgb(31, 41, 55);
            cmbVRS3.Items.AddRange(new object[] { "NA", "1550 [nm]", "1570 [nm]", "1064 [nm]", "1540 [nm]" });
            cmbVRS3.Enabled = false;
            gbComponents.Controls.Add(cmbVRS3);

            // Gimbal - Row 7 (moved from row 1, now under VRS)
            cbGimbal = new CheckBox();
            cbGimbal.Text = "Gimbal";
            cbGimbal.Location = new Point(20, 186);
            cbGimbal.Size = new Size(70, 22);
            cbGimbal.Font = new Font("Segoe UI", 9.5F);
            cbGimbal.ForeColor = Color.FromArgb(55, 65, 81);
            cbGimbal.CheckedChanged += CbGimbal_CheckedChanged;

            Label lblGimbalSize = new Label();
            lblGimbalSize.Text = "Size:";
            lblGimbalSize.Location = new Point(95, 188);
            lblGimbalSize.Size = new Size(35, 20);
            lblGimbalSize.Font = new Font("Segoe UI", 9F);
            lblGimbalSize.ForeColor = Color.FromArgb(75, 85, 99);
            lblGimbalSize.Enabled = false;
            gbComponents.Controls.Add(lblGimbalSize);

            txtGimbalSize = new TextBox();
            txtGimbalSize.Location = new Point(135, 184);
            txtGimbalSize.Size = new Size(30, 24);
            txtGimbalSize.Font = new Font("Segoe UI", 9F);
            txtGimbalSize.BackColor = Color.White;
            txtGimbalSize.ForeColor = Color.FromArgb(31, 41, 55);
            txtGimbalSize.Enabled = false;
            gbComponents.Controls.Add(txtGimbalSize);

            Label lblGimbalUnit = new Label();
            lblGimbalUnit.Text = "[Inches]";
            lblGimbalUnit.Location = new Point(168, 188);
            lblGimbalUnit.Size = new Size(50, 20);
            lblGimbalUnit.Font = new Font("Segoe UI", 8F);
            lblGimbalUnit.ForeColor = Color.FromArgb(75, 85, 99);
            lblGimbalUnit.Enabled = false;
            gbComponents.Controls.Add(lblGimbalUnit);

            gbComponents.Controls.Add(cbSourceStage);
            gbComponents.Controls.Add(cbRackmount);
            gbComponents.Controls.Add(cbGimbal);
            gbComponents.Controls.Add(cbCTE);
            gbComponents.Controls.Add(cbDeviceCenter);
            gbComponents.Controls.Add(cbNewPortStage);
            gbComponents.Controls.Add(cbLOSAlignmentTarget);
            gbComponents.Controls.Add(cbFocusStage);
            gbComponents.Controls.Add(cbVRS);
            gbComponents.Controls.Add(cbXYStage);
            gbComponents.Controls.Add(cbPowerMeter);
            gbComponents.Controls.Add(cbEnergyMeter);
            gbComponents.Controls.Add(cbManualChoke);
            this.Controls.Add(gbComponents);

            yPos += 230; // Increased to account for taller groupbox with Gimbal row

            // Separator line between System Components and Targets Definition
            Panel dividerComponents = new Panel();
            dividerComponents.Location = new Point(30, yPos);
            dividerComponents.Size = new Size(580, 1);
            dividerComponents.BackColor = Color.FromArgb(200, 200, 200);
            this.Controls.Add(dividerComponents);

            yPos += 15;

            // Targets Definition Section Title
            Label lblTargetsDefinition = new Label();
            lblTargetsDefinition.Text = "Targets Definition";
            lblTargetsDefinition.Location = new Point(30, yPos);
            lblTargetsDefinition.Size = new Size(180, 22);
            lblTargetsDefinition.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            lblTargetsDefinition.ForeColor = Color.FromArgb(30, 64, 175);
            this.Controls.Add(lblTargetsDefinition);

            yPos += 26;

            // CSV Upload Section - simple compact style
            Label lblUpload = new Label();
            lblUpload.Text = "Upload targets .csv file:";
            lblUpload.Location = new Point(30, yPos);
            lblUpload.Size = new Size(160, 16);
            lblUpload.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular);
            lblUpload.ForeColor = Color.FromArgb(30, 64, 175);
            this.Controls.Add(lblUpload);

            btnUploadPDF = new Button();
            btnUploadPDF.Text = "Upload CSV";
            btnUploadPDF.Location = new Point(30, yPos + 18);
            btnUploadPDF.Size = new Size(95, 26);
            btnUploadPDF.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular);
            btnUploadPDF.BackColor = Color.FromArgb(59, 130, 246);
            btnUploadPDF.ForeColor = Color.White;
            btnUploadPDF.FlatStyle = FlatStyle.Flat;
            btnUploadPDF.FlatAppearance.BorderSize = 0;
            btnUploadPDF.Cursor = Cursors.Hand;
            btnUploadPDF.Click += BtnUploadPDF_Click;
            this.Controls.Add(btnUploadPDF);

            lblPDFPath = new Label();
            lblPDFPath.Text = "No file selected";
            lblPDFPath.Location = new Point(135, yPos + 22);
            lblPDFPath.Size = new Size(330, 16);
            lblPDFPath.Font = new Font("Segoe UI", 8.5F, FontStyle.Italic);
            lblPDFPath.ForeColor = Color.FromArgb(107, 114, 128);
            this.Controls.Add(lblPDFPath);

            yPos += 50;

            // Separator line between sections
            Panel divider1 = new Panel();
            divider1.Location = new Point(30, yPos);
            divider1.Size = new Size(580, 1);
            divider1.BackColor = Color.FromArgb(200, 200, 200);
            this.Controls.Add(divider1);

            yPos += 15;

            // Save Section - matching style of other section titles
            Label lblSaveLocation = new Label();
            lblSaveLocation.Text = "Save Location";
            lblSaveLocation.Location = new Point(30, yPos);
            lblSaveLocation.Size = new Size(150, 22);
            lblSaveLocation.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            lblSaveLocation.ForeColor = Color.FromArgb(30, 64, 175);
            this.Controls.Add(lblSaveLocation);

            yPos += 26;

            Label lblSaveTo = new Label();
            lblSaveTo.Text = "Save to:";
            lblSaveTo.Location = new Point(30, yPos);
            lblSaveTo.Size = new Size(80, 18);
            lblSaveTo.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular);
            lblSaveTo.ForeColor = Color.FromArgb(30, 64, 175);
            this.Controls.Add(lblSaveTo);

            yPos += 20;

            txtSavePath = new TextBox();
            txtSavePath.Location = new Point(30, yPos);
            txtSavePath.Size = new Size(460, 22);
            txtSavePath.Font = new Font("Segoe UI", 8.5F);
            txtSavePath.BackColor = Color.White;
            txtSavePath.ForeColor = Color.FromArgb(31, 41, 55);
            txtSavePath.ReadOnly = true;
            txtSavePath.BorderStyle = BorderStyle.FixedSingle;
            // Set default path to Documents\Generated ATPs
            savePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Generated ATPs");
            txtSavePath.Text = savePath;
            this.Controls.Add(txtSavePath);

            btnBrowseSavePath = new Button();
            btnBrowseSavePath.Text = "Browse";
            btnBrowseSavePath.Location = new Point(500, yPos - 1);
            btnBrowseSavePath.Size = new Size(90, 24);
            btnBrowseSavePath.Font = new Font("Segoe UI", 8.5F);
            btnBrowseSavePath.BackColor = Color.FromArgb(59, 130, 246);
            btnBrowseSavePath.ForeColor = Color.White;
            btnBrowseSavePath.FlatStyle = FlatStyle.Flat;
            btnBrowseSavePath.FlatAppearance.BorderSize = 0;
            btnBrowseSavePath.Cursor = Cursors.Hand;
            btnBrowseSavePath.Click += BtnBrowseSavePath_Click;
            this.Controls.Add(btnBrowseSavePath);

            yPos += 35;

            // Generate ATP Button - larger size and closer to Save section
            btnGenerateATP = new Button();
            btnGenerateATP.Text = "Generate ATP";
            btnGenerateATP.Location = new Point(220, yPos);
            btnGenerateATP.Size = new Size(200, 40);
            btnGenerateATP.BackColor = Color.FromArgb(16, 185, 129);
            btnGenerateATP.ForeColor = Color.White;
            btnGenerateATP.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnGenerateATP.FlatStyle = FlatStyle.Flat;
            btnGenerateATP.FlatAppearance.BorderSize = 0;
            btnGenerateATP.Cursor = Cursors.Hand;
            btnGenerateATP.Click += BtnGenerateATP_Click;
            this.Controls.Add(btnGenerateATP);
        }

        private void CreateTextLogo()
        {
            // Create a bitmap for the logo - 70% of original size
            Bitmap logoBitmap = new Bitmap(322, 35);
            using (Graphics g = Graphics.FromImage(logoBitmap))
            {
                // Use the same background as the GUI
                g.Clear(Color.FromArgb(245, 247, 250));
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

                // CI SYSTEMS brand red color
                Color ciRed = Color.FromArgb(227, 6, 19);

                // Draw three horizontal red bars on the left (scaled to 70%)
                using (Brush redBrush = new SolidBrush(ciRed))
                {
                    float barWidth = 28;  // 40 * 0.7
                    float barHeight = 3.5f;  // 5 * 0.7
                    float startX = 7;  // 10 * 0.7
                    
                    g.FillRectangle(redBrush, startX, 7, barWidth, barHeight);
                    g.FillRectangle(redBrush, startX, 15.4f, barWidth, barHeight);
                    g.FillRectangle(redBrush, startX, 23.8f, barWidth, barHeight);
                }

                // Draw the large curved "C" shape (scaled to 70%)
                using (Pen redPen = new Pen(ciRed, 3.5f))  // 5 * 0.7
                {
                    redPen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                    redPen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                    
                    // Create the C curve (scaled to 70%)
                    Rectangle cRect = new Rectangle(36, 5, 31, 22);  // 52*0.7, 8*0.7, 45*0.7, 32*0.7
                    g.DrawArc(redPen, cRect, -30, 240);
                }

                // Draw the exclamation mark "!" (scaled to 70%)
                using (Brush redBrush = new SolidBrush(ciRed))
                {
                    // The vertical line of the exclamation mark
                    g.FillRectangle(redBrush, 73.5f, 7, 3.15f, 14);  // 105*0.7, 10*0.7, 4.5*0.7, 20*0.7
                    
                    // The dot of the exclamation mark
                    g.FillEllipse(redBrush, 72.8f, 23.1f, 4.2f, 4.2f);  // 104*0.7, 33*0.7, 6*0.7, 6*0.7
                }

                // Draw "CI SYSTEMS" text with scaled font
                using (Font logoFont = new Font("Arial", 15.4f, FontStyle.Bold))  // 22 * 0.7
                using (Brush textBrush = new SolidBrush(ciRed))
                {
                    g.DrawString("CI SYSTEMS", logoFont, textBrush, 80.5f, 7);  // 115*0.7, 10*0.7
                }
            }

            pbLogo.Image = logoBitmap;
        }

        private void CmbSystemType_SelectedIndexChanged(object? sender, EventArgs e)
        {
            // Clear and populate variant and FOV dropdowns based on system type
            cmbVariant.Items.Clear();
            cmbVariant.SelectedIndex = -1;
            cmbFOV.Items.Clear();
            cmbFOV.SelectedIndex = -1;

            string selectedType = cmbSystemType.SelectedItem?.ToString() ?? "";

            if (selectedType == "METS")
            {
                // Reset labels and positions to default
                lblVariant.Text = "Variant:";
                lblVariant.Size = new Size(50, 22);
                cmbVariant.Location = new Point(200, 26);
                cmbVariant.Size = new Size(90, 24);
                lblFOV.Text = "Aperture:";
                lblFOV.Size = new Size(60, 22);
                lblFOV.Location = new Point(300, 28);
                cmbFOV.Location = new Point(365, 26);
                cmbFOV.Size = new Size(100, 24);
                cmbFOV.DropDownWidth = 100;

                // For non-WFOV: label should be in front of dropdown
                lblFOV.BringToFront();

                cmbVariant.Items.AddRange(new object[] { "VS", "S", "L", "VL" });
                cmbVariant.Enabled = true;
                cmbFOV.Items.AddRange(new object[] { "10\"", "12\"", "14\"", "16\"", "19\"", "21\"" });
                cmbFOV.Enabled = true;
            }
            else if (selectedType == "ILET")
            {
                // Reset labels and positions to default
                lblVariant.Text = "Variant:";
                lblVariant.Size = new Size(50, 22);
                cmbVariant.Location = new Point(200, 26);
                cmbVariant.Size = new Size(90, 24);
                lblFOV.Text = "Aperture:";
                lblFOV.Size = new Size(60, 22);
                lblFOV.Location = new Point(300, 28);
                cmbFOV.Location = new Point(365, 26);
                cmbFOV.Size = new Size(100, 24);
                cmbFOV.DropDownWidth = 100;

                // For non-WFOV: label should be in front of dropdown
                lblFOV.BringToFront();

                cmbVariant.Items.AddRange(new object[] { "4", "5", "6" });
                cmbVariant.Enabled = true;
                // FOV for ILET depends on variant - will be set in CmbVariant_SelectedIndexChanged
                cmbFOV.Enabled = false;
            }
            else if (selectedType == "WFOV")
            {
                // Change labels for WFOV and adjust positions
                lblVariant.Text = "Spectral Range:";
                lblVariant.Size = new Size(90, 22); // Wider to accommodate "Spectral Range"
                cmbVariant.Location = new Point(245, 26); // Move dropdown to the right
                cmbVariant.Size = new Size(95, 24); // Slightly wider for longer text
                lblFOV.Text = "FOV:"; // No trailing spaces
                lblFOV.Size = new Size(35, 22); // Adjust width to fit "FOV:" exactly
                lblFOV.Location = new Point(350, 28); // FOV label position
                cmbFOV.Location = new Point(390, 26); // Position FOV dropdown closer to label
                cmbFOV.Size = new Size(90, 24); // Set proper width for FOV values
                cmbFOV.DropDownWidth = 90; // Ensure dropdown list matches control width

                // Ensure dropdown is in front by bringing it to front
                cmbFOV.BringToFront();

                // Add three spectral range options
                cmbVariant.Items.AddRange(new object[] { "0.9-1.7 [um]", "3-5 [um]", "8-12 [um]" });
                cmbVariant.Enabled = true;
                // FOV for WFOV depends on spectral range - will be set in CmbVariant_SelectedIndexChanged
                cmbFOV.Enabled = false;
            }
            else if (selectedType == "CFT")
            {
                // Reset labels and positions to default
                lblVariant.Text = "Variant:";
                lblVariant.Size = new Size(50, 22);
                cmbVariant.Location = new Point(200, 26);
                cmbVariant.Size = new Size(90, 24);
                lblFOV.Text = "Aperture:";
                lblFOV.Size = new Size(60, 22);
                lblFOV.Location = new Point(300, 28);
                cmbFOV.Location = new Point(365, 26);
                cmbFOV.Size = new Size(100, 24);
                cmbFOV.DropDownWidth = 100;

                // For non-WFOV: label should be in front of dropdown
                lblFOV.BringToFront();

                // CFT has IR-2, IR-5, VIS-3 variants
                cmbVariant.Items.AddRange(new object[] { "IR-2", "IR-5", "VIS-3" });
                cmbVariant.Enabled = true;
                // Aperture for CFT depends on variant - will be set in CmbVariant_SelectedIndexChanged
                cmbFOV.Enabled = false;
            }
            else
            {
                // Reset labels and positions to default
                lblVariant.Text = "Variant:";
                lblVariant.Size = new Size(50, 22);
                cmbVariant.Location = new Point(200, 26);
                cmbVariant.Size = new Size(90, 24);
                lblFOV.Text = "Aperture:";
                lblFOV.Size = new Size(60, 22);
                lblFOV.Location = new Point(300, 28);
                cmbFOV.Location = new Point(365, 26);
                cmbFOV.Size = new Size(100, 24);
                cmbFOV.DropDownWidth = 100;

                // For non-WFOV: label should be in front of dropdown
                lblFOV.BringToFront();

                cmbVariant.Enabled = false;
                cmbFOV.Enabled = false;
            }
        }

        private void CmbVariant_SelectedIndexChanged(object? sender, EventArgs e)
        {
            // Clear FOV selection
            cmbFOV.Items.Clear();
            cmbFOV.SelectedIndex = -1;

            string selectedType = cmbSystemType.SelectedItem?.ToString() ?? "";
            string selectedVariant = cmbVariant.SelectedItem?.ToString() ?? "";

            if (selectedType == "ILET" && !string.IsNullOrEmpty(selectedVariant))
            {
                // Set Aperture based on ILET variant - aperture matches variant number
                if (selectedVariant == "4")
                {
                    cmbFOV.Items.Add("4\"");
                    cmbFOV.SelectedIndex = 0; // Auto-select
                    cmbFOV.Enabled = false; // Locked, cannot change
                }
                else if (selectedVariant == "5")
                {
                    cmbFOV.Items.Add("5\"");
                    cmbFOV.SelectedIndex = 0; // Auto-select
                    cmbFOV.Enabled = false; // Locked, cannot change
                }
                else if (selectedVariant == "6")
                {
                    cmbFOV.Items.Add("6\"");
                    cmbFOV.SelectedIndex = 0; // Auto-select
                    cmbFOV.Enabled = false; // Locked, cannot change
                }
            }
            else if (selectedType == "METS")
            {
                // For METS, repopulate FOV options (all variants have same options)
                cmbFOV.Items.AddRange(new object[] { "8\"", "10\"", "12\"", "14\"", "16\"", "19\"", "21\"" });
                cmbFOV.Enabled = true;
            }
            else if (selectedType == "WFOV" && !string.IsNullOrEmpty(selectedVariant))
            {
                // Set FOV based on WFOV Spectral Range
                if (selectedVariant == "0.9-1.7 [um]")
                {
                    // Only one option: 11.7 [deg]
                    cmbFOV.Items.Add("11.7 [deg]");
                    cmbFOV.SelectedIndex = 0; // Auto-select
                    cmbFOV.Enabled = false; // Locked, cannot change (grey/disabled)
                }
                else if (selectedVariant == "3-5 [um]")
                {
                    // Two options: 11 [deg] and 15 [deg]
                    cmbFOV.Items.AddRange(new object[] { "11 [deg]", "15 [deg]" });
                    cmbFOV.Enabled = true; // User can choose
                }
                else if (selectedVariant == "8-12 [um]")
                {
                    // Only one option: 11 [deg]
                    cmbFOV.Items.Add("11 [deg]");
                    cmbFOV.SelectedIndex = 0; // Auto-select
                    cmbFOV.Enabled = false; // Locked, cannot change (grey/disabled)
                }
            }
            else if (selectedType == "CFT" && !string.IsNullOrEmpty(selectedVariant))
            {
                // Set Aperture based on CFT variant - each variant has a fixed aperture
                if (selectedVariant == "IR-2")
                {
                    cmbFOV.Items.Add("1.9\"");
                    cmbFOV.SelectedIndex = 0; // Auto-select
                    cmbFOV.Enabled = false; // Locked, cannot change
                }
                else if (selectedVariant == "IR-5")
                {
                    cmbFOV.Items.Add("4.9\"");
                    cmbFOV.SelectedIndex = 0; // Auto-select
                    cmbFOV.Enabled = false; // Locked, cannot change
                }
                else if (selectedVariant == "VIS-3")
                {
                    cmbFOV.Items.Add("2.95\"");
                    cmbFOV.SelectedIndex = 0; // Auto-select
                    cmbFOV.Enabled = false; // Locked, cannot change
                }
            }
        }

        private void CbComponents_CheckedChanged(object? sender, EventArgs e)
        {
            // PDF upload is always enabled since Target Wheel is always included
            // No action needed here
        }

        private void CbBB_CheckedChanged(object? sender, EventArgs e)
        {
            // Enable B.B controls when B.B is checked
            var lblType = gbRadiationSource.Controls.OfType<Label>().FirstOrDefault(l => l.Text == "Type:");
            var lblSize = gbRadiationSource.Controls.OfType<Label>().FirstOrDefault(l => l.Text == "Size:");
            
            if (lblType != null) lblType.Enabled = cbBB.Checked;
            if (lblSize != null) lblSize.Enabled = cbBB.Checked;
            cmbBBType.Enabled = cbBB.Checked;
            
            if (!cbBB.Checked)
            {
                cmbBBType.SelectedIndex = -1;
                cmbBBSize.Enabled = false;
                cmbBBSize.SelectedIndex = -1;
            }
        }

        private void CmbBBType_SelectedIndexChanged(object? sender, EventArgs e)
        {
            // Enable B.B Size when a B.B type is selected
            var lblSize = gbRadiationSource.Controls.OfType<Label>().FirstOrDefault(l => l.Text == "Size:");
            if (lblSize != null) lblSize.Enabled = cmbBBType.SelectedIndex >= 0;
            cmbBBSize.Enabled = cmbBBType.SelectedIndex >= 0;
            
            if (!cmbBBSize.Enabled)
            {
                cmbBBSize.SelectedIndex = -1;
            }
        }

        private void CbIS_CheckedChanged(object? sender, EventArgs e)
        {
            // Enable I.S Exit Aperture when I.S is checked
            var lblAperture = gbRadiationSource.Controls.OfType<Label>().FirstOrDefault(l => l.Text == "Exit Aperture:");
            if (lblAperture != null) lblAperture.Enabled = cbIS.Checked;
            cmbISExitAperture.Enabled = cbIS.Checked;
            
            if (!cbIS.Checked)
            {
                cmbISExitAperture.SelectedIndex = -1;
            }
        }

        private void BtnBrowseSavePath_Click(object? sender, EventArgs e)
        {
            using (SaveFileDialog saveDialog = new SaveFileDialog())
            {
                saveDialog.Title = "Choose where to save ATP files";
                saveDialog.Filter = "Word Documents (*.docx)|*.docx";
                saveDialog.DefaultExt = "docx";

                // Build default filename from user input
                string type = cmbSystemType.SelectedItem?.ToString() ?? "Type";
                string variant = cmbVariant.SelectedItem?.ToString() ?? "Variant";
                string aperture = cmbFOV.SelectedItem?.ToString() ?? "Aperture";
                aperture = aperture.Replace("\"", ""); // Remove '"' from aperture
                string orderNumber = txtOrderNumber.Text.Trim();
                if (string.IsNullOrEmpty(orderNumber)) orderNumber = "";

                // For ILET, don't include variant in filename (same logic as ATPGenerator)
                string defaultFileName;
                if (type.Equals("ILET", StringComparison.OrdinalIgnoreCase))
                {
                    defaultFileName = $"ATP_{type}_{aperture}_A{orderNumber}1000010.docx";
                }
                else
                {
                    defaultFileName = $"ATP_{type}_{variant}{aperture}_A{orderNumber}1000010.docx";
                }
                saveDialog.FileName = defaultFileName;

                if (!string.IsNullOrEmpty(savePath) && Directory.Exists(savePath))
                {
                    saveDialog.InitialDirectory = savePath;
                }
                else
                {
                    saveDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                }
                
                saveDialog.CheckPathExists = true;
                saveDialog.OverwritePrompt = false;
                
                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    string selectedFolder = Path.GetDirectoryName(saveDialog.FileName) ?? "";
                    if (!string.IsNullOrEmpty(selectedFolder) && Directory.Exists(selectedFolder))
                    {
                        try
                        {
                            string testFile = Path.Combine(selectedFolder, $"test_{Guid.NewGuid()}.tmp");
                            File.WriteAllText(testFile, "test");
                            File.Delete(testFile);
                            savePath = selectedFolder;
                            txtSavePath.Text = savePath;
                        }
                        catch
                        {
                            MessageBox.Show("Cannot write to the selected folder. Please choose a different location or check folder permissions.", 
                                "Access Denied", 
                                MessageBoxButtons.OK, 
                                MessageBoxIcon.Warning);
                        }
                    }
                }
            }
        }

        private void CbFocusStage_CheckedChanged(object? sender, EventArgs e)
        {
            // Enable Finite Distance inputs when Focus Stage is checked
            var lblFiniteDistance = gbComponents.Controls.OfType<Label>().FirstOrDefault(l => l.Text == "Finite Distance:");
            var lblUnits = gbComponents.Controls.OfType<Label>().Where(l => l.Text == "[m]").ToList();

            if (lblFiniteDistance != null) lblFiniteDistance.Enabled = cbFocusStage.Checked;

            // Enable all unit labels
            foreach (var lbl in lblUnits)
            {
                lbl.Enabled = cbFocusStage.Checked;
            }

            // Enable all three textboxes
            txtFocusStageFiniteDistance.Enabled = cbFocusStage.Checked;
            txtFocusStageFiniteDistance2.Enabled = cbFocusStage.Checked;
            txtFocusStageFiniteDistance3.Enabled = cbFocusStage.Checked;

            if (!cbFocusStage.Checked)
            {
                txtFocusStageFiniteDistance.Text = "";
                txtFocusStageFiniteDistance2.Text = "";
                txtFocusStageFiniteDistance3.Text = "";
            }
        }

        private void CbVRS_CheckedChanged(object? sender, EventArgs e)
        {
            // Enable VRS dropdowns when VRS is checked
            cmbVRS1.Enabled = cbVRS.Checked;
            cmbVRS2.Enabled = cbVRS.Checked;
            cmbVRS3.Enabled = cbVRS.Checked;
            
            if (!cbVRS.Checked)
            {
                cmbVRS1.SelectedIndex = -1;
                cmbVRS2.SelectedIndex = -1;
                cmbVRS3.SelectedIndex = -1;
            }
        }

        private void CbGimbal_CheckedChanged(object? sender, EventArgs e)
        {
            // Enable Gimbal size textbox when Gimbal is checked
            var lblGimbalSize = gbComponents.Controls.OfType<Label>().FirstOrDefault(l => l.Text == "Size:");
            var lblGimbalUnit = gbComponents.Controls.OfType<Label>().FirstOrDefault(l => l.Text == "[Inches]");
            
            if (lblGimbalSize != null) lblGimbalSize.Enabled = cbGimbal.Checked;
            if (lblGimbalUnit != null) lblGimbalUnit.Enabled = cbGimbal.Checked;
            txtGimbalSize.Enabled = cbGimbal.Checked;
            
            if (!cbGimbal.Checked)
            {
                txtGimbalSize.Text = "";
            }
        }

        private void CbNewPortStage_CheckedChanged(object? sender, EventArgs e)
        {
            // Enable NewPort Stage max weight textbox when NewPort Stage is checked
            var lblMaxWeight = gbComponents.Controls.OfType<Label>().FirstOrDefault(l => l.Text == "Max Weight:");
            var lblMaxWeightUnit = gbComponents.Controls.OfType<Label>().FirstOrDefault(l => l.Text == "[KG]");
            
            if (lblMaxWeight != null) lblMaxWeight.Enabled = cbNewPortStage.Checked;
            if (lblMaxWeightUnit != null) lblMaxWeightUnit.Enabled = cbNewPortStage.Checked;
            txtNewPortStageMaxWeight.Enabled = cbNewPortStage.Checked;
            
            if (!cbNewPortStage.Checked)
            {
                txtNewPortStageMaxWeight.Text = "";
            }
        }

        private void CbBacklight_CheckedChanged(object? sender, EventArgs e)
        {
            // Enable Backlight Type dropdown when Backlight is checked
            // Find the Backlight Type label (Y coordinate around 87, different from B.B Type label at Y=27)
            var lblBacklightType = gbRadiationSource.Controls.OfType<Label>().FirstOrDefault(l => l.Text == "Type:" && l.Location.Y > 50);

            if (lblBacklightType != null)
            {
                lblBacklightType.Enabled = cbBacklight.Checked;
                // When enabled, override the color to black (otherwise Enabled=false makes it light grey automatically)
                if (cbBacklight.Checked)
                {
                    lblBacklightType.ForeColor = Color.FromArgb(55, 65, 81);  // Black when enabled
                }
            }
            cmbBacklightType.Enabled = cbBacklight.Checked;

            if (!cbBacklight.Checked)
            {
                cmbBacklightType.SelectedIndex = -1;
            }
        }

        private void CbQTHLamp_CheckedChanged(object? sender, EventArgs e)
        {
            // QTH Lamp has no additional options currently
        }

        private void BtnUploadPDF_Click(object? sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "CSV Files (*.csv)|*.csv";
                openFileDialog.Title = "Select Target Wheel CSV File";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // Validate that it's actually a CSV file
                    string extension = Path.GetExtension(openFileDialog.FileName).ToLower();
                    if (extension != ".csv")
                    {
                        MessageBox.Show("Invalid file format! Please upload only .csv files.", 
                            "Invalid File Format", 
                            MessageBoxButtons.OK, 
                            MessageBoxIcon.Error);
                        return;
                    }

                    selectedPDFPath = openFileDialog.FileName;
                    lblPDFPath.Text = Path.GetFileName(selectedPDFPath);
                    lblPDFPath.ForeColor = Color.FromArgb(16, 185, 129);
                    lblPDFPath.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
                }
            }
        }

        private void BtnGenerateATP_Click(object? sender, EventArgs e)
        {
            // Validate inputs
            if (cmbSystemType.SelectedIndex < 0)
            {
                MessageBox.Show("Please select a System Type (METS, ILET, or WFOV).", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cmbVariant.SelectedIndex < 0)
            {
                MessageBox.Show("Please select a Variant.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // FOV validation - required for METS and all ILET variants
            string selectedType = cmbSystemType.SelectedItem?.ToString() ?? "";
            string selectedVariant = cmbVariant.SelectedItem?.ToString() ?? "";
            
            // FOV is required for METS (all variants) and ILET (variants 4, 5, 6)
            bool fovRequired = selectedType == "METS" || selectedType == "ILET";
            
            if (fovRequired && cmbFOV.SelectedIndex < 0)
            {
                MessageBox.Show("Please select a Field of View (FOV).", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // System components are now optional - users can generate ATP without components

            // CSV file is now optional - show warning if not uploaded
            if (string.IsNullOrEmpty(selectedPDFPath))
            {
                DialogResult result = MessageBox.Show(
                    "NOTE! No targets definition .csv file was uploaded!\nTargets table will have to be filled manually!",
                    "Warning - No CSV File",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Warning);
                
                if (result == DialogResult.Cancel)
                {
                    return; // User cancelled, don't generate ATP
                }
            }

            // Validate radiation source selections
            if (!cbBB.Checked && !cbIS.Checked && !cbLOSLaser.Checked && !cbBacklight.Checked && !cbQTHLamp.Checked)
            {
                MessageBox.Show("Please select at least one Radiation Source.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cbBB.Checked && cmbBBType.SelectedIndex < 0)
            {
                MessageBox.Show("Please select a B.B Type (RR or STD).", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cbBB.Checked && cmbBBType.SelectedIndex >= 0 && cmbBBSize.SelectedIndex < 0)
            {
                MessageBox.Show("Please select a B.B Size.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cbIS.Checked && cmbISExitAperture.SelectedIndex < 0)
            {
                MessageBox.Show("Please select an I.S Exit Aperture.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // LOS Laser no longer requires wavelength input

            if (cbFocusStage.Checked && 
                string.IsNullOrWhiteSpace(txtFocusStageFiniteDistance.Text) && 
                string.IsNullOrWhiteSpace(txtFocusStageFiniteDistance2.Text) && 
                string.IsNullOrWhiteSpace(txtFocusStageFiniteDistance3.Text))
            {
                MessageBox.Show("Please enter at least one Focus Stage Finite Distance.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cbGimbal.Checked && string.IsNullOrWhiteSpace(txtGimbalSize.Text))
            {
                MessageBox.Show("Please enter the Gimbal Size.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cbBacklight.Checked && cmbBacklightType.SelectedIndex < 0)
            {
                MessageBox.Show("Please select a Backlight Type (LED or Fiber Optic).", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cbNewPortStage.Checked && string.IsNullOrWhiteSpace(txtNewPortStageMaxWeight.Text))
            {
                MessageBox.Show("Please enter the NewPort Stage Max Weight.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Collect configuration
            ATPConfiguration config = new ATPConfiguration
            {
                SystemSN = txtSystemSN.Text.Trim(),
                Contract = txtContract.Text.Trim(),
                PMName = txtPMName.Text.Trim(),
                OrderNumber = txtOrderNumber.Text.Trim(),
                SystemType = cmbSystemType.SelectedItem?.ToString() ?? "",
                METSVariant = cmbVariant.SelectedItem?.ToString() ?? "",
                FOV = cmbFOV.SelectedItem?.ToString()?.Replace("\"", "") ?? "",
                HasSourceStage = cbSourceStage.Checked,
                HasRackmount = cbRackmount.Checked,
                HasGimbal = cbGimbal.Checked,
                GimbalSize = txtGimbalSize.Text.Trim(),
                HasLOSAlignmentTarget = cbLOSAlignmentTarget.Checked,
                HasTargetWheel = true, // Always included
                HasCTE = cbCTE.Checked,
                HasDeviceCenter = cbDeviceCenter.Checked,
                HasNewPortStage = cbNewPortStage.Checked,
                NewPortStageMaxWeight = txtNewPortStageMaxWeight.Text.Trim(),
                HasFocusStage = cbFocusStage.Checked,
                HasVRS = cbVRS.Checked,
                HasXYStage = cbXYStage.Checked,
                HasPowerMeter = cbPowerMeter.Checked,
                HasEnergyMeter = cbEnergyMeter.Checked,
                HasManualChoke = cbManualChoke.Checked,
                TargetWheelPDFPath = selectedPDFPath,
                FocusStageFiniteDistance = txtFocusStageFiniteDistance.Text.Trim(),
                FocusStageFiniteDistance2 = txtFocusStageFiniteDistance2.Text.Trim(),
                FocusStageFiniteDistance3 = txtFocusStageFiniteDistance3.Text.Trim(),
                VRS1 = cmbVRS1.SelectedIndex >= 0 ? cmbVRS1.SelectedItem.ToString() : "",
                VRS2 = cmbVRS2.SelectedIndex >= 0 ? cmbVRS2.SelectedItem.ToString() : "",
                VRS3 = cmbVRS3.SelectedIndex >= 0 ? cmbVRS3.SelectedItem.ToString() : "",
                HasBB = cbBB.Checked,
                HasIS = cbIS.Checked,
                HasLOSLaser = cbLOSLaser.Checked,
                HasBacklight = cbBacklight.Checked,
                BacklightType = cmbBacklightType.SelectedIndex >= 0 ? cmbBacklightType.SelectedItem.ToString() : "",
                HasQTHLamp = cbQTHLamp.Checked,
                BBType = cmbBBType.SelectedIndex >= 0 ? cmbBBType.SelectedItem.ToString() : "",
                BBSize = cmbBBSize.SelectedIndex >= 0 ? cmbBBSize.SelectedItem.ToString() : "",
                ISExitAperture = cmbISExitAperture.SelectedIndex >= 0 ? cmbISExitAperture.SelectedItem.ToString() : "",
                LOSLaserWavelength = "", // No longer used
                SavePath = savePath
            };

            // Generate ATP
            try
            {
                ATPGenerator generator = new ATPGenerator();
                string outputPath = generator.GenerateATP(config);
                
                MessageBox.Show($"ATP file generated successfully!\n\nSaved to: {outputPath}", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                // Ask if user wants to open the file
                if (MessageBox.Show("Would you like to open the generated ATP file?", "Open File", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = outputPath,
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating ATP file:\n\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }
}

