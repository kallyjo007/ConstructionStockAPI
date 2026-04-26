using System.Text;
using ConstructionStockAPI.Data;
using ConstructionStockAPI.Helpers;
using ConstructionStockAPI.Middleware;
using ConstructionStockAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        return new BadRequestObjectResult(ApiResponse<object>.Fail("Validation failed."))
        {
            Value = ApiResponse<object>.Fail("Validation failed.")
        };
    };
});

builder.Services.AddDbContext<ConstructionStockDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<AlertService>();
builder.Services.AddScoped<ReportService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = builder.Configuration["Jwt:Issuer"],
            ValidAudience            = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey         = new SymmetricSecurityKey(
                                           Encoding.UTF8.GetBytes(
                                               builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins("http://127.0.0.1:5500", "http://localhost:5500")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

app.UseCors("AllowFrontend");

var webPath = Path.Combine(app.Environment.ContentRootPath, "ConstructionStock.Web");
app.UseDefaultFiles(new DefaultFilesOptions { 
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(webPath),
    RequestPath = "" 
});
app.UseStaticFiles(new StaticFileOptions { 
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(webPath),
    RequestPath = "" 
});

app.UseHttpsRedirection();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();