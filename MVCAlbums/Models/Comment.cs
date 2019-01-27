using System;

namespace MVCAlbums.Models
{
    public class Comment
    {
        private int PostId { get; set; }
        private int Id { get; set; }
        public String Name { get; set; }
        public String Email { get; set; }
        public String Body { get; set; }
    }
}