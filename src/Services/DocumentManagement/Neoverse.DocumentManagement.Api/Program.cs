using Neoverse.ApiBase.Extensions;
using Neoverse.DocumentManagement.Infrastructure;
using Neoverse.DocumentManagement.Application;
using Neoverse.SharedKernel.Interceptors;
using Neoverse.SharedKernel.Configuration;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Neoverse.ApiBase.Filters;
using Neoverse.ApiBase.Middleware;
using OpenTelemetry.Logs;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddOpenTelemetry(options =>
{
    options.IncludeScopes = true;
    options.IncludeFormattedMessage = true;
    options.ParseStateValues = true;
    options.AddOtlpExporter(o =>
    {
        var endpoint = builder.Configuration.GetValue<string>("Otlp:Endpoint");
        o.Endpoint = new Uri(endpoint);
    });
});

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ResponseMaskingFilter>();
    options.Filters.Add<DataPrivacyFilter>();
});

var provider = Enum.Parse<DatabaseProvider>(builder.Configuration.GetValue<string>("Database:Provider") ?? "Postgres", true);
var connectionString = provider switch
{
    DatabaseProvider.SqlServer => builder.Configuration.GetConnectionString("SqlServer"),
    DatabaseProvider.Oracle => builder.Configuration.GetConnectionString("Oracle"),
    _ => builder.Configuration.GetConnectionString("Postgres")
};
var redis = builder.Configuration.GetConnectionString("Redis") ?? "localhost";

builder.Services.AddSingleton<AuditSaveChangesInterceptor>();
builder.Services.AddSingleton<MultiLanguageSaveChangesInterceptor>();
builder.Services.AddSingleton<DataProtectionSaveChangesInterceptor>();
builder.Services.AddDocumentInfrastructure(connectionString, redis, builder.Configuration, provider);
builder.Services.AddScoped<DocumentService>();

builder.Services.AddOpenTelemetry().WithTracing(b => b
    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("Neoverse.DocumentManagement"))
    .AddAspNetCoreInstrumentation());

builder.Services.AddSwaggerDocumentation("DocumentManagement API");

var app = builder.Build();

app.UseSwaggerDocumentation();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RequestResponseLoggingMiddleware>();

app.MapControllers();

app.Run();
