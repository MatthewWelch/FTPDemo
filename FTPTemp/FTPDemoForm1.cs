using FTPTemp.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FTPTemp
{
    public partial class FTPDemoForm1 : Form
    {
        List<FTPConfiguration> ftpconfigurations = new List<FTPConfiguration>();
        FTPDemo ftpdemo = new FTPDemo();
        Industry industry = new Industry();
        Theme theme = new Theme();

        string mDbPath = Application.StartupPath + "/FTPDemo.sqlite";

        SQLiteConnection mConn;
        SQLiteDataAdapter mAdapter;
        DataTable mTable;

        private int _lastFormSize;

        //string[] h = new string[] { "10.10.20.1", "10.10.30.1", "10.10.40.1", "10.10.50.1", "10.10.60.1" };
        //string[] u = new string[] { "2MEG", "10MEG", "100MEG", "500MEG", "10GIG" };
        //string[] p = new string[] { "demo", "demo", "demo", "demo", "demo" };
        //string[] f = new string[] { "thb2m.mp4v", "thb10m.mp4v", "thb100m.mp4v", "thb500m.mp4v", "thb5g.mp4v" };
        //string[] d = new string[] { "thb2m.mp4v", "thb10m.mp4v", "thb100m.mp4v", "thb500m.mp4v", "thb5g.mp4v" };
        //string[] s = new string[] { "1660", "332", "166", "33", "4" };      // default # of sec (simulate)
        //string[] z = new string[] { "0", "0", "0", "0", "0" };              // default to 0, if 1 then simulate (in REAL FTP mode)

        //string demoLogo = Properties.Settings.Default.DemoLogo;
        string demoPhoto = "FTPIndustry.jpg";
        //string demoSize = Properties.Settings.Default.DemoSize + "MB";
        //string demoDiagram = Properties.Settings.Default.DemoDiagram;
        string demoDir = @"C:\demo\";
        //string demoAutoRestartString = Properties.Settings.Default.DemoAutoRestart;
        //string demoDelayStartString = Properties.Settings.Default.DemoDelayStart;
        //string demoHeader = Properties.Settings.Default.DemoHeader;

        ////Int32  //.T .TryParse( demoAutoRestartString, out demoAutoRestart);
        //Image img = Properties.Resources.comcast;                    //pictureBox1.Image;

        string TimeElapsed = "";
        //bool ibSimulate = false;            // whether to actually FTP the file or simulate it (01/06/14)
        //bool isTest = false;
        DateTime startTime;
        private bool _running = false;      // is the process running
        private CancellationTokenSource cts = new CancellationTokenSource();
        //long fileSize;
        object l = new object();     // used for locking 
        //        public CancellationTokenSource cts;
        //WebClient client;
        //Color primaryColor = ColorTranslator.FromHtml("#d91e26");         // Ciena
        Color primaryColor = Color.FromArgb(0, 91, 187);                    // Comcast      // Color.RgoyalBlue;
        //#if COMPILE_RED
        //        Color primaryColor = ColorTranslator.FromHtml("#d91e26");         // Ciena
        //#else
        //        Color primaryColor = Color.FromArgb(0, 91, 187);                    // Comcast      // Color.RgoyalBlue;
        //#endif
        Stopwatch stopwatch;
        long ctime = 0, ptime = 0, current = 0, previous = 0;
        long ctime1, ctime2, ctime3, ctime4, ctime5 = 0;        // used for Progresschanged events
        long ptime1, ptime2, ptime3, ptime4, ptime5 = 0;

        System.Threading.Timer timer1;
        bool stopStartClick = false;                            // if user access admin screen before button click is performed to start process then this will be set to True
        bool inStartDelay = false;                              // if true we're in asynch delay before starting Process

        public FTPDemoForm1()
        {
            InitializeComponent();
        }

        private async void FTPDemoForm1_Load(object sender, EventArgs e)
        {
            timer1 = new System.Threading.Timer(new TimerCallback(showElapsedTime));
            tableLayoutPanel1.BackColor = Color.Transparent;    // transparent so backgroung gradient shows
            menuStrip1.BackColor = Color.Transparent;           // transparent so backgroung gradient shows

            getData();
            SetUI();

            // fix 9/26/14 add task delay here to allow access to Admin beforeFTP process starts
            

            inStartDelay = true;    // we are delaying start (if user click admin screen during this period, process is stopped until admin screen closes)
            await TaskDelay(ftpdemo.DelayStart * 1000);
            inStartDelay = false;    // done delaying start

            start();
        }

        private void FTPDemoForm1_FormClosing(object sender, FormClosingEventArgs e)
        {        // User is closing the form, stop any work if we have some running:
            if (_running)
            {
                cts.Cancel(); // stop loop:
                // System.Threading.Thread.Sleep(1000);  // give tasks time to complete:
            }
        }

        private void getData()
        {
            bool allSimMode = true;     // if all FTPConfiguratios at set to Simulate filesize from Admin screen (not any actual filesize)
            // -------------------- Connecting To DB --------------------
            using (mConn = new SQLiteConnection("Data Source=" + mDbPath))
            {

                // ----------------- Opening The Connection -----------------
                mConn.Open();
                using (SQLiteCommand fmd = mConn.CreateCommand())
                {
                    try
                    {

                        // Get FTPConfiguration data
                        fmd.CommandText = @"SELECT ID, Name, Description, Host, Username, Password, Duration, Bandwidth, Simulate, Filename FROM [FTPConfiguration]";
                        fmd.CommandType = CommandType.Text;
                        SQLiteDataReader r = fmd.ExecuteReader();
                        while (r.Read())
                        {
                            FTPConfiguration f = new FTPConfiguration(Convert.ToInt16(r["ID"]),
                                        Convert.ToString(r["Name"]),
                                        Convert.ToString(r["Description"]),
                                        Convert.ToString(r["Host"]),
                                        Convert.ToString(r["Username"]),
                                        Convert.ToString(r["Password"]),
                                        r["Duration"] == DBNull.Value ? 0 : Convert.ToInt64(r["Duration"]),
                                        Convert.ToString(r["Bandwidth"]),
                                        r["Simulate"] == DBNull.Value ? false : Convert.ToBoolean(r["Simulate"]),
                                        Convert.ToString(r["Filename"]));

                            ftpconfigurations.Add(f);      // add to list
                            if (f.Simulate == false)
                            {
                                allSimMode = false;
                            }
                        }

                        r.Close();
                        fmd.Cancel();

                        // Get FTPDemo data
                        fmd.CommandText = @"SELECT ID, Name, Description, Filesize, OverheadPct, AutoRestart, DelayStart, ThemeCode, IndustryCode, ChangeIndustryOnRestart FROM [FTPDemo]";
                        fmd.CommandType = CommandType.Text;
                        r = fmd.ExecuteReader();
                        //SQLiteDataReader r = fmd.ExecuteReader();
                        while (r.Read())
                        {
                            ftpdemo = new FTPDemo(Convert.ToInt16(r["ID"]),
                                        Convert.ToString(r["Name"]),
                                        Convert.ToString(r["Description"]),
                                        r["Filesize"] == DBNull.Value ? 0 : Convert.ToInt64(r["Filesize"]),
                                        r["OverheadPct"] == DBNull.Value ? 0 : Convert.ToInt16(r["OverheadPct"]),
                                        r["AutoRestart"] == DBNull.Value ? 0 : Convert.ToInt16(r["AutoRestart"]),
                                        r["DelayStart"] == DBNull.Value ? 0 : Convert.ToInt16(r["DelayStart"]),
                                        r["ThemeCode"] == DBNull.Value ? 0 : Convert.ToInt16(r["ThemeCode"]),
                                        r["IndustryCode"] == DBNull.Value ? 0 : Convert.ToInt16(r["IndustryCode"]),
                                        r["ChangeIndustryOnRestart"] == DBNull.Value ? false : Convert.ToBoolean(r["ChangeIndustryOnRestart"])
                                );
                        }

                        r.Close();
                        fmd.Cancel();

                        // Get Industry data
                        fmd.CommandText = @"SELECT ID, Name, Description, Occupation, Image, HourlyRate, PeopleCount FROM [Industry] Where [ID] = @IndustryCode";
                        fmd.CommandType = CommandType.Text;
                        fmd.Parameters.Add(new SQLiteParameter("@IndustryCode", ftpdemo.IndustryCode));
                        //                        fmd.Parameters.Add(new SQLiteParameter("@IndustryCode", SqlDbType.Int) { Value = ftpdemo.IndustryCode });
                        r = fmd.ExecuteReader();
                        //SQLiteDataReader r = fmd.ExecuteReader();
                        while (r.Read())
                        {
                            industry = new Industry(Convert.ToInt16(r["ID"]),
                                        Convert.ToString(r["Name"]),
                                        Convert.ToString(r["Description"]),
                                        Convert.ToString(r["Occupation"]),
                                        Convert.ToString(r["Image"]),
                                        r["HourlyRate"] == DBNull.Value ? 0 : Convert.ToInt16(r["HourlyRate"]),
                                        r["PeopleCount"] == DBNull.Value ? 0 : Convert.ToInt16(r["PeopleCount"])
                                );

                             byte[] imgdata = (byte[])r["Image"];
                             if (imgdata != null)
                             {
                                 File.WriteAllBytes(demoDir + "FTPIndustry.jpg", imgdata);
                             }

                            ////                          pictureBox2.Image = ByteToImage((byte[])r["Image"]);

                            //// Converting An Image To Array Of Bytes
                            //ImageConverter converter = new ImageConverter();
                            //pictureBox2.Image = (Bitmap)converter.ConvertTo((byte[])r["Image"], typeof(Bitmap));

                        }
                        r.Close();
                        fmd.Cancel();

                        //                        // Get Industry data
                        //                        fmd.CommandText = @"SELECT Image FROM [Industry] Where [ID] = @IndustryCode";
                        //                        fmd.CommandType = CommandType.Text;
                        //                        fmd.Parameters.Add(new SQLiteParameter("@IndustryCode", ftpdemo.IndustryCode));
                        //                        //                        fmd.Parameters.Add(new SQLiteParameter("@IndustryCode", SqlDbType.Int) { Value = ftpdemo.IndustryCode });
                        //                        r = fmd.ExecuteReader();
                        //                        //SQLiteDataReader r = fmd.ExecuteReader();
                        //                        while (r.Read())
                        //                        {
                        //                            pictureBox2.Image = ByteToImage(GetBytes(r));
                        //                        }
                        //                        r.Close();
                        //                        fmd.Cancel();
                        // Get Theme data
                        fmd.CommandText = @"SELECT ID, Name, Description, Logo, BackgroundColor, ProgressbarColor, TextColor FROM [Theme] Where [ID] = @ThemeCode";
                        fmd.CommandType = CommandType.Text;
                        fmd.Parameters.Add(new SQLiteParameter("@ThemeCode", ftpdemo.ThemeCode));
                        r = fmd.ExecuteReader();


                        //SQLiteDataReader r = fmd.ExecuteReader();
                        while (r.Read())
                        {

                            theme = new Theme(Convert.ToInt16(r["ID"]),
                                        Convert.ToString(r["Name"]),
                                        Convert.ToString(r["Description"]),
                                        Convert.ToString(r["Logo"]),
                                        Convert.ToString(r["BackgroundColor"]),
                                        Convert.ToString(r["ProgressbarColor"]),
                                        Convert.ToString(r["TextColor"])
                                );
                            pictureLogo.Image = ByteToImage((byte[])r["Logo"]);
                            //pictureLogo.ImageLocation = @"C:\\demo\\image.png";

                        }
                        r.Close();
                        fmd.Cancel();

                    }
                    catch (Exception ex)
                    {
                        ReportError(ex.Message, 0);
                    }
                } // end of using command
            } // end of using connection

            FileInfo myFile = new FileInfo(@demoDir + ftpconfigurations[0].Filename);

            // if file does not exist or AllSimMode use file size from Admin screen (FTPDemo)
            if (!myFile.Exists || allSimMode)
            {
                if (ftpdemo.Description.Length > 0)
                {
                    lFileSize.Text = ftpdemo.Description +" "+ ftpdemo.Filesize.ToString() +"MB";
                }
                else
                {
                    lFileSize.Text = "This Demonstration shows five file transfers at five different speeds. The file size is " + ftpdemo.Filesize.ToString() + "MB";
                }

            }
            else
            {
                if (ftpdemo.Description.Length > 0)
                {
                    lFileSize.Text = ftpdemo.Description + Utils.BytesToString(myFile.Length);
                }
                else
                {
                    lFileSize.Text = "This Demonstration shows five file transfers at five different speeds. The file size is " + Utils.BytesToString(myFile.Length);
                }
            }

            //pictureBox1.Image = theme.Logo;
        }

        public static Bitmap ByteToImage(byte[] blob)
        {
            MemoryStream mStream = new MemoryStream();
            byte[] pData = blob;
            mStream.Write(pData, 0, Convert.ToInt32(pData.Length));
            Bitmap bm = new Bitmap(mStream, false);
            mStream.Dispose();
            return bm;

        }

        private void updateNextIndustry()
        {
            using (mConn = new SQLiteConnection("Data Source=" + mDbPath))
            {

                // ----------------- Opening The Connection -----------------
                mConn.Open();
                using (SQLiteCommand fmd = mConn.CreateCommand())
                {
                    try
                    {
                        int nextIndustryID=-1;
                        // Get FTPConfiguration data
                        fmd.CommandText = @"SELECT id FROM industry WHERE id > @ID ORDER BY ID ASC LIMIT 1";
                        fmd.CommandType = CommandType.Text;
                        fmd.Parameters.Add(new SQLiteParameter("@ID", ftpdemo.IndustryCode));

                        SQLiteDataReader r = fmd.ExecuteReader();
                        while (r.Read())
                        {
                            nextIndustryID = Convert.ToInt16(r["ID"]);
                        }
                        r.Close();
                        fmd.Cancel();
                        if (nextIndustryID <= 0)
                        {
                            fmd.CommandText = @"SELECT id FROM industry ORDER BY ID ASC LIMIT 1";
                            fmd.CommandType = CommandType.Text;

                            r = fmd.ExecuteReader();
                            while (r.Read())
                            {
                                nextIndustryID = Convert.ToInt16(r["ID"]);
                            }
                            r.Close();
                            fmd.Cancel();
                        }

                        if (nextIndustryID != null && nextIndustryID > 0)
                        {
                            // Update IndustryCode in FTPDemo
                            fmd.CommandText = @"Update FTPDemo Set IndustryCode = @IndustryCode WHERE ID = @ID";
                            fmd.CommandType = CommandType.Text;
                            fmd.Parameters.Add(new SQLiteParameter("@ID", ftpdemo.ID));
                            fmd.Parameters.Add(new SQLiteParameter("@IndustryCode", nextIndustryID));

                            int rows = fmd.ExecuteNonQuery();
                            //rows number of record got updated
                        }
                        

                    }
                                    catch (Exception ex)
                    {
                        ReportError(ex.Message, 0);
                    }
                }

            }
        }

        private void showElapsedTime(object obj)
        {
            TimeSpan elapsed = DateTime.Now.Subtract(startTime);
            //it executes every second 
            ltimer1.Invoke((Action)delegate
            {
                ltimer1.Text = (int)elapsed.TotalSeconds + " seconds elapsed";

                // autorestart 01/9/14
                //if (elapsed.TotalSeconds >= (demoAutoRestart) && demoAutoRestart > 0)
                if (ftpdemo.AutoRestart > 0 && elapsed.TotalSeconds >= (ftpdemo.AutoRestart))
                {
                    autoRestart();
                }
                //DateTime.Now.ToLongTimeString() + "," + DateTime.Now.ToLongDateString();
            });
        }

        private void start()
        {
            listBox1.Items.Clear();       // clear all items from list
            StartMain(cts);               // all
        }

        public void StartMain(CancellationTokenSource cts)
        {
            ResetUI();

            Task t1 = Task.Factory.StartNew(() =>
            {
                var progress = new Progress<string>();

                var options = new ParallelOptions();
                //                CancellationTokenSource _cts = new CancellationTokenSource();
                CancellationToken token = cts.Token;
                options.CancellationToken = token;
                options.CancellationToken.ThrowIfCancellationRequested();
                options.MaxDegreeOfParallelism = Environment.ProcessorCount;

                //--- Record the start time---
                startTime = DateTime.Now;
                try
                {
                    startTime = DateTime.Now;
                    timer1.Change(0, 1000); //enable
                    //for (int i = 0; i < 5; i++)
                    //Parallel.For(0, 4, options, (i) =>
                    Parallel.For(0, 5, options, (i) =>
                    {

                            if (ftpconfigurations[i].Simulate == true)    // from FTPDemo.csv, option to simulate in REAL FTP mode
                            {
                                decimal iSeconds = 0;
                                // Note delay is in Millaseconds
                                iSeconds = Convert.ToDecimal(ftpconfigurations[i].Duration) * 1000;
                                MyDelayAsync(i, iSeconds, cts.Token);      // this will upate progressbar async
                            }
                            else
                            {
                                //using (WebClient client = new WebClient())
                                //{
                                MyMainAsync(i, ftpconfigurations[i], cts.Token, progress);
                                //}
                            }
                    });
                    // timer1.Change(Timeout.Infinite, Timeout.Infinite);  //stop

                }
                catch (OperationCanceledException oce)
                { /*loop canceled*/
                    //                        string s = oce.CancellationToken.ToString();
                    //                        return;
                }
                catch (AggregateException ae)
                { /*something else*/
                    //                        int z = 1;
                }


            });

            //t1.Wait();
            //t1.Result;
            //Task.WaitAll();
            //label11.Text = "Done...";

            Task t2 = t1.ContinueWith((antecedent) =>
            {
                //if (antecedent.IsCanceled)
                //{   label11.Text = "Cancelled.";        }
                //else
                //{   label11.Text = "Done...";           }

                // code to update UI once simulation task is finished:
                //label11.Text = "Done...";
                button1.Text = "Start";
                button1.Enabled = true;
                _running = false;
                //timer1.Change(Timeout.Infinite, Timeout.Infinite);
            },
            TaskScheduler.FromCurrentSynchronizationContext()
            );

        }

 //       public async Task<string> MyMainAsync(int i, string h, string u, string p, string f, CancellationToken cancel, IProgress<string> progress)
        public async Task<string> MyMainAsync(int i, FTPConfiguration ftpconfiguration, CancellationToken cancel, IProgress<string> progress)
        {
            using (WebClient client = new WebClient())
            {
                try
                {
                    //--- Record the start time---
                    startTime = DateTime.Now;
                    // First create a Stopwatch instance
                    stopwatch = new Stopwatch();
                    ctime1 = ctime2 = ctime3 = ctime4 = ctime5 = ptime1 = ptime2 = ptime3 = ptime4 = ptime5 = 0;    // zero out each run
                    // Begin the stopwatch
                    stopwatch.Start();
                    cancel.ThrowIfCancellationRequested();
                    if (cancel.IsCancellationRequested)
                    {
                        client.CancelAsync();
                    }
                    // Get the object used to communicate with the server.
                    //                WebClient client = new WebClient();
                    client.Proxy = null;
                    client.BaseAddress = "ftp://" + ftpconfiguration.Host;
// TODO: put back in /demo/ when doing hardwired for real
//                    client.BaseAddress = "ftp://" + ftpconfiguration.Host + "/demo/";     

                    CredentialCache cache = new CredentialCache();
                    NetworkCredential credential = new NetworkCredential(ftpconfiguration.Username, ftpconfiguration.Password);

                    client.Credentials = credential;

                    if (i == 0)
                    { client.UploadProgressChanged += new UploadProgressChangedEventHandler(client_UploadProgressChanged1); }
                    else if (i == 1)
                    { client.UploadProgressChanged += new UploadProgressChangedEventHandler(client_UploadProgressChanged2); }
                    else if (i == 2)
                    { client.UploadProgressChanged += new UploadProgressChangedEventHandler(client_UploadProgressChanged3); }
                    else if (i == 3)
                    { client.UploadProgressChanged += new UploadProgressChangedEventHandler(client_UploadProgressChanged4); }
                    else if (i == 4)
                    { client.UploadProgressChanged += new UploadProgressChangedEventHandler(client_UploadProgressChanged5); }

                    //client.UploadProgressChanged += new UploadProgressChangedEventHandler(client_UploadProgressChanged); 
                    //client.UploadFileCompleted += new UploadFileCompletedEventHandler(client_UploadFileCompleted);
                    Uri remoteFile;
                    Uri.TryCreate(ftpconfiguration.Filename, System.UriKind.Relative, out remoteFile);

                    FileInfo fi = new FileInfo(@demoDir + ftpconfiguration.Filename);
                    //fileSize = fi.Length;
                    //                await client.DownloadFileTaskAsync(remoteFile, "/temp/demo/RRVideo.mp4");

                    Task uploadTask = client.UploadFileTaskAsync(remoteFile, @demoDir + ftpconfiguration.Filename);
                    //string sNull = null;
                    //client.UploadFileAsync(remoteFile, sNull, @demoDir + f, i.ToString());
                    //client.UploadFile(remoteFile, @demoDir + f);

                    //if (i == 3)
                    //{
                    //   await MyDelayAsync(i);
                    //}
                    await uploadTask;
                    ////update the progress on UI
//                  label1.Invoke((Action)delegate { ReportProgress2(1, i, Convert.ToString(fi.Length) + " bytes sent", 0); });

                    if (i == 0)     // stop Elapsed timer when last Task done.
                    {
                        label1.Invoke((Action)delegate { timer1.Change(Timeout.Infinite, Timeout.Infinite); });
                    }


                    if (cancel.IsCancellationRequested)
                    {
                        client.CancelAsync();
                    }
                    //showTime();

                }
                catch (OperationCanceledException oce)
                {
                    client.CancelAsync();
                    //label1.Text = "Cancelled";
                }
                catch (Exception e)
                {
                    ReportError(e.Message, i);
                }
                return "end";
            }
        }

        void client_UploadProgressChanged1(object sender, UploadProgressChangedEventArgs e)
        {
            try
            {
                int value = e.ProgressPercentage;           //(int)((decimal)e.BytesSent / (decimal)e.TotalBytesToSend * 100);
                //update the progress on UI
                label1.Invoke((Action)delegate { ReportProgress1(1, 0, e.BytesSent.ToString() + " bytes sent", value); });

                //// get the elapsed time in milliseconds
                //ctime1 = stopwatch.ElapsedMilliseconds;
                //// 
                //if (ctime1 - ptime1 > 1400)     // only display byts sent every 5 sec
                //{
                //    label2.Invoke((Action)delegate { ReportProgress2(1, 0, e.BytesSent.ToString() + " bytes sent", value); });
                //    ptime1 = ctime1;
                //}

            }
            catch (Exception ee)
            { ltime1.Text = ee.Message; }
        }
        void client_UploadProgressChanged2(object sender, UploadProgressChangedEventArgs e)
        {
            try
            {
                int value = e.ProgressPercentage;           //(int)((decimal)e.BytesSent / (decimal)e.TotalBytesToSend * 100);
                //update the progress on UI
                label1.Invoke((Action)delegate { ReportProgress1(1, 1, e.BytesSent.ToString() + " bytes sent", value); });

                //// get the elapsed time in milliseconds
                //ctime2 = stopwatch.ElapsedMilliseconds;
                //// 
                //if (ctime2 - ptime2 > 1100)
                //{
                //    label2.Invoke((Action)delegate { ReportProgress2(1, 1, e.BytesSent.ToString() + " bytes sent", value); });
                //    ptime2 = ctime2;
                //}
            }
            catch (Exception ee)
            { ltime2.Text = ee.Message; }
        }
        void client_UploadProgressChanged3(object sender, UploadProgressChangedEventArgs e)
        {
            try
            {
                int value = e.ProgressPercentage;           //(int)((decimal)e.BytesSent / (decimal)e.TotalBytesToSend * 100);
                //update the progress on UI
                label1.Invoke((Action)delegate { ReportProgress1(1, 2, e.BytesSent.ToString() + " bytes sent", value); });

                //// get the elapsed time in milliseconds
                //ctime3 = stopwatch.ElapsedMilliseconds;
                //// 
                //if (ctime3 - ptime3 > 700)
                //{
                //    label2.Invoke((Action)delegate { ReportProgress2(1, 2, e.BytesSent.ToString() + " bytes sent", value); });
                //    ptime3 = ctime3;
                //}

            }
            catch (Exception ee)
            { ltime3.Text = ee.Message; }
        }
        void client_UploadProgressChanged4(object sender, UploadProgressChangedEventArgs e)
        {
            try
            {
                int value = e.ProgressPercentage;           //(int)((decimal)e.BytesSent / (decimal)e.TotalBytesToSend * 100);
                //update the progress on UI
                label1.Invoke((Action)delegate { ReportProgress1(1, 3, e.BytesSent.ToString() + " bytes sent", value); });

                //// get the elapsed time in milliseconds
                //ctime4 = stopwatch.ElapsedMilliseconds;
                //if (ctime4 - ptime4 > 300)
                //{
                //    label2.Invoke((Action)delegate { ReportProgress2(1, 3, e.BytesSent.ToString() + " bytes sent", value); });
                //    ptime4 = ctime4;
                //}

            }
            catch (Exception ee)
            { ltime4.Text = ee.Message; }
        }
        void client_UploadProgressChanged5(object sender, UploadProgressChangedEventArgs e)
        {
            try
            {

                int value = e.ProgressPercentage;           // (int)((decimal)e.BytesSent / (decimal)e.TotalBytesToSend * 100);

                //update the progress on UI
                label1.Invoke((Action)delegate { ReportProgress1(1, 4, e.BytesSent.ToString() + " bytes sent", value); });

                //// get the elapsed time in milliseconds
                //ctime5 = stopwatch.ElapsedMilliseconds;
                //if (ctime5 - ptime5 > 125)
                //{

                //    double dn = (double)e.BytesSent / 1024.0 / stopwatch.Elapsed.TotalSeconds;             //(DateTime.Now - start).TotalSeconds;
                //    string sSpeed = (dn.ToString("n") + " KB/s) " + e.ProgressPercentage);

                //    label2.Invoke((Action)delegate { ReportProgress2(1, 4, e.BytesSent.ToString() + " bytes sent" + " | "+sSpeed, value); });
                //    ptime5 = ctime5;
                //}

            }
            catch (Exception ee)
            { ltime5.Text = ee.Message; }
        }

        void client_UploadFileCompleted(object sender, UploadFileCompletedEventArgs e)
        {
            try
            {
                if (e.Cancelled)
                {
                    label12.Text = "Cancelled";
                }
                //label1.Invoke((Action)delegate { ReportProgress2(1, 4, e.Result.ToString() + " bytes sent", 0); });
            }
            catch (Exception ee)
            {
                label12.Text = ee.Message;
            }
        }

        private void ReportProgress1(long status, int i, string desc, int value)
        {
            try
            {
                ////            int value = ((int)status * 100) / 5;
                //int jloops = Convert.ToInt16(lloops.Text);
                ////int value = ((int)status * 100) / jloops;
                ////Note the time consumed here
                TimeSpan elapsed = DateTime.Now.Subtract(startTime);
                TimeElapsed = (int)elapsed.TotalSeconds + " s : " + elapsed.Milliseconds + " ms";

                switch (i)
                {
                    case 0:          //"thb2m.mp4v":
                        progressBar1.Value = value;
                        scaleableImageCtrl1.SetOpacity((double)value / 100);

                        //pictureBox1.Image = Picture.SetImageOpacity(img, (float)value/100 );
                        //ltime1.Text = TimeElapsed + " | " + desc + " | Count: " + status.ToString();   // + " | " + desc;
                        ltime1.Text = TimeElapsed + " | " + Utils.calcTotalCost(industry.PeopleCount, industry.HourlyRate, elapsed);
                        break;
                    case 1:          //"thb10m.mp4v":
                        progressBar2.Value = value;
                        scaleableImageCtrl2.SetOpacity((double)value / 100);

                        //pictureBox2.Image = Picture.SetImageOpacity(img, (float)value/100 );
                        //ltime2.Text = TimeElapsed + " | " + desc + " | Count: " + status.ToString();
                        ltime2.Text = TimeElapsed + " | " + Utils.calcTotalCost(industry.PeopleCount, industry.HourlyRate, elapsed);
                        break;
                    case 2:         //"thb100m.mp4v":
                        progressBar3.Value = value;
                        scaleableImageCtrl3.SetOpacity((double)value / 100);

                        //pictureBox3.Image = Picture.SetImageOpacity(img, (float)value/100 );
                        //ltime3.Text = TimeElapsed + " |" + desc + " | Count: " + status.ToString();
                        ltime3.Text = TimeElapsed + " | " + Utils.calcTotalCost(industry.PeopleCount, industry.HourlyRate, elapsed);
                        break;
                    case 3:         //"thb500m.mp4v":
                        progressBar4.Value = value;
                        scaleableImageCtrl4.SetOpacity((double)value / 100);

                        //pictureBox4.Image = Picture.SetImageOpacity(img, (float)value/100 );
                        //ltime4.Text = TimeElapsed + " |" + desc + " | Count: " + status.ToString();
                        //ltime4.Text = (elapsed.TotalSeconds* 500).ToString("$ 0.##");
                        ltime4.Text = TimeElapsed + " | " + Utils.calcTotalCost(industry.PeopleCount, industry.HourlyRate, elapsed);
                        break;
                    case 4:         //"thb5g.mp4v":
                        progressBar5.Value = value;
                        scaleableImageCtrl5.SetOpacity((double)value / 100);

                        //pictureBox5.Image = Picture.SetImageOpacity(img, (float)value/100 );
                        ltime5.Text = TimeElapsed + " | " + Utils.calcTotalCost(industry.PeopleCount, industry.HourlyRate, elapsed);
                        break;
                }
            }
            catch (Exception e)
            {
                ReportError(e.Message, i);
            }
        }

        private void ReportError(string message, int i)
        {
            //label12.Text = "Error: " + message + "from file # "+ i.ToString();
            lock (l) { listBox1.Items.Insert(0, "Error: " + message + " from file # " + i.ToString()); }
            //            listBox1.Items.Insert(0, "Error: " + message + "from file # " + i.ToString()); 

        }
        private void ReportProgress2(long status, int i, string desc, int value)
        {
            try
            {
                //            int value = ((int)status * 100) / 5;
                int jloops = Convert.ToInt16(lloops.Text);
                //int value = ((int)status * 100) / jloops;
                //Note the time consumed here
                TimeSpan elapsed = DateTime.Now.Subtract(startTime);
                TimeElapsed = (int)elapsed.TotalSeconds + " s : " + elapsed.Milliseconds + " ms";

                switch (i)
                {
                    case 0:          //"thb2m.mp4v":
                        //progressBar1.Value = value;
                        // lock (l) {ltime1.Text = TimeElapsed + " | " + desc + " | Count: " + status.ToString();}   // + " | " + desc;
                        lock (l) { ltime1.Text = TimeElapsed; }  //+ " | " + desc; }
                        break;
                    case 1:          //"thb10m.mp4v":
                        //progressBar2.Value = value;
                        lock (l) { ltime2.Text = TimeElapsed; }  //+ " | " + desc; }
                        break;
                    case 2:         //"thb100m.mp4v":
                        //progressBar3.Value = value;
                        lock (l) { ltime3.Text = TimeElapsed; }  // +" |" + desc; }
                        break;
                    case 3:         //"thb500m.mp4v":
                        //progressBar4.Value = value;
                        lock (l) { ltime4.Text = TimeElapsed; }  // +" |" + desc; }
                        break;
                    case 4:         //"thb5g.mp4v":
                        //progressBar5.Value = value;
                        lock (l) { ltime5.Text = TimeElapsed; }  // +" |" + desc; }
                        break;
                }
            }
            catch (Exception e)
            {
                ReportError(e.Message, i);
            }

        }


 
        private async Task<int> MyDelayAsync(int i, decimal iMs, CancellationToken ct)
        {
            int iRet = 0;
            try
            {
                int iDelay = 0;
                if (iMs > 0)
                { iDelay = (int)(iMs / 100); }      // cast as int
                for (int j = 1; j <= 100; j++)
                {
                    if (ct.IsCancellationRequested)
                    {
                        ct.ThrowIfCancellationRequested();
                    }
                    await Task.Delay(iDelay, ct);       // include cancellation 01/09/14
                    label1.Invoke((Action)delegate { ReportProgress1(1, i, " bytes sent", j * 1); });
                }
                //update the progress on UI
//                label1.Invoke((Action)delegate { ReportProgress2(1, i, " bytes sent", 0); });
                if (i == 0)     // stop Elapsed timer when last Task done.
                {
                    label1.Invoke((Action)delegate { timer1.Change(Timeout.Infinite, Timeout.Infinite); });
                    // enable start button
                    label1.Invoke((Action)delegate { button1.Enabled = true; });
                }

            }
            catch (OperationCanceledException oce)
            {
                // cancelled since MyMain() sync method finished; progress at 100%
                label1.Invoke((Action)delegate { ReportProgress1(1, i, " bytes sent", 100); });
                //return 0;
            }
            catch (Exception e)
            {
                ReportError(e.Message, i);
            }
            return iRet;
        }

        private string getElapsedTime()
        {
            TimeSpan elapsed = DateTime.Now.Subtract(startTime);
            string TimeElapsed = (int)elapsed.TotalSeconds + " s : " + elapsed.Milliseconds + " ms";
            return TimeElapsed;
        }
        private void showTime()
        {
            TimeSpan elapsed = DateTime.Now.Subtract(startTime);
            string TimeElapsed = (int)elapsed.TotalSeconds + " s : " + elapsed.Milliseconds + " ms";
            label1.Text = TimeElapsed;
        }


        private int autoRestart()
        {
            cts.Cancel(); // stop loop:

            if (ftpdemo.ChangeIndustryOnRestart)
            {
                updateNextIndustry();
            }
            Application.Restart();      // restart process
            return 0;
        }

        void SetUI()
        {
            //-----------------------------------------------------------------------------
            //this.Height = 800;
            //this.Width = 1000;
            // Retrieve the working rectangle from the Screen class
            // using the PrimaryScreen and the WorkingArea properties.
            System.Drawing.Rectangle workingRectangle = Screen.PrimaryScreen.WorkingArea;

            // Set the size of the form slightly less than size of 
            // working rectangle.
            this.Size = new System.Drawing.Size(
                workingRectangle.Width - 50, workingRectangle.Height - 50);

            // Set the location so the entire form is visible.
            this.Location = new System.Drawing.Point(25, 25);
            //-----------------------------------------------------------------------------

            label1.Text = ftpconfigurations[0].Description;
            label2.Text = ftpconfigurations[1].Description;
            label3.Text = ftpconfigurations[2].Description;
            label4.Text = ftpconfigurations[3].Description;
            label5.Text = ftpconfigurations[4].Description;
            ltimer1.Text = "";
            lIndustry.Text = industry.Name;

            //progressBar1.ForeColor = progressBar2.ForeColor = progressBar3.ForeColor = progressBar4.ForeColor = progressBar5.ForeColor = System.Drawing.ColorTranslator.FromHtml("#d91e26");
            progressBar1.ForeColor = progressBar2.ForeColor = progressBar3.ForeColor = progressBar4.ForeColor = progressBar5.ForeColor = primaryColor;          // Color.FromArgb(0, 91, 187);  // Color.RgoyalBlue;
            progressBar1.Style = progressBar2.Style = progressBar3.Style = progressBar4.Style = progressBar5.Style = ProgressBarStyle.Continuous;
//            pictureLogo.ImageLocation = demoDir + demoLogo;
//
//            if (!File.Exists(demoDir + demoLogo))
//            { ReportError("Error: Missing File " + demoDir + demoLogo + " ", 0); }
            if (!File.Exists(demoDir + demoPhoto))
            { ReportError("Error: Missing File " + demoDir + demoPhoto + " ", 0); }

        }
        void ResetUI()
        {
            try
            {
                //lblResult.Text = "";
                progressBar1.Value = progressBar2.Value = progressBar3.Value = progressBar4.Value = progressBar5.Value = 0;
                ltime1.Text = ltime2.Text = ltime3.Text = ltime4.Text = ltime5.Text = "";
                //pictureBox1.Image = pictureBox2.Image = pictureBox3.Image = pictureBox4.Image = pictureBox5.Image = null;
                //label11.Text = label12.Text = "";
                scaleableImageCtrl1.SetOpacity(0);
                scaleableImageCtrl2.SetOpacity(0);
                scaleableImageCtrl3.SetOpacity(0);
                scaleableImageCtrl4.SetOpacity(0);
                scaleableImageCtrl5.SetOpacity(0);
                scaleableImageCtrl1.SetSource(demoDir + demoPhoto);
                scaleableImageCtrl2.SetSource(demoDir + demoPhoto);
                scaleableImageCtrl3.SetSource(demoDir + demoPhoto);
                scaleableImageCtrl4.SetSource(demoDir + demoPhoto);
                scaleableImageCtrl5.SetSource(demoDir + demoPhoto);
            }
            catch (Exception e)
            { ReportError(e.Message, 0); }
        }

        public async Task TaskDelay(int mSec)           // util function to sleep for a period w/o holding iu thread
        {
            await Task.Delay(mSec);
            //txtConsole.AppendText("Waiting...");
            //DoStuff();
        }

        private void adminToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // added 9/26/14, allow to process not to start if Admin screen is activated
            if (inStartDelay)
            {
                stopStartClick = true;
            }

            //var frm = new Form8();
            //frm.Location = this.Location;
            //frm.StartPosition = FormStartPosition.Manual;
            //frm.FormClosing += delegate { this.backFromForm8(); };
            //frm.Show();

            FTPAdminForm frm = new FTPAdminForm();
            frm.ShowDialog();
        }

        private void logToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string msg = "";
            int cnt = listBox1.Items.Count;
            var items = new System.Collections.ArrayList(listBox1.Items);
            foreach (var item in items)
            {
                msg += item + Environment.NewLine;
            }
            if (items.Count > 0)
            {
                msg += Environment.NewLine + "Note: If message is 'An exception occurred during a WebClient request' then File is not selected. To do this: Click on file folder, Click Browse File, Select file, Open and Save.";
                msg += Environment.NewLine + "Note: If message is 'Unable to connect to remote server' Then check if the network is properly configured.";
            }
            else
            {
                msg += "No messages received.";
            }
            MessageBox.Show(msg);
        }

        private void restartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Restart();
        }

    }   // Form
}       // namespace
