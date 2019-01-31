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
using MVCAlbums.Utilities;

namespace MVCAlbums.Controllers
{
    public class HomeController : Controller
    {
        private static HttpClient _client;
        private static List<Album> _allAlbums;
        private static IEnumerable<User> _allUsers;

        /// <summary>
        /// Instantiates the http client used for http requests
        /// </summary>
        public HomeController()
        {
            if (_client != null) return;

            // Define client
            _client = new HttpClient
            {
                BaseAddress = new Uri("http://jsonplaceholder.typicode.com/")
            };
            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        /// <summary>
        /// Returns to the index page
        /// </summary>
        /// <returns> Main View </returns>
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Front loads all of the albums and users for use
        /// during pagination / searching as the api
        /// does not provide functionality for this.
        /// </summary>
        /// <returns>All album and user data</returns>
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
        
        /// <summary>
        /// Main functionality for Albums page,
        /// accounts for pagination and search
        /// </summary>
        /// <param name="skip">Number of items to skip for pagination</param>
        /// <param name="take">Number of items to take in for pagination</param>
        /// <param name="searchParam">Used when filtering search results</param>
        /// <param name="isPartial">If true, return the partial page</param>
        /// <returns>Album paged view</returns>
        public async Task<ActionResult> Albums(int skip = 0, int take = 10, string searchParam = null, bool isPartial = false)
        {
            // Get initial list of albums if we have not already
            await LoadAlbumsAndUsers();

            var albumsViews = new List<AlbumsView>();

            // Search functionality
            var albumList = !string.IsNullOrWhiteSpace(searchParam)
                ? _allAlbums.Where(x =>
                    x.Title.ToLower().Contains(searchParam.ToLower()) ||
                    x.User.Name.ToLower().Contains(searchParam.ToLower())).ToList()
                : _allAlbums;

            var totalItems = albumList.Count();

            // Used for pagination 
            albumList = albumList.Skip(skip).Take(take).ToList();

            foreach(var album in albumList)
            {
                // Create the temp albumView to later put into albumsView list
                var tempAlbumView = new AlbumsView
                {
                    AlbumTitle = album.Title,
                    AlbumId = album.Id,
                    SearchParam = searchParam,
                    AlbumUser = album.User
                };

                // Getting the first photo of the album
                var tempPhoto = (await GetMultiData<Photo>("photos?albumId=" + album.Id)).First() 
                                ?? throw new ArgumentNullException("photos?albumId=\" + album.Id).Result.First()");

                tempAlbumView.AlbumThumbnail = tempPhoto.ThumbnailUrl;
                albumsViews.Add(tempAlbumView);
            }

            // Create PagedResult for pagination 
            var result = new PagedResult<AlbumsView>
            {
                Items = albumsViews,
                Skipped = skip,
                Taken = take,
                TotalItemCount = totalItems
            };

            if (isPartial)
            {
                return PartialView("_AlbumListPartial", result);
            }
            return View(result);
        }

        /// <summary>
        /// Main functionality for User page
        /// </summary>
        /// <param name="id">ID of the user</param>
        /// <returns>User view</returns>
        public async Task<ActionResult> User(int id = 1)
        {
            var userView = new UserView
            {
                User = (User) (await GetSingleData<User>("users/" + id + "/"))
            };

            // Get the post data of the user
            var posts = (await GetMultiData<Post>("posts?userId=" + id)).ToList();

            foreach (var post in posts)
            {
                // Get comments of the post
                post.Comments = (await GetMultiData<Comment>("comments?postId=" + post.Id)).ToList();
            }

            userView.UserPosts = posts;

            return View(userView);
        }

        /// <summary>
        /// Retrieves data from the web api
        /// </summary>
        /// <typeparam name="T">Generic</typeparam>
        /// <param name="url">Specific endpoint to call for data</param>
        /// <returns>Data collected from endpoint in Task</returns>
        private async Task<IEnumerable<T>> GetMultiData<T>(string url)
        {
            try
            {
                var res = await _client.GetAsync(url).ConfigureAwait(false);

                if (!res.IsSuccessStatusCode) return null;

                // Storing the response details received from web api   
                var response = await res.Content.ReadAsStringAsync();

                // Deserializing the response received from web api & returning
                return JsonConvert.DeserializeObject<IEnumerable<T>>(response);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return null;
            }
        }

        /// <summary>
        /// Retrieves datum from the web api
        /// </summary>
        /// <typeparam name="T">Generic</typeparam>
        /// <param name="url">Specific endpoint to call for data</param>
        /// <returns>Data collected from endpoint in Task</returns>
        private async Task<Object> GetSingleData<T>(string url)
        {
            try
            {
                var res = await _client.GetAsync(url).ConfigureAwait(false);

                if (!res.IsSuccessStatusCode) return null;

                // Storing the response details received from web api   
                var response = await res.Content.ReadAsStringAsync();

                // Deserializing the response received from web api & returning
                return JsonConvert.DeserializeObject<T>(response);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return null;
            }           
        }
    }
}