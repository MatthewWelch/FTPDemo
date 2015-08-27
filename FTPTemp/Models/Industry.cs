using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTPTemp.Models
{
    class Industry
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Occupation { get; set; }
        public string Image { get; set; }
        public int HourlyRate { get; set; }
        public int PeopleCount { get; set; }

        public Industry() { }       // empty constructor
        public Industry(int id,
        string name,
        string description,
        string occupation,
        string image,
        int hourlyrate,
        int peoplecount)
        {
            this.ID = id;
            this.Name = name;
            this.Description = description;
            this.Occupation = occupation;
            this.Image = image;
            this.HourlyRate = hourlyrate;
            this.PeopleCount = peoplecount;
        }
    }
}
