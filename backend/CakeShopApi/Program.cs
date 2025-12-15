using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using CakeShopApi.Data;
using CakeShopApi.Services;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------
// DATABASE (SQLite)
// ---------------------------------------------------------
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=cakeshop.db"
    )
);

// ---------------------------------------------------------
// SERVICES
// ---------------------------------------------------------
builder.Services.AddScoped<ITokenService, TokenService>();

// ---------------------------------------------------------
// CORS (Frontend access)
// ---------------------------------------------------------
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173",   // Vite
                "http://localhost:3000"    // React
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// ---------------------------------------------------------
// CONTROLLERS
// ---------------------------------------------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ---------------------------------------------------------
// SWAGGER + JWT AUTHORIZE 🔒 BUTTON
// ---------------------------------------------------------
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {your JWT token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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

// ---------------------------------------------------------
// JWT AUTHENTICATION
// ---------------------------------------------------------
var jwt = builder.Configuration.GetSection("Jwt");
var key = jwt["Key"] ?? "SUPER_SECRET_DEV_KEY_CHANGE_IN_PRODUCTION";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // dev only
    options.SaveToken = true;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwt["Issuer"],
        ValidAudience = jwt["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(key)
        )
    };
});

// ---------------------------------------------------------
// BUILD APP
// ---------------------------------------------------------
var app = builder.Build();

// ---------------------------------------------------------
// APPLY MIGRATIONS + SEED DATABASE
// ---------------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    DbSeeder.Seed(db);
}

// ---------------------------------------------------------
// MIDDLEWARE PIPELINE
// ---------------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors();              // MUST be before auth
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
