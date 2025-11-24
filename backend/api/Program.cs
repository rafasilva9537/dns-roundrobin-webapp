using System.Text.Json.Serialization;
using api.Configuration;
using api.Endpoints;
using api.Startup;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddOptions<JwtConfig>()
    .BindConfiguration(JwtConfig.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.ConfigureHttpJsonOptions(options => options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

builder.Services.AddAppDbContext(builder.Configuration);

builder.Services.AddAppServices();

builder.Services.AddJwtBearerAuthentication(builder.Configuration);
builder.Services.AddAppIdentity();
builder.Services.AddPolicyBasedAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (app.Environment.IsProduction())
{
    app.UseProductionDatabase();
}
else
{
    app.UseNonProductionDatabase();
}

app.UseHttpsRedirection();

app.UseAuthorization();
app.UseAuthentication();

app.MapAccountEndpoints();
app.MapAuthEndpoints();

app.Run();