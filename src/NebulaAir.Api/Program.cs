using Microsoft.EntityFrameworkCore;
using NebulaAir.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Obtener cadena de conexión
var connectionString = builder.Configuration.GetConnectionString("NebulaAirDb");

// Registrar DbContext con EF Core y SQL Server
builder.Services.AddDbContext<NebulaAirDbContext>(options =>
    options.UseSqlServer(connectionString));

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<NebulaAirDbContext>();
    await DbSeeder.SeedAsync(db);
}

app.Run();
