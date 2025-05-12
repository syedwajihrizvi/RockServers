using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Amazon;
using Amazon.S3;
using Amazon.Extensions.NETCore.Setup;
using RockServers.Data;
using RockServers.Models;
using Newtonsoft.Json;
using RockServers.Interfaces;
using RockServers.Services;
using System.Reflection;
var builder = WebApplication.CreateBuilder(args);

// var awsConfig = builder.Configuration.GetSection("AWS");
var accessKey = builder.Configuration["AWS:AccessKey"];
var secretKey = builder.Configuration["AWS:SecretKey"];
var region = builder.Configuration["AWS:Region"];
var password = builder.Configuration["DB:Password"];

if (accessKey == null || secretKey == null || region == null)
    throw new Exception("Aws Settings not verified");

var port = Environment.GetEnvironmentVariable("PORT") ?? "5191";

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(int.Parse(port));
});

builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

builder.Services.AddSingleton<IAmazonS3>(sp =>
{
    var credentials = new Amazon.Runtime.BasicAWSCredentials(accessKey, secretKey);
    var config = new AmazonS3Config
    {
        RegionEndpoint = RegionEndpoint.GetBySystemName(region)
    };
    return new AmazonS3Client(credentials, config);
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "RockServers", Version = "v1", });
});

var defaultConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (defaultConnectionString == null)
    throw new Exception("DB Connection failed");

var connectionString = defaultConnectionString.Replace("__DB_PASSWORD", password);
builder.Services.AddDbContext<ApplicationDBContext>(options =>
{
    options.UseMySql(
        connectionString,
        new MySqlServerVersion(new Version(9, 0, 1)));
});

builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 10;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
}).AddEntityFrameworkStores<ApplicationDBContext>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme =
    options.DefaultChallengeScheme =
    options.DefaultForbidScheme =
    options.DefaultScheme =
    options.DefaultSignInScheme =
    options.DefaultSignOutScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["JWT:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["JWT:Audience"],
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:SigningKey"]!))
    };
});

builder.Services.AddScoped<ITokenService, TokenService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
