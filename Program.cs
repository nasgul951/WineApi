using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using WineApi.Data;
using WineApi.Extensions;
using WineApi.Helpers;

var builder = WebApplication.CreateBuilder(args);


// Get Db Options
var dbOptions = builder.Configuration.GetDbOptions();
var connString = $"server={dbOptions.Server};user={dbOptions.User};password={dbOptions.Password};database={dbOptions.Database}";
var serverVersion = ServerVersion.AutoDetect(connString);

// Add services to the container.
builder.Services.AddDbContext<WineContext>(
    dbContextOptions => dbContextOptions
        .UseMySql(connString, serverVersion)
        .LogTo(Console.WriteLine, LogLevel.Information)
);

builder.Services
    .AddDataServices()
    .AddHttpContextAccessor()
    .AddAuthentication("Token")
    .AddScheme<AuthenticationSchemeOptions, TokenAuthenticationHandler>("Token", null);

builder.Services.AddControllers();

builder.Services.AddCorsPolicy(builder.Configuration);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseCors("CorsPolicy");
app.UseAuthorization();

app.MapControllers();

app.Run();
