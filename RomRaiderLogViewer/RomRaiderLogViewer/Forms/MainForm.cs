using System;
using System.Reflection;
using System.Windows.Forms;

namespace RomRaiderLogViewer
{
    public partial class MainForm : Form
    {
        #region Public Constructors

        public MainForm()
        {
            InitializeComponent();
        }

        #endregion Public Constructors

        #region Events

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            lblBuild.Text = $"{version.Build}.{version.Revision}";
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*";

                if (dialog.ShowDialog() != DialogResult.OK)
                    return;

                lblFileName.Text = dialog.SafeFileName;
            }
        }

        #endregion Events
    }
}