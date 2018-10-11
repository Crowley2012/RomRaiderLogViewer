using Newtonsoft.Json;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace RomRaiderLogViewer
{
    public partial class MainForm : Form
    {
        #region Private Fields

        private string[] _columns;
        private string[] _lines;

        #endregion Private Fields

        #region Public Constructors

        public MainForm()
        {
            InitializeComponent();
        }

        public MainForm(string path)
        {
            InitializeComponent();

            lblFileName.Text = Path.GetFileName(path);
            _lines = File.ReadAllLines(path);

            BuildGrid();
        }

        #endregion Public Constructors

        #region Events

        private void BuildGraph_Click(object sender, EventArgs e)
        {
            BuildGraph();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            lblBuild.Text = $"{version.Build}.{version.Revision}";

            cmbGridType.DataSource = Enum.GetValues(typeof(SeriesChartType)).Cast<SeriesChartType>().OrderBy(x => x.ToString()).ToList();
            cmbGridType.SelectedItem = SeriesChartType.Line;

            ThreadStarter(new Thread(new ThreadStart(CheckUpdate)));
        }

        private void Open_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*";

                if (dialog.ShowDialog() != DialogResult.OK)
                    return;

                lblFileName.Text = dialog.SafeFileName;
                _lines = File.ReadAllLines(dialog.FileName);

                BuildGrid();
            }
        }

        #endregion Events

        #region Private Methods

        private void BuildGraph()
        {
            //Chart type
            Enum.TryParse(cmbGridType.SelectedValue.ToString(), out SeriesChartType chartType);

            //Check valid columns selected to graph
            if ((chartType == SeriesChartType.ThreeLineBreak
                || chartType == SeriesChartType.PointAndFigure
                || chartType == SeriesChartType.Kagi
                || chartType == SeriesChartType.Renko)
                && checkedListBox.CheckedItems.Count > 1)
            {
                MessageBox.Show("This chart type only accepts a single column", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            //Reset graph
            chart.ResetAutoValues();
            chart.Series.Clear();

            //Build series for each selected column
            foreach (var item in checkedListBox.CheckedItems)
            {
                var series = new Series
                {
                    Name = item.ToString(),
                    IsVisibleInLegend = true,
                    IsXValueIndexed = true,
                    ChartType = chartType
                };

                chart.Series.Add(series);

                //Graph only selected rows
                if (cbSelectedRowsOnly.Checked)
                {
                    foreach (DataGridViewRow line in dataGridView1.SelectedRows)
                        series.Points.AddXY(line.Cells[0].Value, line.Cells[item.ToString()].Value);
                }
                else
                {
                    foreach (DataGridViewRow line in dataGridView1.Rows)
                        series.Points.AddXY(line.Cells[0].Value, line.Cells[item.ToString()].Value);
                }
            }

            chart.Invalidate();
        }

        private void BuildGrid()
        {
            var dataTable = new DataTable();
            _columns = new string[0];

            //Clear check list
            checkedListBox.Items.Clear();

            //Check all lines to find which row has the most columns
            foreach (var line in _lines)
            {
                var split = line.Split(',');

                if (split.Length > _columns.Length)
                    _columns = split;
            }

            //Add columns to table and check list
            foreach (var column in _columns)
            {
                dataTable.Columns.Add(column);
                checkedListBox.Items.Add(column);
            }

            //Add rows
            foreach (var line in _lines)
            {
                var split = line.Split(',');

                if (double.TryParse(split[0], out double x))
                    dataTable.Rows.Add(split);
            }

            dataGridView1.DataSource = dataTable;
        }

        private void CheckUpdate()
        {
            bool checkingUpdate = true;
            int attempts = 0;

            //Check for updates with a max attempt of 3
            while (checkingUpdate && attempts < 3)
            {
                try
                {
                    using (var webClient = new WebClient())
                    {
                        webClient.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2;)");

                        //Get latest release data from Github
                        var release = JsonConvert.DeserializeObject<ApiGithub>(webClient.DownloadString("https://api.github.com/repos/Crowley2012/RomRaiderLogViewer/releases/latest"));
                        var currentVersion = new Version(Assembly.GetExecutingAssembly().GetName().Version.ToString());
                        var latestVersion = new Version(release.tag_name);
                        var updateAvailable = currentVersion.CompareTo(latestVersion) < 0;

                        //Prompt user if there is an update
                        if (updateAvailable && MessageBox.Show($"Download new version?\n\nCurrent Version: {currentVersion}\nLatest Version {latestVersion}", "Update Available", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk) == DialogResult.Yes)
                            System.Diagnostics.Process.Start("https://github.com/Crowley2012/RomRaiderLogViewer/releases/latest");

                        checkingUpdate = false;
                        Invoke((MethodInvoker)delegate { lblUpdate.Text = updateAvailable ? $"Update Available [{latestVersion}]" : string.Empty; });
                    }
                }
                catch (WebException)
                {
                    attempts++;
                }
            }
        }

        private void ThreadStarter(Thread thread)
        {
            thread.IsBackground = true;
            thread.Start();
        }

        #endregion Private Methods
    }
}