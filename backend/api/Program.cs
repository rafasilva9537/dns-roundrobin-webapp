using System.Text.Json.Serialization;
using api.Configuration;
using api.Endpoints;
using api.Startup;
using api.Startup.OpenApiTransformers;

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

builder.Services.AddOpenApi(options => options.AddDocumentTransformer<BearerSecuritySchemeTransformer>());
builder.Services.AddGlobalExceptionHandling();

// TODO: Improve CORS to restrict to only the frontend
const string myAllowAllOrigins = "myAllowAllOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(myAllowAllOrigins,
        policyBuilder => policyBuilder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()
    );
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapApiDocumentation();
}

if (app.Environment.IsProduction())
{
    app.UseProductionDatabase();
}
else
{
    app.UseNonProductionDatabase();
}

app.UseExceptionHandler();

//app.UseHttpsRedirection();

app.UseCors(myAllowAllOrigins);

app.UseAuthentication();
app.UseAuthorization();

app.MapAccountEndpoints();
app.MapAuthEndpoints();

app.Run();