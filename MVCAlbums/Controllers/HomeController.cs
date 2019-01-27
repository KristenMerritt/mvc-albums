using System;
using System.Collections.Generic;
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
        private static HttpClient Client;

        public HomeController()
        {
            if (Client == null)
            {
                // Define request
                Client = new HttpClient
                {
                    BaseAddress = new Uri("https://jsonplaceholder.typicode.com/")
                };
                Client.DefaultRequestHeaders.Clear();
                Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }           
        }

        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> Albums(int id)
        {
            // List to store data to send to Albums page
            List<AlbumsView> albumsViews = new List<AlbumsView>();

            // Sending request to get all albums  
            HttpResponseMessage albumsResponse = await Client.GetAsync("albums");

            // Checking to see if the response is successful or not
            if (albumsResponse.IsSuccessStatusCode)
            {
                // Deserializing the response recieved from web api and storing into Albums list  
                List<Album> albums = JsonConvert.DeserializeObject<List<Album>>(await albumsResponse.Content.ReadAsStringAsync());

                // Used for pagnation 
                var start = 1;
                var end = 10;

                if (id != 1)
                {
                    start = ((id * 10) + 1) - 10;
                    end = start + 10;
                }

                for(int i = start-1; i < end; i++)
                {
                    // Make sure we do not go over the albums count during pagnation
                    if (!(i > albums.Count()-1))
                    {
                        var album = albums[i];

                        // Create the temp albumview to later put into albumsView list
                        AlbumsView tempAlbumView = new AlbumsView
                        {
                            AlbumTitle = album.Title
                        };

                        // Sending request to get the user of the current album
                        HttpResponseMessage userResponse = await Client.GetAsync("users/" + album.UserId + "/");

                        if (userResponse.IsSuccessStatusCode)
                        {
                            // Put data recieved into temp albumview
                            User tempUserData = JsonConvert.DeserializeObject<User>(await userResponse.Content.ReadAsStringAsync());
                            tempAlbumView.AlbumUser = tempUserData;
                        }

                        // Sending request to get the first photo of the current album
                        HttpResponseMessage photoResponse = await Client.GetAsync("photos?albumId=" + album.Id);

                        if (photoResponse.IsSuccessStatusCode)
                        {
                            // Put data recieved into temp albumview
                            Photo tempPhoto = JsonConvert.DeserializeObject<List<Photo>>(await photoResponse.Content.ReadAsStringAsync()).First();         
                            tempAlbumView.AlbumThumbnail = tempPhoto.ThumbnailUrl;
                        }

                        // Add the tempalbum into list to send to View
                        albumsViews.Add(tempAlbumView);
                    }                
                }
            }

            // Send album data to the view
            return View(albumsViews);
        }

        public ActionResult User()
        {
            return View();
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