
using Microsoft.EntityFrameworkCore;
using WijkMeld.API.Data;
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

builder.Services.AddScoped<IncidentService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
