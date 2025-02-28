using BusinessLayer.Service;
using RepositoryLayer.Service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddScoped<UserRegistrationBL>();
builder.Services.AddScoped<UserRegistrationRL>();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
