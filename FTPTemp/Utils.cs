using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FTPTemp
{
    class Utils
    {
        public static Color getColorFromValue(DataGridViewRow row, String column)
        {
            Color rc = Color.Empty;
            try
            {
                if (row.Cells[column].Value != null)
                {
                    Color c = Color.FromName(row.Cells[column].Value.ToString());
                    if (c.IsKnownColor)
                    {
                        rc = c;
                    }
                    else
                    {
                        rc = ColorTranslator.FromHtml("#" + row.Cells[column].Value.ToString());
                    }
                }
            }
            catch (Exception e)
            { }
            return rc;
        }

        // display human readable file size
        public static String BytesToString(long byteCount)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
            if (byteCount == 0)
                return "0" + suf[0];
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num).ToString() + suf[place];
        }

        
        public static String calcTotalCost(int peopleCount, int hourlyRate, TimeSpan elapsedTime)
//        public static String calcTotalCost(int peopleCount, int hourlyRate, int pctComplete)
        {
            String rc="";
            if (elapsedTime.TotalHours != 0 )
            {
                rc = (peopleCount * hourlyRate * elapsedTime.TotalHours).ToString("$ 0.##");    
            }
            return rc;
        }
    }
}
