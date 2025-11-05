using HomeCenter.Core;
using HomeCenter.Core.Interface;
using HomeCenter.Core.Netatmo;
using HomeCenter.WebApi;

const string allowUiOriginsPolicy = "AllowUiOrigins";

var builder = WebApplication.CreateBuilder(args);

var allowedOrigins = builder.Configuration.GetValue<string>("CorsAllowedOrigins");
if (!string.IsNullOrEmpty(allowedOrigins))
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy(name: allowUiOriginsPolicy,
            policy =>
            {
                policy.WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
    });
}

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient();
builder.Services.AddMemoryCache();
builder.Services.Configure<NetatmoOptions>(builder.Configuration.GetSection("Netatmo"));
builder.Services.AddSingleton<IOAuthClient, OAuthClient>();
builder.Services.AddScoped<IAccessTokenProvider, AccessTokenProvider>();
builder.Services.AddScoped<IWeatherService, NetatmoWeatherService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseCors(allowUiOriginsPolicy);

app.UseAuthorization();

app.MapControllers();

app.Run();
