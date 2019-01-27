using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Mvc;
using MVCAlbums.Models;
using MVCAlbums.ViewModels;
using Newtonsoft.Json;

namespace MVCAlbums.Controllers
{
    public class HomeController : Controller
    {
        private static HttpClient _client;

        public HomeController()
        {
            if (_client != null) return;

            // Define request
            _client = new HttpClient
            {
                BaseAddress = new Uri("https://jsonplaceholder.typicode.com/")
            };
            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> Albums(int id = 1)
        {
            // List to store data to send to Albums page
            var albumsViews = new List<AlbumsView>();

            // Sending request to get all albums  
            var albumsResponse = await _client.GetAsync("albums");

            // Checking to see if the response is successful or not
            if (!albumsResponse.IsSuccessStatusCode) return View(albumsViews);

            // Deserializing the response received from web api and storing into Albums list  
            var albums = JsonConvert.DeserializeObject<List<Album>>(await albumsResponse.Content.ReadAsStringAsync());

            // Used for pagination 
            var start = 1;
            var end = 10;

            if (id != 1)
            {
                start = ((id * 10) + 1) - 10;
                end = start + 10;
            }

            for(var i = start-1; i < end; i++)
            {
                // Make sure we do not go over the albums count during pagination
                if (i > albums.Count() - 1) continue;

                var album = albums[i];

                // Create the temp albumView to later put into albumsView list
                var tempAlbumView = new AlbumsView
                {
                    AlbumTitle = album.Title
                };

                // Sending request to get the user of the current album
                var userResponse = await _client.GetAsync("users/" + album.UserId + "/");

                if (userResponse.IsSuccessStatusCode)
                {
                    // Put data received into temp albumView
                    var tempUserData = JsonConvert.DeserializeObject<User>(await userResponse.Content.ReadAsStringAsync());
                    tempAlbumView.AlbumUser = tempUserData;
                }

                // Sending request to get the first photo of the current album
                var photoResponse = await _client.GetAsync("photos?albumId=" + album.Id);

                if (photoResponse.IsSuccessStatusCode)
                {
                    // Put data received into temp albumView
                    var tempPhoto = JsonConvert.DeserializeObject<List<Photo>>(await photoResponse.Content.ReadAsStringAsync()).First();         
                    tempAlbumView.AlbumThumbnail = tempPhoto.ThumbnailUrl;
                }

                // Add the tempAlbumView into list to send to View
                albumsViews.Add(tempAlbumView);
            }

            // Send album data to the view
            return View(albumsViews);
        }

        public async Task<ActionResult> User(int id = 1)
        {
            Debug.WriteLine("Getting User: " + id );
            var userView = new UserView();
            
            // Sending request to get specified user  
            var userResponse = await _client.GetAsync("users/" + id + "/");

            // Checking to see if the response is successful or not
            if (!userResponse.IsSuccessStatusCode) return View(userView);

            // Deserializing the response received from web api and storing into userView 
            var user = JsonConvert.DeserializeObject<User>(await userResponse.Content.ReadAsStringAsync());
            userView.User = user;

            // Sending request to get all posts for the user 
            var postResponse = await _client.GetAsync("posts?userId=" + id );

            // Checking to see if the response is successful or not
            if (!postResponse.IsSuccessStatusCode) return View(userView);

            // Deserializing the response received from web api and storing into userView
            var posts = JsonConvert.DeserializeObject<List<Post>>(await postResponse.Content.ReadAsStringAsync());
            

            foreach (var post in posts)
            {
                // Sending request to get specified user  
                var commentResponse = await _client.GetAsync("comments?postId=" + post.Id);

                // Checking to see if the response is successful or not
                if (!commentResponse.IsSuccessStatusCode) continue;

                // Deserializing the response received from web api and storing into userView 
                var postComments = JsonConvert.DeserializeObject<List<Comment>>(await commentResponse.Content.ReadAsStringAsync());
                post.Comments = postComments;
            }

            userView.UserPosts = posts;

            return View(userView);
        }

        //private async Task<IEnumerable<T>> GetData<T>(string url)
        //{
        //    System.Diagnostics.Debug.WriteLine("In call method");
        //    Client.BaseAddress = new Uri(BaseUrl);
        //    Client.DefaultRequestHeaders.Clear();

        //    //Define request data format  
        //    Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        //    System.Diagnostics.Debug.WriteLine("Awaiting Response");
        //    //Sending request to find web api REST service resource GetAllEmployees using HttpClient  
        //    HttpResponseMessage Res = await Client.GetAsync(url);
        //    System.Diagnostics.Debug.WriteLine("Got Async Response");
        //    //Checking the response is successful or not which is sent using HttpClient  
        //    if (Res.IsSuccessStatusCode)
        //    {
        //        System.Diagnostics.Debug.WriteLine("Success Code Recieved");
        //        //Storing the response details recieved from web api   
        //        var Response = await Res.Content.ReadAsStringAsync();

        //        //Deserializing the response recieved from web api and storing into the Employee list

        //        return JsonConvert.DeserializeObject<IEnumerable<T>>(Response);
        //    }
        //    else
        //    {
        //        System.Diagnostics.Debug.WriteLine("No Success Code Recieved");
        //        return null;
        //    }
        //}
    }
}