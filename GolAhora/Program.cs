using Microsoft.EntityFrameworkCore;
using AppContext = GolAhora.Data.AppContext;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Conexion a la base de datos
builder.Services.AddDbContext<AppContext>(
    options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
    );

//ligas
builder.Services.AddScoped<GolAhora.Services.LeagueService>();
//torneos 
builder.Services.AddScoped<GolAhora.Services.TournamentService>();
//equipos
builder.Services.AddScoped<GolAhora.Services.TeamService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
