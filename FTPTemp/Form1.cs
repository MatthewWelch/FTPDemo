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
    public partial class Form1 : Form
    {
        string mDbPath = Application.StartupPath + "/FTPDemo.sqlite";

        SQLiteConnection mConn;
        SQLiteDataAdapter mAdapter;
        DataTable mTable;

        SQLiteDataAdapter mAdapter1;
        DataTable mTable1;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // -------------------- Connecting To DB --------------------
            mConn = new SQLiteConnection("Data Source=" + mDbPath);

            // ----------------- Opening The Connection -----------------
            mConn.Open();





            // --- Putting All Data From Selected Table To DataTable ---
            mAdapter = new SQLiteDataAdapter("SELECT * FROM [FTPDemo]", mConn);

            new SQLiteCommandBuilder(mAdapter);

            mTable = new DataTable();
            mAdapter.Fill(mTable);
            // done with main table

            mAdapter1 = new SQLiteDataAdapter("SELECT * FROM [Industry]", mConn);

            new SQLiteCommandBuilder(mAdapter1);

            mTable1 = new DataTable();
            mAdapter1.Fill(mTable1);

            this.cbIndustry.DataSource = mTable1;
            this.cbIndustry.DisplayMember = "Name";
            this.cbIndustry.ValueMember = "ID";

            // NOTE: This binding must be done after the main (mTable) is filled
            BindingSource bsIndustries = new BindingSource();
            bsIndustries.DataSource = mTable1;

            this.cbIndustry.DataBindings.Add(new Binding("SelectedValue", mTable, "IndustryCode", true, DataSourceUpdateMode.OnPropertyChanged));
            //
            mAdapter1 = new SQLiteDataAdapter("SELECT * FROM [Theme]", mConn);

            new SQLiteCommandBuilder(mAdapter1);

            mTable1 = new DataTable();
            mAdapter1.Fill(mTable1);

            this.cbTheme.DataSource = mTable1;
            this.cbTheme.DisplayMember = "Name";
            this.cbTheme.ValueMember = "ID";

            // NOTE: This binding must be done after the main (mTable) is filled
            BindingSource bsThemes = new BindingSource();
            bsThemes.DataSource = mTable1;

            this.cbTheme.DataBindings.Add(new Binding("SelectedValue", mTable, "ThemeCode", true, DataSourceUpdateMode.OnPropertyChanged));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.BindingContext[mTable].EndCurrentEdit();
            mAdapter.Update(mTable);

        }
    }
}
