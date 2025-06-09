using MongoDB.Driver;
using Casino_onlineAPI.config;
using Casino_onlineAPI.modelos;
using Casino_onlineAPI.servicios;
using Stripe;


var builder = WebApplication.CreateBuilder(args);

//  controladores
builder.Services.AddControllers();

// Add services to the container.
builder.Services.AddRazorPages();


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMyOrigin",
        policy =>
        {
            policy.AllowAnyOrigin() // <--- ¡CAMBIO AQUÍ!
                   .AllowAnyHeader()
                   .AllowAnyMethod();
        });
});



//se añade Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

// Registra MongoDBService como Singleton
builder.Services.AddSingleton<MongoDBService>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    return new MongoDBService(configuration);
});

// Registra IMongoDatabase como Scoped, obteniéndola de MongoDBService
builder.Services.AddScoped<IMongoDatabase>(provider =>
{
    // ¡Ojo aquí! Database puede ser null si la conexión falla o el nombre de la DB no está en la URL.
    // Es importante manejar este posible null.
    var mongoService = provider.GetRequiredService<MongoDBService>();
    return mongoService.Database;
});

builder.Services.AddScoped<IUsuariosService, UsuariosService>();
builder.Services.AddScoped<IPartidasService, PartidasService>();
builder.Services.AddScoped<IJuegoService, JuegoService>();
builder.Services.AddScoped<ITransaccionService, TransaccionService>();



var app = builder.Build();

// Configure the HTTP request pipeline.

    app.UseSwagger();
    app.UseSwaggerUI();


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseCors("AllowMyOrigin");

app.UseAuthorization();

app.MapControllers();

app.MapRazorPages();
app.UseHttpsRedirection();

app.Run();
