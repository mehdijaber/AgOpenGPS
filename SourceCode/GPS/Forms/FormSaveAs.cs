using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace AgOpenGPS
{
    public partial class FormSaveAs : Form
    {
        //class variables
        private readonly FormGPS mf = null;

        public FormSaveAs(Form _callingForm)
        {
            //get copy of the calling main form
            mf = _callingForm as FormGPS;

            InitializeComponent();

            label1.Text = gStr.gsEnterFieldName;
            label3.Text = gStr.gsBasedOnField;

            this.Text = gStr.gsSaveAs;
            lblTemplateChosen.Text = gStr.gsNoneUsed;
        }

        private void FormSaveAs_Load(object sender, EventArgs e)
        {
            btnSave.Enabled = false;
            lblTemplateChosen.Text = Properties.Settings.Default.setF_CurrentDir;
            lblFilename.Text = "";
            mf.CloseTopMosts();
        }

        private void tboxFieldName_TextChanged(object sender, EventArgs e)
        {
            TextBox textboxSender = (TextBox)sender;
            int cursorPosition = textboxSender.SelectionStart;
            textboxSender.Text = Regex.Replace(textboxSender.Text, glm.fileRegex, "");
            textboxSender.SelectionStart = cursorPosition;

            if (String.IsNullOrEmpty(tboxFieldName.Text.Trim()))
            {
                btnSave.Enabled = false;
            }
            else
            {
                btnSave.Enabled = true;
            }

            lblFilename.Text = tboxFieldName.Text.Trim();
            lblFilename.Text += " " + DateTime.Now.ToString("MMM.dd", CultureInfo.InvariantCulture);
            lblFilename.Text += " " + DateTime.Now.ToString("HH_mm", CultureInfo.InvariantCulture);
        }

        private void btnSerialCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            //fill something in
            if (String.IsNullOrEmpty(tboxFieldName.Text.Trim()))
            {
                Close();
                return;
            }

            //append date time to name

            mf.currentFieldDirectory = Path.Combine(tboxFieldName.Text.Trim(), " ");

            //date
            if (cboxAddDate.Checked) mf.currentFieldDirectory += " " + DateTime.Now.ToString("MMM.dd", CultureInfo.InvariantCulture);
            if (cboxAddTime.Checked) mf.currentFieldDirectory += " " + DateTime.Now.ToString("HH_mm", CultureInfo.InvariantCulture);

            //get the directory and make sure it exists, create if not
            string directoryName = Path.Combine(mf.fieldsDirectory, mf.currentFieldDirectory);

            mf.menustripLanguage.Enabled = false;

            mf.displayFieldName = mf.currentFieldDirectory;

            if ((!string.IsNullOrEmpty(directoryName)) && (Directory.Exists(directoryName)))
            {
                MessageBox.Show(gStr.gsChooseADifferentName, gStr.gsDirectoryExists, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }
            else
            {
                //create the new directory
                if ((!string.IsNullOrEmpty(directoryName)) && (!Directory.Exists(directoryName)))
                { Directory.CreateDirectory(directoryName); }
            }

            string line;
            string offsets, convergence, startFix;

            using (StreamReader reader = new StreamReader(Path.Combine(mf.fieldsDirectory, lblTemplateChosen.Text, "Field.txt")))
            {
                try
                {
                    line = reader.ReadLine();
                    line = reader.ReadLine();
                    line = reader.ReadLine();
                    line = reader.ReadLine();

                    //read the Offsets  - all we really need from template field file
                    offsets = reader.ReadLine();

                    line = reader.ReadLine();
                    convergence = reader.ReadLine();

                    line = reader.ReadLine();
                    startFix = reader.ReadLine();
                }
                catch (Exception ex)
                {
                    mf.WriteErrorLog("While Opening Field" + ex);

                    FormTimedMessage form = new FormTimedMessage(2000, gStr.gsFieldFileIsCorrupt, gStr.gsChooseADifferentField);
                    form.Show(this);
                    mf.JobClose();
                    return;
                }

                const string myFileName = "Field.txt";

                using (StreamWriter writer = new StreamWriter(Path.Combine(directoryName, myFileName)))
                {
                    //Write out the date
                    writer.WriteLine(DateTime.Now.ToString("yyyy-MMMM-dd hh:mm:ss tt", CultureInfo.InvariantCulture));

                    writer.WriteLine("$FieldDir");
                    writer.WriteLine(mf.currentFieldDirectory.ToString(CultureInfo.InvariantCulture));

                    //write out the easting and northing Offsets
                    writer.WriteLine("$Offsets");
                    writer.WriteLine(offsets);

                    writer.WriteLine("$Convergence");
                    writer.WriteLine(convergence);

                    writer.WriteLine("StartFix");
                    writer.WriteLine(startFix);
                }

                //create txt file copies
                string templateDirectoryName = Path.Combine(mf.fieldsDirectory, lblTemplateChosen.Text);
                string fileToCopy = "";
                string destinationDirectory = "";

                if (chkApplied.Checked)
                {
                    fileToCopy = Path.Combine(templateDirectoryName, "Contour.txt");
                    destinationDirectory = Path.Combine(directoryName, "Contour.txt");
                    if (File.Exists(fileToCopy))
                        File.Copy(fileToCopy, destinationDirectory);

                    fileToCopy = Path.Combine(templateDirectoryName, "Sections.txt");
                    destinationDirectory = Path.Combine(directoryName, "Sections.txt");
                    if (File.Exists(fileToCopy))
                        File.Copy(fileToCopy, destinationDirectory);
                }

                else
                {
                    //create blank Contour and Section files
                    mf.FileCreateSections();
                    mf.FileCreateContour();
                    //mf.FileCreateElevation();
                }

                fileToCopy = Path.Combine(templateDirectoryName, "Boundary.txt");
                destinationDirectory = Path.Combine(directoryName, "Boundary.txt");
                if (File.Exists(fileToCopy))
                    File.Copy(fileToCopy, destinationDirectory);

                if (chkFlags.Checked)
                {
                    fileToCopy = Path.Combine(templateDirectoryName, "Flags.txt");
                    destinationDirectory = Path.Combine(directoryName, "Flags.txt");
                    if (File.Exists(fileToCopy))
                        File.Copy(fileToCopy, destinationDirectory);
                }
                else
                {
                    mf.FileSaveFlags();
                }

                if (chkGuidanceLines.Checked)
                {
                    fileToCopy = Path.Combine(templateDirectoryName, "ABLines.txt");
                    destinationDirectory = Path.Combine(directoryName, "ABLines.txt");
                    if (File.Exists(fileToCopy))
                        File.Copy(fileToCopy, destinationDirectory);

                    fileToCopy = Path.Combine(templateDirectoryName, "RecPath.txt");
                    destinationDirectory = Path.Combine(directoryName, "RecPath.txt");
                    if (File.Exists(fileToCopy))
                        File.Copy(fileToCopy, destinationDirectory);

                    fileToCopy = Path.Combine(templateDirectoryName, "CurveLines.txt");
                    destinationDirectory = Path.Combine(directoryName, "CurveLines.txt");
                    if (File.Exists(fileToCopy))
                        File.Copy(fileToCopy, destinationDirectory);
                }
                else
                {
                    mf.FileSaveABLines();
                    mf.FileSaveCurveLines();
                }

                if (chkHeadland.Checked)
                {
                    fileToCopy = Path.Combine(templateDirectoryName, "Headland.txt");
                    destinationDirectory = Path.Combine(directoryName, "Headland.txt");
                    if (File.Exists(fileToCopy))
                        File.Copy(fileToCopy, destinationDirectory);
                }
                else
                    mf.FileSaveHeadland();

                //fileToCopy = Path.Combine(templateDirectoryName, "Elevation.txt");
                //destinationDirectory = Path.Combine(directoryName, "Elevation.txt");
                //if (File.Exists(fileToCopy))
                //    File.Copy(fileToCopy, destinationDirectory);

                //now open the newly cloned field
                mf.FileOpenField(Path.Combine(directoryName, myFileName));
                mf.displayFieldName = mf.currentFieldDirectory;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void tboxFieldName_Click(object sender, EventArgs e)
        {
            if (mf.isKeyboardOn)
            {
                mf.KeyboardToText((TextBox)sender, this);
                btnSerialCancel.Focus();
            }
        }

        private void tboxTask_Click(object sender, EventArgs e)
        {
            if (mf.isKeyboardOn)
            {
                mf.KeyboardToText((TextBox)sender, this);
                btnSerialCancel.Focus();
            }
        }

        private void tboxVehicle_Click(object sender, EventArgs e)
        {
            if (mf.isKeyboardOn)
            {
                mf.KeyboardToText((TextBox)sender, this);
                btnSerialCancel.Focus();
            }
        }
    }
}