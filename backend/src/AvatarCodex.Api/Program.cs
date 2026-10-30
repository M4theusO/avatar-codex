using AvatarCodex.Api.Extensions;
using AvatarCodex.Application;
using AvatarCodex.Infrastructure;
using AvatarCodex.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApiControllers();
builder.Services.AddOpenApi();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddFrontendCors(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddReverseProxySupport();

var app = builder.Build();

if (app.Configuration.GetValue<bool>("Database:MigrateAndSeedOnStartup"))
{
    await app.Services.InitializeDatabaseAsync(app.Configuration.GetValue("Database:SeedDemoUser", true));
}

app.UseForwardedHeaders();
app.UseExceptionHandler();

// Sem UseHttpsRedirection: em produção o HTTPS termina no proxy (Vercel/Render), e o health check
// do Render chama o container direto em HTTP.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "Avatar Codex API v1"));
}

app.UseCors(CorsExtensions.FrontendPolicy);
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapApiHealthChecks();

app.Run();
