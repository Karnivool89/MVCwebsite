using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MvcMovie.Data;
using MvcMovie.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace MvcMovie.Controllers
{
    public class MoviesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MoviesController(ApplicationDbContext context)
        {
            _context = context;    
        }


        // GET: Movies
        public async Task<IActionResult> Index(string movieGenre, string searchString)
        {
            // Use LINQ to get list of genres.
            IQueryable<string> genreQuery = from m in _context.Movie
                                            orderby m.Genre
                                            select m.Genre;

            var movies = from m in _context.Movie
                         select m;

            if (!String.IsNullOrEmpty(searchString))
            {
                movies = movies.Where(s => s.Title.Contains(searchString));
            }

            if (!String.IsNullOrEmpty(movieGenre))
            {
                movies = movies.Where(x => x.Genre == movieGenre);
            }

            var movieGenreVM = new MovieGenreViewModel();
            movieGenreVM.genres = new SelectList(await genreQuery.Distinct().ToListAsync());
            movieGenreVM.movies = await movies.ToListAsync();

            return View(movieGenreVM);
        }

        // Right now, when a movie is created it then passes the inputted Movie Title to the imdbCall method,
        // which then calls the imdbSearch method to gather the resourceID from the title. imdbSearch then
        // passes the resourceID back to imdbCall which allows it to gather specific data from the JSON object
        // (in this case, duration). 

        private async Task<string> imdbCall(string searchTerm)
        {
            string imdbUrl = "http://imdb.wemakesites.net/api/{0}?api_key={1}";
            string resourceID = await imdbSearch(searchTerm);

            string _address = string.Format(imdbUrl, resourceID, "f90e14f3-9b18-47be-b213-75da0f686986");

            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(_address);
            response.EnsureSuccessStatusCode(); 
            string content = await response.Content.ReadAsStringAsync();
            dynamic movieiNFO = JObject.Parse(content);
            return movieiNFO.data.duration;
            
        }

        private async Task<string> imdbSearch(string searchTerm)
        {
            string imdbUrl = "http://imdb.wemakesites.net/api/search?q={0}&api_key={1}";

            string formattedSearchTerm = searchTerm.Replace(" ", "+");
            string _address = string.Format(imdbUrl, formattedSearchTerm, "f90e14f3-9b18-47be-b213-75da0f686986");

            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(_address);
            response.EnsureSuccessStatusCode();
            string content = await response.Content.ReadAsStringAsync();
            dynamic movieINFO = JObject.Parse(content);
            foreach (var result in movieINFO.data.results.titles)
            {
                if (result.title == searchTerm)
                    return result.id;
            }
            return "Movie not found";
        }

        // GET: Movies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie.SingleOrDefaultAsync(m => m.ID == id);
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // GET: Movies/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Movies/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Genre,Price,ReleaseDate,Title,Rating,Duration")] Movie movie)
        {
            if (ModelState.IsValid)
            {
                // 
                movie.Duration = await imdbCall(movie.Title);
                _context.Add(movie);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(movie);
        }

        // GET: Movies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie.SingleOrDefaultAsync(m => m.ID == id);
            if (movie == null)
            {
                return NotFound();
            }
            return View(movie);
        }

        // POST: Movies/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Genre,Price,ReleaseDate,Title,Rating, Duration")] Movie movie)
        {
            if (id != movie.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(movie);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MovieExists(movie.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index");
            }
            return View(movie);
        }

        // GET: Movies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie.SingleOrDefaultAsync(m => m.ID == id);
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // POST: Movies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movie = await _context.Movie.SingleOrDefaultAsync(m => m.ID == id);
            _context.Movie.Remove(movie);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private bool MovieExists(int id)
        {
            return _context.Movie.Any(e => e.ID == id);
        } 

    }
}
