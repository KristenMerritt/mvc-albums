using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MVCAlbums.Models;

namespace MVCAlbums.ViewModels
{
    public class UserView
    {
        public User User  { get; set; }
        public ICollection<Post> UserPosts { get; set; }
    }
}