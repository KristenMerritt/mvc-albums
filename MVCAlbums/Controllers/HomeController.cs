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
        /// 
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
        /// 
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
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
        /// 
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <param name="searchParam"></param>
        /// <param name="isPartial"></param>
        /// <returns></returns>
        public async Task<ActionResult> Albums(int skip = 0, int take = 10, string searchParam = null, bool isPartial = false)
        {
            // Get initial list of albums
            await LoadAlbumsAndUsers();

            // Used to send data to Album page
            var albumsViews = new List<AlbumsView>();

            var albumList = !string.IsNullOrWhiteSpace(searchParam)
                ? _allAlbums.Where(x =>
                    x.Title.ToLower().Contains(searchParam.ToLower()) ||
                    x.User.Name.ToLower().Contains(searchParam.ToLower())).ToList()
                : _allAlbums;

            var totalItems = albumList.Count();

            // Used for pagination 
            albumList = albumList.Skip(skip).Take(take).ToList();

            // Iterate through albums for pagination
            foreach(var album in albumList)
            {
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
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ActionResult> User(int id = 1)
        {
            var userView = new UserView();
            
            // Get the user based off of user id
            var user = (User)(await GetSingleData<User>("users/" + id + "/"));
            userView.User = user;

            // Get posts data of the user
            var posts = (await GetMultiData<Post>("posts?userId=" + id)).ToList();

            foreach (var post in posts)
            {
                // Get comments of the post
                var postComments = (await GetMultiData<Comment>("comments?postId=" + post.Id)).ToList();
                post.Comments = postComments;
            }

            userView.UserPosts = posts;

            return View(userView);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <returns></returns>
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
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <returns></returns>
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