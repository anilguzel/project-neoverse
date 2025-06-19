using Microsoft.Extensions.DependencyInjection;
using Neoverse.ApiBase.Extensions;
using Neoverse.Customers.Infrastructure;
using Neoverse.Customers.Api;
using Neoverse.Customers.Application;
using Neoverse.SharedKernel.Configuration;
using Neoverse.SharedKernel.Interceptors;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Neoverse.ApiBase.Filters;
using Neoverse.ApiBase.Middleware;
using Neoverse.Customers.Infrastructure.Client;
using OpenTelemetry.Logs;
using Polly;

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
var redis = builder.Configuration.GetConnectionString("Redis");
var kafka = builder.Configuration.GetValue<string>("Kafka:BootstrapServers");

builder.Services.AddSingleton<AuditSaveChangesInterceptor>();
builder.Services.AddSingleton<MultiLanguageSaveChangesInterceptor>();
builder.Services.AddSingleton<DataProtectionSaveChangesInterceptor>();
builder.Services.AddCustomerInfrastructure(connectionString, redis, kafka, builder.Configuration, provider);
builder.Services.AddScoped<CustomerService>();

builder.Services.AddHttpClient<DocumentClient>(client =>
{
    client.BaseAddress = new Uri("http://document-service");
})
.AddTransientHttpErrorPolicy(p => p.CircuitBreakerAsync(3, TimeSpan.FromSeconds(30)));

builder.Services.AddOpenTelemetry()
    .WithTracing(b => b
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("Neoverse.Customers"))
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation());

builder.Services.AddSwaggerDocumentation("Customers API");

var app = builder.Build();

app.UseSwaggerDocumentation();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RequestResponseLoggingMiddleware>();

app.MapControllers();

app.Run();
