using myapplication.Models;
using Microsoft.EntityFrameworkCore;
using myapplication.Services;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var provider = builder.Services.BuildServiceProvider();
var config = provider.GetRequiredService<IConfiguration>();
// Add services to the container.

builder.Services.AddControllers();
services.AddTransient<IProductService, ProductService>();
services.AddTransient<ICategoryService, CategoryService>();

services.AddDbContext<AdventureWorks2022Context>(opts =>
    opts.UseSqlServer(builder.Configuration.GetConnectionString("ProductDbConnection")));
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp",
        builder => builder
            .WithOrigins("http://localhost:4200")
            .AllowAnyMethod()
            .AllowAnyHeader());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("AllowAngularApp");
app.UseAuthorization();

app.MapControllers();

app.Run();
