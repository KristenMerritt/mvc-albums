using System;

namespace MVCAlbums.Models
{
    public class Todo
    {
        private int UserId { get; set; }
        private int Id { get; set; }
        public String Title { get; set; }
        public String Completed { get; set; }
    }
}