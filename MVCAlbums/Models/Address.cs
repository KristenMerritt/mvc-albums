using System;

namespace MVCAlbums.Models
{
    public class Address
    {
        public String Street { get; set; }
        public String Suite { get; set; }
        public String City { get; set; }
        public String Zipcode { get; set; }
        public Geo Geo { get; set; }
    }
}