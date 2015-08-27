using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTPTemp.Models
{
    class Theme
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Logo { get; set; }
        public string BackgroundColor { get; set; }
        public string ProgressbarColor { get; set; }
        public string TextColor { get; set; }

    public Theme() {}               // empty contructor
    public Theme(int id,
        string name,
        string description,
        string logo,
        string backgroundcolor,
        string progressbarcolor,
        string textcolor)
        { 
        this.ID = id;
        this.Name = name;
        this.Description = description;
        this.Logo = logo;
        this.ProgressbarColor = progressbarcolor;
        this.TextColor = textcolor;
        }
    }
}
