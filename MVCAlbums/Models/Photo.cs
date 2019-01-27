using System;

namespace MVCAlbums.Models
{
    public class Photo
    {
        private int AlbumId { get; set; }
        private int Id { get; set; }
        public String Title { get; set; }
        public String Url { get; set; }
        public String ThumbnailUrl { get; set; }
    }
}