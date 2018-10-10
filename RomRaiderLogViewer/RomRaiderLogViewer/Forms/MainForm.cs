using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
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

        #endregion Public Constructors

        #region Events

        private void MainForm_Load(object sender, EventArgs e)
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            lblBuild.Text = $"{version.Build}.{version.Revision}";

            cmbGridType.DataSource = Enum.GetValues(typeof(SeriesChartType)).Cast<SeriesChartType>().OrderBy(x => x.ToString()).ToList();
            cmbGridType.SelectedItem = SeriesChartType.Line;
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

        private void BuildGraph_Click(object sender, EventArgs e)
        {
            BuildGraph();
        }

        #endregion Events

        #region Private Methods

        private void BuildGraph()
        {
            //Chart type
            Enum.TryParse(cmbGridType.SelectedValue.ToString(), out SeriesChartType chartType);

            //Check valid columns selected to graph
            if (chartType == SeriesChartType.ThreeLineBreak && checkedListBox.CheckedItems.Count > 1)
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

        #endregion Private Methods
    }
}