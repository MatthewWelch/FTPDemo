using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTPTemp.Models
{
    class FTPDemo
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Logo { get; set; }
        public long Filesize { get; set; }
        public int OverheadPct { get; set; }
        public int AutoRestart { get; set; }
        public int DelayStart { get; set; }
        public int ThemeCode { get; set; }
        public int IndustryCode { get; set; }
        public bool ChangeIndustryOnRestart { get; set; }

        public FTPDemo() { }        // empty constructor

        public FTPDemo(int id,
        string name,
        string description,
            //string logo,
        long filesize,
        int overheadpct,
        int autorestart,
        int delaystart,
        int themecode,
        int industrycode,
        bool changeindustryonrestart)
        {
            this.ID = id;
            this.Name = name;
            this.Description = description;
            //this.Logo = logo;
            this.Filesize = filesize;
            this.OverheadPct = overheadpct;
            this.AutoRestart = autorestart;
            this.DelayStart = delaystart;
            this.ThemeCode = themecode;
            this.IndustryCode = industrycode;
            this.ChangeIndustryOnRestart = changeindustryonrestart;
        }
    }
}
