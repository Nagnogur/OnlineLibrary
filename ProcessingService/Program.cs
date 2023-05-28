using ProcessingService;
using Microsoft.EntityFrameworkCore;
using ProcessingService.Entities;

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
    foreach (var book in books)
    {
        var possibleDuplicates = db.Books
        .Include(b => b.Authors)
        .Where(b => b.Title == book.Title);
        if (possibleDuplicates.Count() > 0)
        {
            /*foreach (var dup in possibleDuplicates)
            {
                if (dup.Authors != null && book.Authors != null && !dup.Authors.Select(a => a.Name).Except(book.Authors.Select(a => a.Name)).Any())
                {

                }
            }*/
            continue;
        }
        db.Books.Add(book);
    }
    int res;
    try
    {
        res = await db.SaveChangesAsync();
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex);
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

app.UseCors("DeleteLater");

app.Run();