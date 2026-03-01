using InventoryManagment.Data;
using InventoryManagment.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Swashbuckle.AspNetCore.Filters;
using System.Text;
using InventoryManagment.Models;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

// AddScoped One instance per HTTP request and then all classes share,
// AddSingleton One instance for the entire application all requests,
// AddTransient A new instance every time it's requested, every class gets its own.

builder.Services.AddScoped<IUserAccount, UserRepository>();
builder.Services.AddScoped<IBillOfSaleRepository, BillOfSaleRepository>();
builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
builder.Services.AddScoped<IItemRepository, ItemRepository>();
builder.Services.AddScoped<ILocationHistoryRepository, LocationHistoryRepository>();
builder.Services.AddScoped<IShipmentOrderRepository, ShipmentOrderRepository>();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection") ??
        throw new InvalidOperationException("Connection String is not found"));
});

builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddSignInManager()
    .AddRoles<IdentityRole>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudiences = builder.Configuration.GetSection("Jwt:Audiences").Get<List<string>>(),
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
});

// This makes it possible to test the API with Swagger and include the JWT token in the header for authenticated endpoints.
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });

    options.OperationFilter<SecurityRequirementsOperationFilter>();
});

//builder.Services.AddControllers()
//    .AddJsonOptions(options =>
//    {
//        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
//        options.JsonSerializerOptions.MaxDepth = 10;
//    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    Console.WriteLine("--> Dev mode, conn string: " + builder.Configuration.GetConnectionString("DefaultConnection"));
    Console.WriteLine($"Environment: {builder.Environment.EnvironmentName}");
}
else
{
    Console.WriteLine("--> Running in producion");
    Console.WriteLine("--> Connection string: " + builder.Configuration.GetConnectionString("DefaultConnection"));
}

app.UseHttpsRedirection();

// Validates JWT from request header.
// If valid, sets HttpContext.User.
// Does not block access by itself.
// This must be before UseAuthorization, otherwise the user won't be authenticated and will get 401 Unauthorized when trying to access protected endpoints.
app.UseAuthentication();

// Checks [Authorize] attributes on endpoints.
// Uses HttpContext.User.
// Returns 401/403 if requirements are not met.
// This must be after UseAuthentication, otherwise the user won't be authenticated and will get 401 Unauthorized when trying to access protected endpoints.
app.UseAuthorization();

app.MapControllers();

app.UseCors(options =>
{
    options.AllowAnyHeader();
    options.AllowAnyMethod();
    options.AllowAnyOrigin();
});

PrepDb.PrepPopulation(app, builder.Environment.IsProduction());

app.Run();