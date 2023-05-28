using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using OnlineLibrary.ApiParsers;
using OnlineLibrary.Models;
using ParsingService.Models.Entities;
using System;
using System.Data.Common;
using System.Reflection.Metadata;

namespace ParsingService.RegisteredAPIs.GoogleBooksAPI
{
    public class GoogleApiModel : ApiModel
    {
        public GoogleApiModel()
        {
            path = "https://www.googleapis.com/books/v1/volumes";
            changingValueParameters = new List<(string, string)>();
            constantQueryParameters = new Dictionary<string, string>()
            {
                { "maxResults", "20"}
            };

            foreach (var subject in Enum.GetNames(typeof(SubjectEnum)))
            {
                changingValueParameters.Add(("q", "subject:" + subject));
            }

            pagingParameters = ("startIndex", 0);
            itemsPerPage = 20;
        }
        enum SubjectEnum
        {
            fiction,
            historical,
        }
    }
}
