using AvatarCodex.Api.Extensions;
using AvatarCodex.Infrastructure;
using AvatarCodex.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddFrontendCors(builder.Configuration);

var app = builder.Build();

if (app.Configuration.GetValue<bool>("Database:MigrateAndSeedOnStartup"))
{
    await app.Services.InitializeDatabaseAsync();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "Avatar Codex API v1"));
}
else
{
    app.UseHttpsRedirection();
}

app.UseCors(CorsExtensions.FrontendPolicy);
app.UseAuthorization();

app.MapControllers();
app.MapApiHealthChecks();

app.Run();
