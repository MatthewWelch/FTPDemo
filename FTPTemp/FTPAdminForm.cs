using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FTPTemp
{
    public partial class FTPAdminForm : Form
    {
        string mDbPath = Application.StartupPath + "/FTPDemo.sqlite";

        SQLiteConnection mConn;
        SQLiteDataAdapter mAdapter;
        DataTable mTable;

        SQLiteDataAdapter mAdapter1;
        DataTable mTable1;
        string demoDir = @"C:\demo\";

        public FTPAdminForm()
        {
            InitializeComponent();
        }

        private void FTPAdminForm_Load(object sender, EventArgs e)
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


            // ---------- Disabling Counter Field For Edition ----------
            // ---------------------------------------------------------
            if (mTable.Columns.Contains("id"))
            {
                mTable.Columns["id"].ReadOnly = true;
            }


            //textBox1.DataBindings.Add("Text", mTable, "Name", true, DataSourceUpdateMode.OnPropertyChanged);
            textBox2.DataBindings.Add("Text", mTable, "Description", true, DataSourceUpdateMode.OnPropertyChanged);
            textSize.DataBindings.Add("Text", mTable, "Filesize", true, DataSourceUpdateMode.OnPropertyChanged);
            textOverhead.DataBindings.Add("Text", mTable, "OverheadPct", true, DataSourceUpdateMode.OnPropertyChanged);
            textBox5.DataBindings.Add("Text", mTable, "AutoRestart", true, DataSourceUpdateMode.OnPropertyChanged);
            textBox6.DataBindings.Add("Text", mTable, "DelayStart", true, DataSourceUpdateMode.OnPropertyChanged);
            cbChangeIndustryOnRestart.DataBindings.Add("Checked", mTable, "ChangeIndustryOnRestart");

            setupCombobox(cbIndustry);
            setupCombobox(cbTheme);
//            setupCombobox("Industry");
//            setupCombobox("Theme");

            //--------------------------------------------------------------------------------------------------------------
            // --- Putting All Data From Selected Table To DataTable ---
            mAdapter1 = new SQLiteDataAdapter("SELECT * FROM [FTPConfiguration]", mConn);

            new SQLiteCommandBuilder(mAdapter1);

            mTable1 = new DataTable();
            mAdapter1.Fill(mTable1);


            // ---------- Disabling Counter Field For Edition ----------
            // ---------------------------------------------------------
            if (mTable1.Columns.Contains("id"))
            {
                mTable1.Columns["id"].ReadOnly = true;
            }


            // ------------ Making DataBase Saving Changes -------------
            // ---------------------------------------------------------
            new SQLiteCommandBuilder(mAdapter1);

            // ----------- Binding DataTable To DataGridView -----------
            // ---------------------------------------------------------
            dataGridView1.DataSource = mTable1;
            dataGridView1.Columns["ID"].Visible = false;

        }

        private void save()
        {
            // -------- Saving Modified Data To Selected Table ---------
            if (mAdapter == null) // If No Table Selected.
                return;

            this.BindingContext[mTable].EndCurrentEdit();
            //this.BindingContext[fTPDemoDataSet, "FTPDemo"].EndCurrentEdit();
            //this.BindingContext[this.fTPDemoDataSet.Tables["Industry"]].EndCurrentEdit();
            //this.BindingContext[this.fTPDemoDataSet.Tables[2]].EndCurrentEdit();
            //this.BindingContext[this.fTPDemoDataSet.Tables[3]].EndCurrentEdit();
            //this.BindingContext[this.fTPDemoDataSet.Tables[4]].EndCurrentEdit();
            //this.BindingContext[this.fTPDemoDataSet.Tables[0]].EndCurrentEdit();

            //bool rc = this.fTPDemoDataSet.HasChanges();
            mAdapter.Update(mTable);
//            mAdapter.Update(this.fTPDemoDataSet.FTPDemo);
//            this.fTPDemoTableAdapter.Update(this.fTPDemoDataSet.FTPDemo);

            this.BindingContext[mTable1].EndCurrentEdit();
            mAdapter1.Update(mTable1);

        }

        private void button1_Click(object sender, EventArgs e)
        {
            // -------- Saving Modified Data To Selected Table ---------
            this.save();
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //this.BindingContext[fTPDemoDataSet, "FTPDemo"].EndCurrentEdit();
            //this.BindingContext[fTPDemoDataSet, "FTPConfiguration"].EndCurrentEdit();
//            this.BindingContext[fTPDemoDataSet, "FTPDemo"].EndCurrentEdit();

            this.BindingContext[mTable].EndCurrentEdit();
            DataTable xDataTable = mTable.GetChanges();
//            DataTable xDataTable = this.fTPDemoDataSet.FTPDemo.GetChanges();
            DataTable xDataTable1 = mTable1.GetChanges();
            //if (xDataTable != null && xDataTable.Rows.Count > 0)
            //{ }
            if ((xDataTable != null && xDataTable.Rows.Count > 0) || (xDataTable1 != null && xDataTable1.Rows.Count > 0))
            {
                DialogResult retVal;
                retVal = MessageBox.Show("Save Changes?", "Exiting with pending changes", MessageBoxButtons.YesNoCancel);
                if (retVal == DialogResult.Yes)
                {
                    this.save();
                }
                if (retVal == DialogResult.Cancel)
                {
                    return;
                }
            }
            this.Close();


        }

        private void setupCombobox(ComboBox cbName)
        {
            mAdapter1 = new SQLiteDataAdapter("SELECT * FROM ["+cbName.Tag+"]", mConn);

            new SQLiteCommandBuilder(mAdapter1);

            mTable1 = new DataTable();
            mAdapter1.Fill(mTable1);


            // NOTE: This binding must be done after the main (mTable) is filled
            //BindingSource bsNames = new BindingSource();
            //bsNames.DataSource = mTable1;

            cbName.DataSource = mTable1;
            cbName.DisplayMember = "Name";
            cbName.ValueMember = "ID";
            //if (cbName.Text == "Industry")
            //{
                cbName.DataBindings.Add(new Binding("SelectedValue", mTable, cbName.Tag+"Code", true, DataSourceUpdateMode.OnPropertyChanged));
            //}
            //else
            //{
            //    cbName.DataBindings.Add(new Binding("SelectedValue", mTable, "ThemeCode", true, DataSourceUpdateMode.OnPropertyChanged));
            //}

            //if (name == "Industry")
            //{
            //    this.cbIndustry.DataSource = mTable1;
            //    this.cbIndustry.DisplayMember = "Name";
            //    this.cbIndustry.ValueMember = "ID";
            //    this.cbIndustry.DataBindings.Add(new Binding("SelectedValue", mTable, "IndustryCode", true, DataSourceUpdateMode.OnPropertyChanged));
            //}
            //else
            //{
            //    this.cbTheme.DataSource = mTable1;
            //    this.cbTheme.DisplayMember = "Name";
            //    this.cbTheme.ValueMember = "ID";
            //    this.cbTheme.DataBindings.Add(new Binding("SelectedValue", mTable, "ThemeCode", true, DataSourceUpdateMode.OnPropertyChanged));
            //}
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            IndustryForm frm = new IndustryForm();
            frm.ShowDialog();

        }
        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            DialogForm frm = new DialogForm();
            frm.ShowDialog();
        }

        private void calcSeconds()
        {
            bool rc;
            decimal dSec = 0;
            decimal dOverhead = 0;
            decimal filesize = 0;
            decimal dBandwidth = 0;

            rc = decimal.TryParse(textOverhead.Text, out dOverhead);
            if (rc == false)
            {
                dOverhead = 0; // default to no overhead if not supplied
                //    MessageBox.Show("Please change '% Overhead:'  to a valid number", "Notice", MessageBoxButtons.OK);
                //    return;
            }
            dOverhead = dOverhead / 100;
            rc = decimal.TryParse(textSize.Text, out filesize);
            if (rc == false)
            {
                //MessageBox.Show("Please change 'File Size:'  to a valid number", "Notice", MessageBoxButtons.OK);
                return;
            }
            //lSize = lSize * 8;      // convert Bytes into Bits

            //1073741824        GB to Byte
            //132073            MB to Byte
            //8                 Byte to bit
            //filesize *= 1073741824;
            filesize *= 1024 * 1024;    //131072;
            filesize *= 8;

            foreach (DataGridViewRow row in this.dataGridView1.Rows)
            {
                if (row.Cells[6].Value == null)
                {
                    MessageBox.Show("Please change 'Bandwidth'  to a valid number", "Notice", MessageBoxButtons.OK);
                    return;
                }
                rc = decimal.TryParse(row.Cells[6].Value.ToString(), out dBandwidth);
                if (rc == false)
                {
                    MessageBox.Show("Please change 'Bandwidth'  to a valid number", "Notice", MessageBoxButtons.OK);
                    return;
                }
            }

            foreach (DataGridViewRow row in this.dataGridView1.Rows)
            {
                rc = decimal.TryParse(row.Cells[6].Value.ToString(), out dBandwidth);
                dSec = filesize / ((1 - dOverhead) * dBandwidth * 1000000);
                dSec = Decimal.Round(dSec, 2);
                row.Cells[5].Value = dSec.ToString();
                //foreach (DataGridViewCell cell in row.Cells)
                //{
                //    if (cell.Size.IsEmpty)
                //    {
                //        continue;
                //    }
                //    MessageBox.Show(cell.Value.ToString());
                //}
            }

        }

        private void button4_Click(object sender, EventArgs e)
        {
            calcSeconds();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            //loadFile();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                this.Enabled = false;    // disable during copy
                string sExt = "1";
                //if (System.Environment.MachineName.Contains("Demo2"))
                if (System.Environment.MachineName.EndsWith("2"))   // if last char in machinename is 2
                {
                    sExt = "2";
                }
                string sName = "";
                int iPos;
                try
                {
                    for (int i = 1; i <= 5; i++)
                    {
                        iPos = openFileDialog1.SafeFileName.LastIndexOf(".");
                        if (iPos > 0)
                        {
                            sName = "FTPfile" + sExt + i.ToString() + openFileDialog1.SafeFileName.Substring(iPos);
                            //sName = openFileDialog1.SafeFileName.Substring(0, iPos) + sExt + i.ToString() + openFileDialog1.SafeFileName.Substring(iPos);
                        }
                        File.Copy(openFileDialog1.FileName, demoDir + sName, true);
                        //dataGridView1.Rows[i - 1].Cells[3].Value = sName;
                        dataGridView1.Rows[i - 1].Cells["Filename"].Value = sName;
                    }
                }
                catch (IOException ex)
                {
                    MessageBox.Show("Error Copying files: " + ex.Message);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error files: " + ex.Message);
                }
                finally
                {
                    this.Enabled = true;    // enable after copy
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {

        }
    }
}
