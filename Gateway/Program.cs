using Gateway.Context;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

ConfigurationManager confManager = builder.Configuration;
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddIdentity<IdentityUser, IdentityRole>(
        options => {
            options.SignIn.RequireConfirmedAccount = false;

            options.User.RequireUniqueEmail = true;

            options.Password.RequireDigit = false;
            options.Password.RequiredLength = 6;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireLowercase = false;
        }
        )
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

//builder.Services.AddMvc();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
    {
        options.SaveToken = true;
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidAudience = confManager["JWT:ValidAudience"],
            ValidIssuer = confManager["JWT:ValidIssuer"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(confManager["JWT:Secret"]))
        };
    });

builder.Host.ConfigureServices((hostContext, services) =>
{
    IConfiguration configuration = hostContext.Configuration;

    services.AddHttpClient("InvokeServiceClient", client =>
    {
        client.BaseAddress = new Uri(configuration["ApiConfig:InvokeServiceUrl"]);
    });
    
    services.AddHttpClient("BookDataSourceClient", client =>
    {
        client.BaseAddress = new Uri(configuration["ApiConfig:BookDataSource"]);
    });

    /*services.AddHttpClient("ApiClient2", client =>
    {
        client.BaseAddress = new Uri(configuration["ApiConfig:Url2"]);
    });

    services.AddHttpClient("ApiClient3", client =>
    {
        client.BaseAddress = new Uri(configuration["ApiConfig:Url3"]);
    });*/

    //services.AddHostedService<Worker>();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
