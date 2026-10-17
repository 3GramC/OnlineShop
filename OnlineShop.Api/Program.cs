using OnlineShop.Api.Data;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(option => option.UseSqlite(connectionString));

builder.Services.AddScoped<IProductService, ProductService>();

var app =  builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/api/v1/products", async (IProductService productService, CancellationToken ct) =>
{
    var products = await productService.GetAllAsync(ct);
    return Results.Ok(products);
});
app.Run();