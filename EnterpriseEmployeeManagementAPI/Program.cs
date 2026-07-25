using Asp.Versioning;
using EnterpriseEmployeeManagementAPI.Data;
using EnterpriseEmployeeManagementAPI.Extensions;
using EnterpriseEmployeeManagementAPI.Logging;
using EnterpriseEmployeeManagementAPI.Middleware;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables(prefix: "EEMA_");

builder.Host.UseSerilog((context, _, loggerConfiguration) =>
    LoggingConfiguration.Configure(
        loggerConfiguration,
        context.Configuration,
        context.HostingEnvironment));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();
builder.Services.AddEmployeeModule(builder.Configuration);
builder.Services
    .AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
    })
    .AddMvc()
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

await SeedData.InitializeAsync(app.Services);

app.Run();

public partial class Program;
