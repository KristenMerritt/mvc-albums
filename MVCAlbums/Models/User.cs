using System;

namespace MVCAlbums.Models
{
    public class User
    {
        public int Id { get; set; }
        public String Name { get; set; }
        public String Username { get; set; }
        public String Email { get; set; }
        public Address Address { get; set; }
        public String Phone { get; set; }
        public String Website { get; set; }
        public Company Company { get; set; }
    }
}