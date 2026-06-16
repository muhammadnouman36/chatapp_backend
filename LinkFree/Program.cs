using System.Security.Claims;
using System.Text;
using Application.Interfaces.AppUsers;
using Application.Interfaces.Chat;
using Application.Interfaces.LinkFree;
using Infrastructure.Context;
using Infrastructure.Services.AppUsers;
using Infrastructure.Services.Chat;
using Infrastructure.Services.LinkFree;
using Infrastructure.SignalR;
using LinkFree.InjectedServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddCustomeService();

builder.Services.AddControllers();
builder.Services.AddControllers().AddApplicationPart(typeof(Program).Assembly);


builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer
(builder.Configuration.GetConnectionString("ConnectionString")));

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
    });

builder.Services.AddCors(options => options.AddPolicy(name: "CorsPolicy",
builder =>
{
    builder.WithOrigins("http://localhost:3000", "https://localhost:7079/",
           "http://localhost:4200","https://blabster.pages.dev")
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

// required: bind to dynamic port from Render
//builder.WebHost.UseUrls($"http://*:{Environment.GetEnvironmentVariable("PORT") ?? "5000"}");

var app = builder.Build();

// Enable Developer Exception Page for debugging
//if (app.Environment.IsDevelopment())
//{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
//}

//app.UseHttpsRedirection();
app.UseCors("CorsPolicy");

app.MapHub<ChatHub>("/ChatHub");

app.UseAuthentication(); // Ensure authentication middleware is before authorization
app.UseAuthorization();

app.MapControllers();

app.Run();
