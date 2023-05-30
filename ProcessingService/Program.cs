using ProcessingService;
using Microsoft.EntityFrameworkCore;
using ProcessingService.Entities;
using Azure;
using FuzzySharp;
using AutoMapper;
using System.Runtime.CompilerServices;
using ProcessingService.Processing;
using Microsoft.AspNetCore.Mvc;
using ProcessingService.RetrieveLogic;

///



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<LibraryContext>(options =>
    options.UseSqlServer(connectionString));

// TODO Delete later
builder.Services.AddCors(p => p.AddPolicy("DeleteLater", builder =>
{
    builder.WithOrigins("*").AllowAnyMethod().AllowAnyHeader();
}));

/*builder.Services.Configure<JsonOptions>(options =>
{
    options.JsonSerializerOptions.IncludeFields = true;
});*/

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("/processing/addbooks", async (List<Book> books, LibraryContext db) =>
{
    int res = 0;
    var bookTitles = db.Books.Select(b => b.Title).ToList();
    BookProcessing bookProcessing = new BookProcessing();

    foreach (var book in books)
    {
        var extractedMatch = Process.ExtractOne(book.Title, bookTitles, (s) => s);
        if (extractedMatch == null)
        {
            var isSaved = await bookProcessing.SaveBook(db, book);
            if (isSaved)
            {
                res++;
                bookTitles.Add(book.Title);
            }
            continue;
        }
        
        if (extractedMatch.Score >= 90)
        {
            Book? extractedBook = await db.Books
            .Include(b => b.Authors)
            .Include(b => b.Origin)
            .FirstOrDefaultAsync(b => b.Title == extractedMatch.Value);

            if (extractedBook == null)
            {
                continue;
            }

            var bookLink = book.Origin.FirstOrDefault();
            if (extractedBook.Origin.Where(o => o.Link == bookLink?.Link).Any())
            {
                ///  set missing parameters
                continue;
            }
            else if (bookLink != null)
            {
                extractedBook.Origin.Add(bookLink);
            }

            if (extractedBook.Authors == null || book.Authors == null)
            {
                /// set new book
                try
                {
                    db.Books.Update(extractedBook);
                    await db.SaveChangesAsync();
                    continue;
                }
                catch (Exception ex)
                {
                    continue;
                }
            }

            foreach (var author in book.Authors)
            {
                var extractedAuthorMatch = Process.ExtractOne(author.Name, extractedBook.Authors.Select(a => a.Name), (s) => s);
                
                if (extractedAuthorMatch.Score < 90)
                {
                    /// 
                    var isSaved = await bookProcessing.SaveBook(db, book);
                    if (isSaved)
                    {
                        res++;
                        bookTitles.Add(book.Title);
                    }
                    continue;
                }
                else
                {
                    try
                    {
                        db.Books.Update(extractedBook);
                        await db.SaveChangesAsync();
                        continue;
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }
                }
            }
        }
        else
        {
            var isSaved = await bookProcessing.SaveBook(db, book);
            if (isSaved)
            {
                res++;
                bookTitles.Add(book.Title);
            }
        }
    }
    
    if (res > 0)
    {
        return Results.Ok(res);
    }
    else
    {
        return Results.BadRequest();
    }
    

})
.WithName("AddBooksToDb");

app.MapGet("/processing/getbooks", async (LibraryContext db) =>
{
    var books = await db.Books
    .Include(b => b.Authors)
    .Include(b => b.Categories)
    .Include(b => b.IndustryIdentifiers)
    .Include(b => b.Origin)
    .ToListAsync();
    return books;
})
.WithName("BooksController");

/*app.MapGet("/api/books", async (LibraryContext db, [FromQuery]BookQueryParameters bookParameters) =>
{
    BookManipulationService bookManipulationService = new BookManipulationService(db);
    var res = await bookManipulationService.GetBooksWithParameters(bookParameters);
    
    return Results.Ok(res);
})
.WithName("BooksWithPaging");*/


app.UseCors("DeleteLater");

app.Run();