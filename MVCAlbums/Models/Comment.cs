using System;

namespace MVCAlbums.Models
{
    public class Comment
    {
        public int PostId { get; set; }
        public int Id { get; set; }
        public String Name { get; set; }
        public String Email { get; set; }
        public String Body { get; set; }
    }
}