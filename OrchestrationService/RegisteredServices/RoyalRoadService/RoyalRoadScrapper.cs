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
using ParsingService.Models;
using OrchestrationService.Services;

namespace OrchestrationService.RegisteredServices
{
    public class RoyalRoadScrapper : WebParser, ISearchService 
    {
        private string baseUrl = "https://www.royalroad.com";
        private string url = "https://www.royalroad.com/fictions/search";

        public async Task<List<BookModel>> GetFromService()
        {
            QueryModel queryModel = GetQueryModel();

            var result = await ParseWholeDomain(url, pagingQuery: queryModel.pagingParameters, 
                pageParseNumber: queryModel.PagesToProcess, pageIncrement: queryModel.pagingIncrement);
        
            if (result != null)
            {
                return result;
            }
            else
            {
                return null;
            }
        }

        public QueryModel GetQueryModel()
        {
            QueryModel queryModel = new QueryModel();

            queryModel.url = "https://www.royalroad.com/fictions/search";
            queryModel.pagingParameters = new KeyValuePair<string, int>("page", 1);
            queryModel.pagingIncrement = 1;
            queryModel.PagesToProcess = 20;

            return queryModel;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="searchUrl"> url for search</param>
        /// <param name="baseUrl"> base url of site</param>
        /// <param name="pagingQuery"> paging format</param>
        /// <param name="limitPage"></param>
        /// <param name="pageIncrement"></param>
        /// <returns></returns>
        /*public async Task<IList<BookModel>> ParseWholeSite(string searchUrl, string baseUrl, KeyValuePair<string, int>? pagingQuery = null, int pageParseNumber = 1, int pageIncrement = 1)
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
                *//*foreach(var link in links)
                {
                    var result = await ParsePage(link, baseUrl);
                    if (result != null)
                    {
                        books.AddRange(result);
                    }
                };*//*
            }
            else
            {
                return await ParsePage(searchUrl, baseUrl);
            }

            return books;
        }
*/


        public override async Task<List<BookModel>> ParsePage(string url)
        {
            List<BookModel> books = new List<BookModel>();

            var html = await CallUrl(url);

            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var bookContainers = htmlDoc.DocumentNode.SelectNodes("//div[(contains(@class, 'fiction-list-item'))]");

            foreach ( var bookContainer in bookContainers )
            {
                string title = WebUtility.HtmlDecode(bookContainer.SelectSingleNode(".//h2[(contains(@class, 'fiction-title'))]/a").InnerText);
                //var title = fictionTitleRow.InnerText;
                var link = bookContainer.SelectSingleNode(".//figure/a").Attributes["href"].Value;
                link = baseUrl + link;

                if (title != null && title.Contains("sinner")) 
                { 

                }

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


                var description = WebUtility.HtmlDecode(stats.SelectSingleNode("./div[(contains(@id, \'description\'))]").InnerText.Trim());

                int ratingCount = -1;

                string author = null;

                if (link != null)
                {
                    var detailsHtml = await CallUrl(link);

                    HtmlDocument detailsHtmlDoc = new HtmlDocument();
                    detailsHtmlDoc.LoadHtml(detailsHtml);

                    author = WebUtility.HtmlDecode(detailsHtmlDoc.DocumentNode.SelectSingleNode(".//div[(contains(@class, 'fic-header'))]//span[./a]/a").InnerText);
                    int.TryParse(detailsHtmlDoc.DocumentNode.SelectSingleNode("//li[.='Ratings :']/following-sibling::li").InnerText, NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out ratingCount);
                    
                }


                BookModel book = new BookModel()
                {
                    title = title,
                    thumbnailLink = thumbnailLink,
                    origin = new List<LinkWithPrice>() { new LinkWithPrice() { link = link, portalDomain = "RoyalRoad"} },
                    averagerating = rating >= 0 ? rating : null,
                    pageCount = pages >= 0 ? pages : null,
                    publishedDate = publishTimeUnixtime != 0 ? DateTimeOffset.FromUnixTimeSeconds(publishTimeUnixtime).DateTime : null,
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
            return books;
        }

    }
}
