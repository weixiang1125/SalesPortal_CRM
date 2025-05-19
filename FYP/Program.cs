using CRM_API.Mappings;
using CRM_API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SharedLibrary;
using SharedLibrary.Hubs;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Get the connection string from appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// Add JWT authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var cookie = context.Request.Cookies["authToken"];
                if (!string.IsNullOrEmpty(cookie))
                {
                    context.Token = cookie;
                }
                return Task.CompletedTask;
            }
        };

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"])),
            RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
        };
    });

builder.Services.AddAuthorization();

// Add the JSON options configuration here
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
    });



// Add services to the container
builder.Services.AddScoped<IUsersService, UsersService>();  // Register UserService
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IContactService, ContactService>();
builder.Services.AddScoped<IDealService, DealService>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddSingleton<WhatsAppService>();
builder.Services.AddSignalR(); // so you can inject IHubContext

builder.Services.AddControllers();

// Update CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("https://localhost:44373") // Your frontend URL
              .AllowCredentials()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddAutoMapper(typeof(AutoMapperProfile));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.WebHost.UseUrls("https://localhost:44309", "https://0.0.0.0:44309");

var app = builder.Build();


app.UseCors("AllowFrontend");

// Add this after UseCors()
app.UseCookiePolicy(new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.None,
    HttpOnly = HttpOnlyPolicy.Always,
    Secure = CookieSecurePolicy.Always
});

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
// Enable Authentication middleware
app.UseAuthentication(); // Add this line to enable JWT Authentication

app.UseAuthorization();
app.MapHub<ChatHub>("/chathub");
app.MapControllers();

app.Run();
