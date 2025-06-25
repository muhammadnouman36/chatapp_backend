using System.Security.Claims;
using System.Text;
using Amazon.S3;
using Application.Interfaces.AppUsers;
using Application.Interfaces.Chat;
using Application.Interfaces.LinkFree;
using CommonOperations.Methods;
using Infrastructure.Context;
using Infrastructure.Services.AppUsers;
using Infrastructure.Services.Chat;
using Infrastructure.Services.LinkFree;
using Infrastructure.SignalR;
using LinkFree.InjectedServices;
using LinkFree.Midddleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;


builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssemblies(typeof(CustomeServices).Assembly);
});

builder.Services.AddCustomServices();

// Add services to the container.


builder.Services.AddControllers();
//builder.Services.AddControllers().AddApplicationPart(typeof(Program).Assembly);

var wasabiConfig = builder.Configuration.GetSection("Wasabi");
var accessKey = wasabiConfig["AccessKey"];
var secretKey = wasabiConfig["SecretKey"];
var serviceURL = wasabiConfig["ServiceURL"];
var bucketName = wasabiConfig["BucketName"];


builder.Services.AddSingleton<IAmazonS3>(sp =>
    new AmazonS3Client(
        accessKey,
        secretKey,
        new AmazonS3Config
        {
            ServiceURL = serviceURL,
            ForcePathStyle = true
        }
    ));

var serviceProvider = builder.Services.BuildServiceProvider();
var s3Client = serviceProvider.GetRequiredService<IAmazonS3>();
CommonMethods.Initialize(s3Client, bucketName);


builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer
(builder.Configuration.GetConnectionString("ConnectionString")));

builder.Services.AddSignalR();

// JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.SaveToken = true;
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidAudience = builder.Configuration["JWT:ValidAudience"],
            ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
            NameClaimType = ClaimTypes.NameIdentifier,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8
            .GetBytes(builder.Configuration["JWT:Secret"]))
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];

                // If the request is for our hub...
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) &&
                    (path.StartsWithSegments("/ChatHub")))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    });


builder.Services.AddCors(options => options.AddPolicy(name: "CorsPolicy",
builder =>
{
    builder.WithOrigins("http://localhost:3000", "https://localhost:7079/",
           "http://localhost:4200")
           .AllowAnyHeader()
           .AllowAnyMethod()
           .SetIsOriginAllowed((host) => true)
           .AllowCredentials();
}));

// Swagger Configuration
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opt =>
{
    opt.SwaggerDoc("v1", new OpenApiInfo { Title = "MyAPI", Version = "v1" });
    opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });

    opt.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});


builder.Services.AddSingleton(new TokenValidationParameters
{
    ValidateIssuer = true,
    ValidateAudience = true,
    ValidAudience = builder.Configuration["JWT:ValidAudience"],
    ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"])),
    ClockSkew = TimeSpan.Zero
});
builder.Services.AddTransient<JWTMiddleware>();


var app = builder.Build();

// Enable Developer Exception Page for debugging
//if (app.Environment.IsDevelopment())
//{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
//}

app.UseHttpsRedirection();
app.UseMiddleware<JWTMiddleware>();
app.UseCors("CorsPolicy");


app.UseAuthentication();
app.UseAuthorization();

app.MapHub<ChatHub>("/ChatHub");


app.MapControllers();

app.Run();
