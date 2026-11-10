using ActosAdultosAPI.Data;
using ActosAdultosAPI.Models;
using ActosAdultosAPI.Services.Implementation;
using ActosAdultosAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Entity Framework + SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString(
            "DefaultConnection")));

// Identity
builder.Services
    .AddIdentityCore<AppUser>(options =>
    {
        options.User.RequireUniqueEmail = true;

        // Contraseñas
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 8;

        // No bloquear usuarios por intentos fallidos
        options.Lockout.AllowedForNewUsers = false;

        // Obligatorio confirmar email
        options.SignIn.RequireConfirmedEmail = true;

        // Duración del token de confirmación
        options.Tokens.EmailConfirmationTokenProvider =
            TokenOptions.DefaultProvider;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

// JWT
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException(
        "JWT Key no configurada.");

if (Encoding.UTF8.GetByteCount(jwtKey) < 32)
{
    throw new InvalidOperationException(
        "La clave JWT debe tener al menos 32 bytes.");
}

var jwtIssuer = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException(
        "JWT Issuer no configurado.");

var jwtAudience = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException(
        "JWT Audience no configurado.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;

        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtIssuer,

                ValidateAudience = true,
                ValidAudience = jwtAudience,

                ValidateLifetime = true,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtKey)),

                NameClaimType =
                    System.Security.Claims.ClaimTypes.Name,

                RoleClaimType =
                    System.Security.Claims.ClaimTypes.Role,

                ClockSkew = TimeSpan.Zero
            };
    });

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Authorization
builder.Services.AddAuthorization();


// JUNTA API
builder.Services.AddHttpClient<
    IJuntaApiService,
    JuntaApiService>(
    (serviceProvider, client) =>
    {
        var configuration = serviceProvider
            .GetRequiredService<IConfiguration>();

        var url = configuration["JuntaApi:BaseUrl"]
            ?? throw new InvalidOperationException(
                "JuntaApi:BaseUrl no configurada.");

        client.BaseAddress = new Uri(url);
        client.Timeout = TimeSpan.FromSeconds(10);
    });
// memoria cache para guardar los codigos junta
builder.Services.AddMemoryCache();

// QuestPDF
QuestPDF.Settings.License = LicenseType.Community;

// Services
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ICiudadService, CiudadService>();
builder.Services.AddScoped<IActoService, ActoService>();
builder.Services.AddScoped<IInstitucionService, InstitucionService>();
builder.Services.AddScoped<IPostulanteService, PostulanteService>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IOrientacionService, OrientacionService>();
builder.Services.AddScoped<ICargoService, CargoService>();
builder.Services.AddScoped<IInscripcionService, InscripcionService>();
builder.Services.AddScoped<IInscripcionPdfService, InscripcionPdfService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseCors("AngularPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
