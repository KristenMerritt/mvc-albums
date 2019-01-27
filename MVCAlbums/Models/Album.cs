using System;
using System.Collections.Generic;

namespace MVCAlbums.Models
{
    public class Album
    {
        public int UserId { get; set; }
        public int Id { get; set; }
        public String Title { get; set; }

        public ICollection<Photo> Photos { get; set; }
        public User User { get; set; }
    }
}