using ExchangeRateApp.Models;
using ExchangeRateApp.NBPApi;
using ExchangeRateApp.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("ExchangeRateAppDB")));
builder.Services.AddHttpClient("NBPApi", client => client.BaseAddress = new Uri(builder.Configuration.GetSection("NBPApiEndpoint").Value ?? string.Empty));
builder.Services.AddScoped<INBPApiService, NBPApiService>();
builder.Services.AddHostedService<PeriodicUpdateRateService>();
builder.Services.AddControllers(op => op.EnableEndpointRouting = false).AddNewtonsoftJson(options => options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
	c.SwaggerDoc("v1", new OpenApiInfo
	{
		Version = "v1",
		Title = "ExchangeRateApp API",
	});

	var xmlPath = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
	c.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
