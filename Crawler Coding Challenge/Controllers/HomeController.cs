using Crawler_Coding_Challenge.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using HtmlAgilityPack;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Net;
using System.Text;
using System.IO;
using AngleSharp;
using AngleSharp.Dom;
using System.Collections.Generic;
using Crawler_Coding_Challenge.ViewModels;

namespace Crawler_Coding_Challenge.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        [HttpGet]
        //[Route("Home/Index/{maxCount:int}/{WordToExclude:string}")]
        public async Task<IActionResult> Index([FromQuery(Name = "maxCount")] int maxCount = 10, [FromQuery(Name = "WordToExclude")] string WordToExclude = "")
        {
            // Parm referactoring
            var rejectedWords = WordToExclude?.Split(',').Select(x => x).ToList();

            var Response = new List<HomeVM>();

            var Config = Configuration.Default.WithDefaultLoader();
            var Address = "https://en.wikipedia.org/wiki/Microsoft";
            var Context = BrowsingContext.New(Config);
            var Document = await Context.OpenAsync(Address);

            var contentSelector = "#content";
            var content = Document.QuerySelector(contentSelector);
            var contentWords = content?.TextContent;

            if (!string.IsNullOrEmpty(contentWords))
            {
                var words = contentWords.Split(' ').ToList();

                var result = words.GroupBy(x => x)
                             .Where(g => g.Count() > 1)
                             .Select(y => new HomeVM { Words = y.Key, WordsCount = y.Count() })
                             .ToList();

                Response = result.OrderByDescending(x => x.WordsCount).Take(maxCount).ToList();
                if (rejectedWords != null && rejectedWords.Any())
                {
                    var rejectList = result.Where(i => rejectedWords.Contains(i.Words));

                    var filteredList = Response.Except(rejectList);
                    Response = filteredList.Take(maxCount).ToList();
                }
            }

            return View(Response);
        }
    }
}