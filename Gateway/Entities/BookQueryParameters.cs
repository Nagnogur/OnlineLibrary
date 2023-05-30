﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using System.Reflection;

namespace ProcessingService.RetrieveLogic
{
    public class BookQueryParameters
    {
        public string? Title { get; set; }
        public string? Publisher { get; set; }
        public float? MinRating { get; set; }
        public float? MaxRating { get; set; }
        public string? IdentifierCode { get; set; }
        public float? MinPrice { get; set; }
        public float? MaxPrice { get; set; }
        public int? MaxPageCount { get; set; }
        public int? MinPageCount { get; set; }
        public string? Domain { get; set; }
        public string? Author { get; set; }
        public DateTime? MinPublishDate { get; set; }
        public DateTime? MaxPublishDate { get; set; } = DateTime.Now;
        public DateTime? MinRetrievedDate { get; set; }
        public DateTime? MaxRetrievedDate { get; set; } = DateTime.Now;
    }
}
       