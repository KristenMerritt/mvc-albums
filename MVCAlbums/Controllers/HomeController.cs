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
                BaseAddress = new Uri("http://jsonplaceholder.typicode.com/")
            };
            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        // INDEX VIEW
        public ActionResult Index()
        {
            return View();
        }

        // ALBUMS VIEW
        public async Task<ActionResult> Albums(int id = 1)
        {
            // Used to send data to Album page
            var albumsViews = new List<AlbumsView>();

            // Get initial list of albums
            var albums = GetMultiData<Album>("albums").Result ?? throw new ArgumentNullException("GetMultiData<Album>(\"albums\").Result");

            // Used for pagination 
            var start = 1;
            var end = 10;

            if (id != 1)
            {
                start = ((id * 10) + 1) - 10;
                end = start + 10;
            }

            // Iterate through albums for pagination
            for(var i = start-1; i < end; i++)
            {
                if (i > albums.Count() - 1) continue;
                var album = albums.ElementAt(i);

                // Create the temp albumView to later put into albumsView list
                var tempAlbumView = new AlbumsView
                {
                    AlbumTitle = album.Title,
                    AlbumId = album.Id
                };

                // Put user and first thumbnail data into temp albumView
                var tempUserData = GetSingleData<User>("users/" + album.UserId + "/").Result ?? throw new ArgumentNullException("GetSingleData<User>(\"users / \" + album.UserId + \" / \").Result");
                var tempPhoto = GetMultiData<Photo>("photos?albumId=" + album.Id).Result.First() ?? throw new ArgumentNullException("photos?albumId=\" + album.Id).Result.First()");

                tempAlbumView.AlbumUser = (User)tempUserData;
                tempAlbumView.AlbumThumbnail = tempPhoto.ThumbnailUrl;
                albumsViews.Add(tempAlbumView);
            }

            // Send album data to the view
            return View(albumsViews);
        }



        // USER VIEW
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

        public async Task<ActionResult> Search(string searchParam)
        {
            var user = (User) GetSingleData<User>("users?name=" + searchParam).Result;

            if (!user.Equals(null))
            {
                Response.Redirect("~/Home/User/" + user.Id);
                return null;
            }

            var albumView = new AlbumsView();

            var album = (Album) GetSingleData<Album>("albums?title=" + searchParam).Result;

            if (album.Equals(null)) return null;

            albumView.AlbumId = album.Id;
            albumView.AlbumTitle = album.Title;

            var userData = (User) GetSingleData<User>("users/" + album.UserId + "/").Result ??
                           throw new ArgumentNullException(
                               "GetSingleData<User>(\"users / \" + album.UserId + \" / \").Result");
            var thumbnail = (Photo) GetMultiData<Photo>("photos?albumId=" + album.Id).Result.First() ??
                            throw new ArgumentNullException("photos?albumId=\" + album.Id).Result.First()");

            albumView.AlbumUser = userData;
            albumView.AlbumThumbnail = thumbnail.ThumbnailUrl;

            return View(albumView);

        }

        // HELPER FUNCTION GET MULTI DATA
        private async Task<IEnumerable<T>> GetMultiData<T>(string url)
        {
            Debug.WriteLine("In call method");

            // Sending request
            try
            {
                var res = await _client.GetAsync(url).ConfigureAwait(false);
                Debug.WriteLine("Got Async Response");
                // Checking the response is successful or not which is sent using HttpClient  
                if (res.IsSuccessStatusCode)
                {
                    Debug.WriteLine("Success Code Received");
                    // Storing the response details received from web api   
                    var response = await res.Content.ReadAsStringAsync();

                    // Deserializing the response received from web api and storing into the Employee list
                    return JsonConvert.DeserializeObject<IEnumerable<T>>(response);
                }
                Debug.WriteLine("No Success Code Received");
                return null;
            }
            catch (Exception e)
            {
                Debug.WriteLine("broke"); 
            }

            return null;
        }

        // HELPER FUNCTION GET SINGLE DATA
        private async Task<Object> GetSingleData<T>(string url)
        {
            Debug.WriteLine("In call method");

            // Sending request
            var res = await _client.GetAsync(url).ConfigureAwait(false);
            Debug.WriteLine("Got Async Response");
            // Checking the response is successful or not which is sent using HttpClient  
            if (res.IsSuccessStatusCode)
            {
                Debug.WriteLine("Success Code Received");
                // Storing the response details received from web api   
                var response = await res.Content.ReadAsStringAsync();

                // Deserializing the response received from web api and storing into the Employee list
                return JsonConvert.DeserializeObject<T>(response);
            }
            Debug.WriteLine("No Success Code Received");
            return null;
        }
    }
}