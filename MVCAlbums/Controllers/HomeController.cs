using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
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
        private static List<Album> _allAlbums;
        private static IEnumerable<User> _allUsers;

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

        private async Task LoadAlbumsAndUsers()
        {
            if (_allAlbums != null && _allAlbums.Any() && _allUsers != null && _allUsers.Any()) return;

            _allAlbums = (await GetMultiData<Album>("albums")).ToList();
            _allUsers = (await GetMultiData<User>("users")).ToList();
            
            foreach (var album in _allAlbums)
            {
                album.User = _allUsers.FirstOrDefault(x => x.Id == album.UserId);
            }
        }
        
        // ALBUMS VIEW
        public async Task<ActionResult> Albums(int id = 1, string searchParam = null, bool isPartial = false)
        {
            var pageNumber = id;

            // Get initial list of albums
            await LoadAlbumsAndUsers();

            // Used to send data to Album page
            var albumsViews = new List<AlbumsView>();

            var albumList = !string.IsNullOrWhiteSpace(searchParam)
                ? _allAlbums.Where(x =>
                    x.Title.ToLower().Contains(searchParam.ToLower()) ||
                    x.User.Name.ToLower().Contains(searchParam.ToLower())).ToList()
                : _allAlbums;

            // Used for pagination 
            var start = 1;
            var end = 10;

            if (pageNumber != 1)
            {
                start = ((pageNumber * 10) + 1) - 10;
                end = start + 10;
            }

            // Iterate through albums for pagination
            for (var i = start - 1; i < end; i++)
            {
                if (i > albumList.Count() - 1) continue;
                var album = albumList.ElementAt(i);

                // Create the temp albumView to later put into albumsView list
                var tempAlbumView = new AlbumsView
                {
                    AlbumTitle = album.Title,
                    AlbumId = album.Id,
                    SearchParam = searchParam
                };

                // Put user and first thumbnail data into temp albumView
                
                var tempUserData = album.User
                                   ?? throw new ArgumentNullException("GetSingleData<User>(\"users / \" + album.UserId + \" / \").Result");
                var tempPhoto = (await GetMultiData<Photo>("photos?albumId=" + album.Id)).First() 
                                ?? throw new ArgumentNullException("photos?albumId=\" + album.Id).Result.First()");

                tempAlbumView.AlbumUser = (User)tempUserData;
                tempAlbumView.AlbumThumbnail = tempPhoto.ThumbnailUrl;
                albumsViews.Add(tempAlbumView);
            }

            if (isPartial)
            {
                return PartialView("_AlbumListPartial", albumsViews);
            }
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