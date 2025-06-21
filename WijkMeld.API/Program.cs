
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using WijkMeld.API.Data;
using WijkMeld.API.Repositories.IncidentPhotos;
using WijkMeld.API.Repositories.Incidents;
using WijkMeld.API.Repositories.StatusUpdates;
using WijkMeld.API.Repositories.Users;
using WijkMeld.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<WijkMeldContext>(options =>
 options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IIncidentRepository, IncidentRepository>();
builder.Services.AddScoped<IStatusUpdateRepository, StatusUpdateRepository>();

builder.Services.AddScoped<IIncidentPhotoRepository, IncidentPhotoRepository>();

builder.Services.AddScoped<IncidentService>();
builder.Services.AddScoped<JwtService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// add Authentication with JWT

var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key)

        };
    });
builder.Services.AddAuthorization();

//Activate Controllers
builder.Services.AddControllers();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<WijkMeldContext>();

    Seed.Initialize(context);
}






if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.UseStaticFiles();

app.Run();
