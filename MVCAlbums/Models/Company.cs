using System;
using System.ComponentModel;

namespace MVCAlbums.Models
{
    public class Company
    {
        public String Name { get; set; }
        [DisplayName("Catch Phrase")]
        public String CatchPhrase { get; set; }
        public String Bs { get; set; }
    }
}