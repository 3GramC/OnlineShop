using OnlineShop.Api.Data;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Api.Services;
using OnlineShop.Api.Domain.Enums;
using OnlineShop.Api.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using OnlineShop.Api.Exceptions;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(option => option.UseSqlite(connectionString));
builder.Services.AddAuthorization();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddAuthentication("CookieAuth").AddCookie("CookieAuth", option =>
{
    option.Cookie.Name = "OnlineShop.Auth";
    option.Cookie.HttpOnly = true;
    option.Cookie.SecurePolicy = CookieSecurePolicy.Always;

    option.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };
    option.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    };
});

builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();

var app =  builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var adminEmail = builder.Configuration["AdminSettings:Email"];
    var adminPassword = builder.Configuration["AdminSettings:Password"];
    if (string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminPassword))
    {
        throw new InvalidOperationException("Admin credentials are not configured.");
    }
    await AdminSeeder.SeedAdminAsync(db, adminEmail, adminPassword, CancellationToken.None);
}
app.MapGet("/api/v1/products", async (IProductService productService, CancellationToken ct) =>
{
    var products = await productService.GetAllAsync(ct);
    return Results.Ok(products);
});

app.MapGet("/api/v1/profile", (HttpContext httpContext) =>
{
    var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var email = httpContext.User.FindFirst(ClaimTypes.Email)?.Value;
    var fullName = httpContext.User.FindFirst(ClaimTypes.Name)?.Value;

    return Results.Ok(new { userId, email, fullName });
}).RequireAuthorization();

app.MapGet("/api/v1/admin/dashboard", () =>
{
    return Results.Ok(new { Message = "Welcome, Admin!" });
}).RequireAuthorization(policy => policy.RequireRole(nameof(UserRole.Admin)));

app.MapPost("/api/v1/auth/register", async ([FromBody] RegisterRequestDto request, IAuthService authService, CancellationToken ct) =>
{
    await authService.RegisterAsync(request, ct);
    return Results.Ok(new { Message = "User registered Successfully" });
}).WithParameterValidation();

app.MapPost("/api/v1/auth/login", async ([FromBody] LoginRequestDto request, IAuthService AuthService,
    HttpContext httpContext, CancellationToken ct) =>
{
    var user = await AuthService.LoginAsync(request, ct);
    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Name, user.FullName),
        new Claim(ClaimTypes.Role, user.Role.ToString())
    };
    var claimsIdentity = new ClaimsIdentity(claims, "CookieAuth");
    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
    await httpContext.SignInAsync("CookieAuth", claimsPrincipal);
    return Results.Ok(new { Message = "Login Successfully" });
}).WithParameterValidation();

app.MapGet("/api/v1/cart", async (ICartService cartService, HttpContext httpContext, CancellationToken ct) =>
{
    var userIdString = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (!int.TryParse(userIdString, out var userId))
    {
        return Results.Unauthorized();
    }
    var cart = await cartService.GetCartAsync(userId, ct);
    return Results.Ok(cart);
}).RequireAuthorization();

app.MapPost("/api/v1/cart/items", async (AddToCartRequestDto request,ICartService cartService, HttpContext httpContext, CancellationToken ct) =>
{
    var userIdString = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if(!int.TryParse(userIdString, out var userId))
    {
        return Results.Unauthorized();
    }
    await cartService.AddToCartAsync(userId, request, ct);
    return Results.Ok(new { Message = "Item added to cart successfully" });
}).RequireAuthorization().WithParameterValidation();

app.MapDelete("/api/v1/cart/items/{cartItemId}", async (int cartItemId, ICartService cartService, HttpContext httpContext, CancellationToken ct) =>
{
    var userIdString = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (!int.TryParse (userIdString, out var userId))
    {
        return Results.Unauthorized();
    }
    await cartService.RemoveFromCartAsync(userId, cartItemId, ct);
    return Results.Ok(new {Message = "Item removed from cart." });
}).RequireAuthorization();

app.MapPost("/api/v1/orders", async ([FromBody] CreateOrderRequestDto request, HttpContext httpContext, IOrderService orderService, CancellationToken ct) =>
{
    var userIdString = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (!int.TryParse(userIdString, out var userId))
    {
        return Results.Unauthorized();
    }
    var orderId = await orderService.CreateOrderAsync(userId,request, ct);
    return Results.Ok(new { Message = "Order Created successfully", OrderId = orderId });
}).RequireAuthorization().WithParameterValidation();

app.MapGet("api/v1/orders", async (IOrderService orderService, HttpContext httpContext, CancellationToken ct) =>
{
    var userIdString = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (!int.TryParse(userIdString, out var userId))
    {
        return Results.Unauthorized();
    }
    var orders =await orderService.GetUserOrdersAsync(userId, ct);
    return Results.Ok(orders);
}).RequireAuthorization();

app.MapPost("api/v1/admin/products", async ([FromBody] CreateProductRequestDto request, IProductService productService, CancellationToken ct = default) =>
{
    var productId = await productService.CreateProductAsync(request, ct);
    return Results.Ok(new { Message = "Product created successfully", ProductId = productId });
}).RequireAuthorization(policy => policy.RequireRole(nameof(UserRole.Admin))).WithParameterValidation();

app.MapPut("api/v1/admin/products/{id}", async (int id ,[FromBody] UpdateProductRequestDto request, IProductService productService, CancellationToken ct ) =>
{
    await productService.UpdateProductAsync(id, request, ct);
    return Results.Ok(new { Message = "Product updated successfully" });
}).RequireAuthorization(policy => policy.RequireRole(nameof(UserRole.Admin))).WithParameterValidation();

app.MapDelete("api/v1/admin/products/{id}", async (int id, IProductService productService, CancellationToken ct) =>
{
    await productService.DeleteProductAsync(id, ct);
    return Results.Ok(new { Message = "Product deleted successfully" });
}).RequireAuthorization(policy => policy.RequireRole(nameof(UserRole.Admin))).WithParameterValidation();

app.Run();