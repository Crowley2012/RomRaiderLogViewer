using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;

namespace RomRaiderLogViewer
{
    public partial class FrmAbout : Form
    {
        #region Constructor

        public FrmAbout()
        {
            InitializeComponent();
        }

        #endregion Constructor

        #region Events

        private void GithubProject_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/Crowley2012/RomRaiderLogViewer");
        }

        private void PopupAbout_Load(object sender, EventArgs e)
        {
            txtVersion.Text = $"Version: {Assembly.GetExecutingAssembly().GetName().Version}";
        }

        #endregion Events
    }
}