using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FTPTemp
{
    public partial class IndustryForm : Form
    {
        string mDbPath = Application.StartupPath + "/FTPDemo.sqlite";

        SQLiteConnection mConn;
        SQLiteDataAdapter mAdapter;
        DataTable mTable;

        public IndustryForm()
        {
            InitializeComponent();
        }

        private void IndustryForm_Load(object sender, EventArgs e)
        {
            dataGridView1.RowTemplate.Height = 60;

            // -------------------- Connecting To DB --------------------
            // ----------------------------------------------------------
            mConn = new SQLiteConnection("Data Source=" + mDbPath);

            // ----------------- Opening The Connection -----------------
            // ----------------------------------------------------------
            //mConn.Open();


            // --- Putting All Data From Selected Table To DataTable ---
            // ---------------------------------------------------------

            mAdapter = new SQLiteDataAdapter("SELECT ID, Name, Description, Occupation, Image, HourlyRate, PeopleCount, Round(HourlyRate*PeopleCount/3600.00,2) as 'Cost Per Sec' FROM [Industry]", mConn);
            //            mAdapter = new SQLiteDataAdapter("SELECT * FROM [Industry]", mConn);
            mTable = new DataTable();
            mAdapter.Fill(mTable);

            // ---------- Disabling Counter Field For Edition ----------
            // ---------------------------------------------------------
            if (mTable.Columns.Contains("id"))
            {
                mTable.Columns["id"].ReadOnly = true;
            }

            // ------------ Making DataBase Saving Changes -------------
            // ---------------------------------------------------------
            new SQLiteCommandBuilder(mAdapter);

            // ----------- Binding DataTable To DataGridView -----------
            // ---------------------------------------------------------
            dataGridView1.DataSource = mTable;

            // Zoom images
            for (int i = 0; i < dataGridView1.Columns.Count; i++)
            {
                if (dataGridView1.Columns[i] is DataGridViewImageColumn)
                {
                    ((DataGridViewImageColumn)dataGridView1.Columns[i]).ImageLayout = DataGridViewImageCellLayout.Zoom;
                    break;
                }
            }
            dataGridView1.Columns["ID"].Visible = false;
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
            this.Close();
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 4)
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
        }

        private void dataGridView1_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            //Skip the Column and Row headers
            if (e.ColumnIndex < 0 || e.RowIndex < 0)
            {
                return;
            }
            var dataGridView = (sender as DataGridView);
            //Check the condition as per the requirement casting the cell value to the appropriate type
            if (e.ColumnIndex == 4)
            {
                dataGridView.Cursor = Cursors.Hand;
            }
            else
            {
                dataGridView.Cursor = Cursors.Default;
            }
        }

    }
}
