using OrchestrationService.Services;
using ParsingService.Models;
using ParsingService.Models.Entities;
using System;
using System.IO;

namespace OrchestrationService.RegisteredServices.GoogleBooksAPIService
{
    public class GoogleBooksApiParser : ApiParser, ISearchService
    {
        static HttpClient client = new HttpClient();
        public async Task<List<BookModel>> GetFromService()
        {
            QueryModel queryModel = GetQueryModel();

            var result = await ParseAllCalls(client, queryModel.url, queryModel.pagingParameters,
                queryModel.PagesToProcess, queryModel.pagingIncrement, queryModel.staticQueryParameters,
                queryModel.changingValueParameters);
            
            if (result != null)
            {
                return result.ToList();
            }
            else
            {
                return null;
            }
        }

        enum SubjectEnum
        {
            fiction,
            historical,
        }
        public QueryModel GetQueryModel()
        {
            QueryModel queryModel = new QueryModel();

            queryModel.url = "https://www.googleapis.com/books/v1/volumes";
            queryModel.staticQueryParameters = new Dictionary<string, string>()
            {
                { "maxResults", "20"},
            };
            queryModel.changingValueParameters = new List<(string, string)>();
            foreach (var subject in Enum.GetNames(typeof(SubjectEnum)))
            {
                queryModel.changingValueParameters.Add(("q", "subject:" + subject));
            }
            queryModel.pagingParameters = new KeyValuePair<string, int>("startIndex", 0);
            queryModel.pagingIncrement = 20;
            queryModel.PagesToProcess = 100;

            return queryModel;
        }

        public override IList<BookModel> ParseResponse(dynamic response)
        {
            IList<BookModel> result = new List<BookModel>();
            var count = response.totalItems;
            if (!response.ContainsKey("items"))
            {
                return result;
            }

            var items = response.items;
            foreach (var item in items)
            {
                BookModel bookItem = new BookModel()
                {
                    title = item.volumeInfo.title,
                    subtitle = item.volumeInfo.subtitle,
                    publisher = item.volumeInfo.publisher,
                    description = item.volumeInfo.description,
                    pageCount = item.volumeInfo.pageCount,
                    averagerating = item.volumeInfo.averageRating,
                    ratingCount = item.volumeInfo.ratingsCount,
                    maturityRating = item.volumeInfo.maturityRating,
                    language = item.volumeInfo.language,
                    timeRetrieved = DateTime.Now,
                };
                try
                {
                    bookItem.publishedDate = item.volumeInfo.publishedDate;
                }
                catch (Exception ex)
                {
                    try
                    {
                        bookItem.publishedDate = new DateTime((int)item.volumeInfo.publishedDate, 1, 1);
                    }
                    catch (Exception)
                    {
                        bookItem.publishedDate = null;
                    }

                }

                if (item.volumeInfo.ContainsKey("imageLinks"))
                {
                    bookItem.thumbnailLink = item.volumeInfo.imageLinks.thumbnail;
                }
                if (item.volumeInfo.ContainsKey("authors"))
                {
                    var authorsList = item.volumeInfo.authors.ToObject<List<string>>();
                    foreach (var author in authorsList)
                    {
                        bookItem.authors?.Add(new Author() { name = author });
                    }
                }
                if (item.volumeInfo.ContainsKey("industryIdentifiers"))
                {
                    bookItem.industryIdentifiers = new List<Identifier>();
                    foreach (var id in item.volumeInfo.industryIdentifiers)
                    {
                        bookItem.industryIdentifiers.Add(new Identifier()
                        {
                            identifierCode = id.identifier,
                            type = id.type,
                        });
                    }
                }
                if (item.volumeInfo.ContainsKey("categories"))
                {
                    var categoriesList = item.volumeInfo.categories.ToObject<List<string>>();
                    foreach (var category in categoriesList)
                    {
                        bookItem.categories?.Add(new Category() { CategoryName = category });
                    }
                }

                var priceInfo = new LinkWithPrice()
                {
                    link = item.volumeInfo.infoLink,
                    portalDomain = "GoogleBooksAPI",
                };
                if (item.saleInfo.ContainsKey("listPrice"))
                {
                    priceInfo.listPrice = item.saleInfo.listPrice.amount;
                    priceInfo.currencyListPrice = item.saleInfo.listPrice.currencyCode;
                }
                if (item.saleInfo.ContainsKey("retailPrice"))
                {
                    priceInfo.retailPrice = item.saleInfo.retailPrice.amount;
                    priceInfo.currencyRetailPrice = item.saleInfo.retailPrice.currencyCode;
                }
                bookItem.origin = new List<LinkWithPrice> { priceInfo };

                result.Add(bookItem);
            }
            return result;
        }
    }
}
