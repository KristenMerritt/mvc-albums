using System;

namespace MVCAlbums.Models
{
    public class Post
    {
        private int UserId { get; set; }
        private int Id { get; set; }
        public String Title { get; set; }
        public String Body { get; set; }
    }
}