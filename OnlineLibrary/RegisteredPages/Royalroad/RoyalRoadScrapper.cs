using OnlineLibrary.ApiParsers;
using ParsingService.Models.Entities;
using HtmlAgilityPack;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Net;
using System.Text;
using System.IO;
using System;
using System.Globalization;
using Microsoft.AspNetCore.WebUtilities;
using OnlineLibrary.Models;

namespace ParsingService.RegisteredPages.Royalroad
{
    public class RoyalRoadScrapper : IPageScrapper
    {
        /*private string baseUrl = "https://www.royalroad.com";
        private string url = "https://www.royalroad.com/fictions/search";
        private int startPage = 1;
        private int endPage = 100;*/

        /// <summary>
        /// 
        /// </summary>
        /// <param name="searchUrl"> url for search</param>
        /// <param name="baseUrl"> base url of site</param>
        /// <param name="pagingQuery"> paging format</param>
        /// <param name="limitPage"></param>
        /// <param name="pageIncrement"></param>
        /// <returns></returns>
        public async Task<IList<BookModel>> ParseWholeSite(string searchUrl, string baseUrl, KeyValuePair<string, int>? pagingQuery = null, int pageParseNumber = 1, int pageIncrement = 1)
        {
            List<BookModel> books = new List<BookModel>();

            if (pagingQuery != null)
            {
                List<string> links = new List<string>();

                for (int i = 0; i < pageParseNumber; i++)
                {
                    var urlWithPaging = QueryHelpers.AddQueryString(searchUrl, pagingQuery.Value.Key, pagingQuery.Value.Value.ToString());
                    pagingQuery = new KeyValuePair<string, int>(pagingQuery.Value.Key, pagingQuery.Value.Value + pageIncrement);

                    links.Add(urlWithPaging);           
                }

                await Parallel.ForEachAsync(links, async (link, CancellationToken) =>
                {
                    var result = await ParsePage(link, baseUrl);
                    if (result != null)
                    {
                        lock (books)
                        {
                            books.AddRange(result);
                        }
                    }
                });
                /*foreach(var link in links)
                {
                    var result = await ParsePage(link, baseUrl);
                    if (result != null)
                    {
                        books.AddRange(result);
                    }
                };*/
            }
            else
            {
                return await ParsePage(searchUrl, baseUrl);
            }

            return books;
        }
        public async Task<IList<BookModel>> ParsePage(string url, string baseUrl)
        {
            IList<BookModel> books = new List<BookModel>();

            var html = await CallUrl(url);

            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var bookContainers = htmlDoc.DocumentNode.SelectNodes("//div[(contains(@class, 'fiction-list-item'))]");

            foreach ( var bookContainer in bookContainers )
            {
                var title = bookContainer.SelectSingleNode(".//h2[(contains(@class, 'fiction-title'))]/a").InnerText;
                //var title = fictionTitleRow.InnerText;
                var link = bookContainer.SelectSingleNode(".//figure/a").Attributes["href"].Value;
                link = baseUrl + link;

                var thumbnailLink = bookContainer.SelectSingleNode(".//figure/a/img").Attributes["src"].Value;
                var stats = bookContainer.SelectSingleNode(".//div[(contains(@class, 'stats'))]");
                
                float rating = -1;
                var ratingString = stats.SelectSingleNode(".//div[(contains(@aria-label, 'Rating'))]/span").Attributes["title"].Value;
                float.TryParse(ratingString, out rating);

                int pages = -1;
                var pagesString = stats.SelectSingleNode("./div[.//i[(contains(@class, 'fa-book'))]]/span").InnerText.Split(' ')[0];
                int.TryParse(pagesString, NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out pages);

                long publishTimeUnixtime;
                var publishTimeUnixtimeString = stats.SelectSingleNode("./div[.//time]/time").Attributes["unixtime"].Value; ;
                long.TryParse(publishTimeUnixtimeString, out publishTimeUnixtime);


                var description = stats.SelectSingleNode("./div[(contains(@id, \'description\'))]").InnerText.Trim();

                int ratingCount = -1;

                string author = null;

                if (link != null)
                {
                    var detailsHtml = await CallUrl(link);

                    HtmlDocument detailsHtmlDoc = new HtmlDocument();
                    detailsHtmlDoc.LoadHtml(detailsHtml);

                    author = detailsHtmlDoc.DocumentNode.SelectSingleNode(".//div[(contains(@class, 'fic-header'))]//span[./a]/a").InnerText;
                    int.TryParse(detailsHtmlDoc.DocumentNode.SelectSingleNode("//li[.='Ratings :']/following-sibling::li").InnerText, NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out ratingCount);
                    
                }


                BookModel book = new BookModel()
                {
                    title = title,
                    thumbnailLink = thumbnailLink,
                    origin = new List<LinkWithPrice>() { new LinkWithPrice() { link = link, portalDomain = "RoyalRoad"} },
                    averagerating = rating >= 0 ? rating : null,
                    pageCount = pages >= 0 ? pages : null,
                    publishedDate = publishTimeUnixtime != 0 ? new DateTime(publishTimeUnixtime) : null,
                    description = description,

                    timeRetrieved = DateTime.Now,
                };

                if (author != null)
                {
                    book.authors = new List<Author>() { new Author() { name = author } };
                }
                if (ratingCount >= 0)
                {
                    book.ratingCount = ratingCount;
                }
                else
                {

                }

                books.Add(book);
            }

            /*List<string> wikiLink = new List<string>();

            foreach (var link in programmerLinks)
            {
                if (link.FirstChild.Attributes.Count > 0) wikiLink.Add("https://en.wikipedia.org/" + link.FirstChild.Attributes[0].Value);
            }

            return wikiLink;*/

            return books;
        }

        private async Task<string> CallUrl(string fullUrl)
        {
            HttpClient client = new HttpClient();
            var response = await client.GetStringAsync(fullUrl);
            return response;
        }
    }
}
