using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WindowsFormsApplication1
{
    class FtpFiles
    {
        private int _startTime = 0;
        private BackgroundWorker _worker = null;

        private CancellationTokenSource _cts;
        private int _files = 5;
        private int _pixels = 0;

    		public void sendFiles(Object sender, DoWorkEventArgs e)
		{
			_worker = (BackgroundWorker)sender;

			_startTime = System.Environment.TickCount;

			//
			// now start computing Mandelbrot set, row by row:
			//
			//for (int r = 0; r < _pixels; r++)
			//
			_cts = new CancellationTokenSource();
			var options = new ParallelOptions();
			options.CancellationToken = _cts.Token;

			try
			{
				Parallel.For(0, 1, options, (r) =>
					{
						//
						// Since we need to pass the new pixel values to the UI thread for display,
						// we allocate a new array so we can keep running in parallel (versus having
						// to wait for the array to become available for the next set of values).
						//
						int[] values = new int[_pixels];  // one row:

						//for (int c = 0; c < _pixels; ++c)
						Parallel.For(0, _files, (c) =>
							{
								values[c] = sendFile(r, c);
							}
						);

						//
						// Set value in last 5 pixels of each row to a thread id so we can "see" who
						// computed this row:
						//
						int threadID = System.Threading.Thread.CurrentThread.ManagedThreadId;  // .NET thread id:

						for (int c = _pixels - 5; c < _pixels; c++)
							values[c] = -threadID;

						//
						// we've generated a row, report this as progress for display:
						//
						_worker.ReportProgress(r, new object[] { r, values });
					}
				);
			}
			catch(OperationCanceledException)
			{
				// tell background worker that work was cancelled:
				e.Cancel = true;
			}
		}


        		//
		// Returns a color reflecting the value of the Mandelbrot set element at this position.
		//
		private int sendFile(int iloop, int ifile)
		{
            int irc = 0;
			//
			// compute pixel position:
			//
			//double ypos = y + size * (yp - pixels / 2) / ((double)pixels);
			//double xpos = x + size * (xp - pixels / 2) / ((double)pixels);

            return irc;
        }



            //
            // Call to cancel calculation:
            //
            public void CancelCalculate()
            {
                _cts.Cancel();
            }

    }   // class

    class FileData
    {
        public string hostname { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string filename { get; set; }
        public string description { get; set; }
        public int    seconds { get; set; }

        /// <summary>
        /// Constructor, allocate balance history for months, assign overdraft
        /// </summary>
        public FileData(string h, string u,string p, string f)
        {
        hostname = h;
        username = u;
        password = p;
        filename = f;
        }

        public FileData(string h, string u, string p, string f, string d)
        {
            hostname = h;
            username = u;
            password = p;
            filename = f;
            description = d;
        }

        public FileData(string h, string u, string p, string f, string d, int s)
        {
            hostname = h;
            username = u;
            password = p;
            filename = f;
            description = d;
            seconds = s;
        }
    }   // class

    //class Utils
    //{

    //public static FileData GetFiles()
    //    {
    //    List<FileData> files;

    //    files.Add( new FileData( "10.10.20.1", "2MEG", "demo", "thb2m.mp4v"));
    //    files.Add( new FileData( "10.10.20.1", "2MEG", "demo", "thb2m.mp4v"));
    //    files.Add( new FileData( "10.10.30.1", "10MEG", "demo", "thb10m.mp4v"));
    //    files.Add( new FileData( "10.10.40.1", "100MEG","demo", "thb100m.mp4v"));
    //    files.Add( new FileData( "10.10.50.1", "500MEG", "demo", "thb500m.mp4v")); 
    //    files.Add( new FileData( "10.10.60.1", "10GIG", "demo", "thb5g.mp4v"));

    //    return files;
    //    }


    //}   // class
}   // namespace
