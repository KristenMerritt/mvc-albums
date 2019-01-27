using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MVCAlbums.Models
{
    public class Post
    {
        public int UserId { get; set; }
        public int Id { get; set; }
        [DisplayName("Post Title")]
        public String Title { get; set; }
        [DisplayName("Post Body")]
        public String Body { get; set; }
        [DisplayName("Post Comments")]
        public ICollection<Comment> Comments { get; set; }
    }
}