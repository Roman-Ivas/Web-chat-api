using Microsoft.AspNetCore.Identity;


using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.SignalR;
using LAST.Data.Context;
using LAST.Hubs;
using LAST.Services;
using LAST.Models.IdentityModels;

var builder = WebApplication.CreateBuilder(args);

//Add services to the container.
//builder.Services.AddDbContext<ApplicationContext>(options =>
//{
//    options.UseSqlServer(builder.Configuration
//        .GetConnectionString("DefaultConnection"));
//});

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration
        .GetConnectionString("DefaultConnection"));
});
//builder.Services.AddIdentity<AppUser, IdentityUserRole<int>>()
//        .AddEntityFrameworkStores<AppDbContext>();

//builder.Services.AddScoped<DbDataInitializer>();



builder.Services.AddScoped<IAuthService, SqlDbAuthService>();
builder.Services.AddScoped<IChatDbService, SqlChatDbService>();

builder.Services.AddCors(options =>
{
    // TODO configure CORS for prod environment
    options.AddPolicy("CorsPolicy",
        builder => builder.WithOrigins("http://localhost:4200", "https://*.gitlab.io")
        .SetIsOriginAllowedToAllowWildcardSubdomains()
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials());
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => AuthOptions.ConfigureJwtBearer(options));

builder.Services.AddSignalR();
builder.Services.AddSingleton<IUserIdProvider, SignalrEmailBasedUserIdProvider>();

builder.Services.AddControllers();
builder.Services.AddHostedService<TokenCleanerService>();



//builder.Services.AddIdentity<User, IdentityRole>()
//    .AddEntityFrameworkStores<ApplicationContext>();
//builder.Services.AddControllersWithViews();
//builder.Services.AddControllers();
//// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//builder.Services.AddCors(option =>
//{
//    option.AddPolicy("MyPolicy", builder =>
//    {
//        builder.AllowAnyOrigin()
//        .AllowAnyMethod()
//        .AllowAnyHeader();
//    });
//});
//builder.Services.AddAuthentication(x =>
//{
//    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//}).AddJwtBearer(x =>
//{
//    x.RequireHttpsMetadata = false;
//    x.SaveToken = true;
//    x.TokenValidationParameters = new TokenValidationParameters
//    {
//        ValidateIssuerSigningKey = true,
//        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("veryverysceret.....")),
//        ValidateAudience = false,
//        ValidateIssuer = false,
//        ClockSkew = TimeSpan.Zero

//    };
//});
var app = builder.Build();



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
//app.UseCors("MyPolicy");

//InitializeDb(app); // seed DB data

app.UseRouting();

app.UseCors("CorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints => {
    endpoints.MapControllers();
    endpoints.MapHub<ChatHub>("/chat");
});

app.UseAuthorization();

app.MapControllers();

app.Run();
