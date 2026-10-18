using OnlineShop.Api.Data;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Api.Services;
using OnlineShop.Api.DTOs;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(option => option.UseSqlite(connectionString));

builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IAuthService, AuthService>();

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
app.MapPost("/api/v1/auth/register", async ([FromBody] RegisterRequestDto request, IAuthService authService, CancellationToken ct) =>
{
    try
    {
        await authService.RegisterAsync(request, ct);
        return Results.Ok(new { Message = "User registered Successfully" });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.Run();