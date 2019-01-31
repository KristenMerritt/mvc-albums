using System;
using System.ComponentModel;
using MVCAlbums.Models;

namespace MVCAlbums.ViewModels
{
    public class AlbumsView
    {
        public int AlbumId { get; set; }
        [DisplayName("Thumbnail")]
        public String AlbumThumbnail { get; set; }
        [DisplayName("Album Title")]
        public String AlbumTitle { get; set; }
        public User AlbumUser { get; set; }
        public string SearchParam { get; set; }
    }
}