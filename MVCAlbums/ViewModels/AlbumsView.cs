﻿using System;
using System.ComponentModel;
using MVCAlbums.Models;

namespace MVCAlbums.ViewModels
{
    public class AlbumsView
    {
        [DisplayName("Thumbnail")]
        public String AlbumThumbnail { get; set; }
        [DisplayName("Album Title")]
        public String AlbumTitle { get; set; }
        public User AlbumUser { get; set; }
    }
}