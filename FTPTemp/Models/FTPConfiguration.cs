using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTPTemp.Models
{
    public class FTPConfiguration
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Host { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public long Duration { get; set; }
        public string Bandwidth { get; set; }
        public bool Simulate { get; set; }
        public string Filename { get; set; }

        public FTPConfiguration(int id,
                          string name,
                          string description,
                          string host,
                          string username,
                          string password,
                          long duration,
                          string bandwidth,
                          bool simulate,
                          string filename)
        {
            this.ID = id;
            this.Name = name;
            this.Description = description;
            this.Host = host;
            this.Username = username;
            this.Password = password;
            this.Duration = duration;
            this.Bandwidth = bandwidth;
            this.Simulate = simulate;
            this.Filename = filename;
        }

    }
}
