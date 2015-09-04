using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FTPTemp
{
    public partial class DialogForm : Form
    {
        string mDbPath = Application.StartupPath + "/FTPDemo.sqlite";

        SQLiteConnection mConn;
        SQLiteDataAdapter mAdapter;
        DataTable mTable;
        public DialogForm()
        {
            InitializeComponent();
        }

        private void DialogForm_Load(object sender, EventArgs e)
        {

            string configSetting = ConfigurationManager.AppSettings["AllowAddDeleteThemes"] ;
            bool allowAddDelete;
            if (Boolean.TryParse(configSetting, out allowAddDelete))        // if its a viable boolean app.config setting
            {
                //bool configAllowAddDelete = Convert.ToBoolean( ConfigurationManager.AppSettings["AllowAddDeleteThemes"] );
                dataGridView1.AllowUserToAddRows = allowAddDelete;
                dataGridView1.AllowUserToDeleteRows = allowAddDelete;
            }


            dataGridView1.RowTemplate.Height = 60;

            // -------------------- Connecting To DB --------------------
            mConn = new SQLiteConnection("Data Source=" + mDbPath);

            // ----------------- Opening The Connection -----------------
            mConn.Open();


            // --- Putting All Data From Selected Table To DataTable ---
            mAdapter = new SQLiteDataAdapter("SELECT ID, Name, Logo, BackgroundColor, ProgressbarColor, TextColor FROM [Theme]", mConn);

            mTable = new DataTable();
            mAdapter.Fill(mTable);
            // ------------ Making DataBase Saving Changes -------------
            // ---------------------------------------------------------
            new SQLiteCommandBuilder(mAdapter);

            // ----------- Binding DataTable To DataGridView -----------
            // ---------------------------------------------------------
            dataGridView1.DataSource = mTable;

            // Zoom images to fit
            for (int i = 0; i < dataGridView1.Columns.Count; i++)
            {
                if (dataGridView1.Columns[i] is DataGridViewImageColumn)
                {
                    ((DataGridViewImageColumn)dataGridView1.Columns[i]).ImageLayout = DataGridViewImageCellLayout.Zoom;
                    //break;
                }

            }
            // get rid of default red x
            dataGridView1.Columns["Logo"].DefaultCellStyle.NullValue = null;

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                row.Cells["BackgroundColor"].Style.BackColor = Utils.getColorFromValue(row, "BackgroundColor");
                row.Cells["ProgressbarColor"].Style.BackColor = Utils.getColorFromValue(row, "ProgressbarColor");
                row.Cells["TextColor"].Style.BackColor = Utils.getColorFromValue(row, "TextColor");
                
            }

            dataGridView1.Columns["ID"].Visible = false;

            // ---------- Disabling Counter Field For Edition ----------
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // -------- Saving Modified Data To Selected Table ---------
            if (mAdapter == null) // If No Table Selected.
                return;

            mAdapter.Update(mTable);
            this.Close();
        
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();       // exit
        
        }

        private void dataGridView1_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
             //Skip the Column and Row headers
            if (e.ColumnIndex < 0 || e.RowIndex < 0)
            {
                return;
            }
            var dataGridView = (sender as DataGridView);
            // show image as hand to give visual clue to open dialog to pick image file or Color
            if ((e.ColumnIndex == 2) || (e.ColumnIndex >= 3 && e.ColumnIndex <= 5))
            {
                dataGridView.Cursor = Cursors.Hand;
            }
            else
            {
                dataGridView.Cursor = Cursors.Default;
            }
        
        }

        private void dataGridView1_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
        if (!e.Row.IsNewRow)
            {
                DialogResult response = MessageBox.Show("Are you sure?", "Delete row?",
                         MessageBoxButtons.YesNo,
                         MessageBoxIcon.Question,
                         MessageBoxDefaultButton.Button2);
                if (response == DialogResult.No)
                    e.Cancel = true;
            }
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
        if (e.ColumnIndex == 2)
            {
                // -------------- Loading An Image From File ---------------
                // ------------------- As Bytes Array ----------------------

                var ofd = new OpenFileDialog();

                if (ofd.ShowDialog() != DialogResult.OK)
                    return;

                Image img = Image.FromFile(ofd.FileName);

                // Converting An Image To Array Of Bytes
                ImageConverter converter = new ImageConverter();
                byte[] imgBytes = (byte[])converter.ConvertTo(img, typeof(byte[]));

                // Removing Image From RAM
                // Also, you can use "uses" keyword for Auto Dispose
                img.Dispose();
                dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = imgBytes;
                ((DataGridViewImageColumn)dataGridView1.Columns[e.ColumnIndex]).ImageLayout = DataGridViewImageCellLayout.Zoom;


            }

            if (e.ColumnIndex > 2 && e.ColumnIndex < 6)
            {

                ColorDialog cd = new ColorDialog();
                if (cd.ShowDialog() == DialogResult.OK)
                {
                    dataGridView1.SelectedCells[0].Style.BackColor = cd.Color;
                    //                dataGridView1.SelectedCells[0].Style.ForeColor = cd.Color;
                    //                dataGridView1.SelectedCells[0].Style.SelectionForeColor = dataGridView1.SelectedCells[0].Style.SelectionBackColor;
                    dataGridView1.SelectedCells[0].Value = cd.Color.Name;
                }
            }
        }
    }
}
