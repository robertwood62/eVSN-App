using Fri.FieldPlotService.Api.Common;
using Fri.FieldPlotService.Common;
using Fri.FieldPlotService.Managers;
using Fri.FieldPlotService.Tools;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;

const string CorsPolicyName = "CorsPolicy";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

// Add the Field Plot Service specific dependencies.
builder.Services.AddSingleton<HttpClient>();
builder.Services.AddSingleton<DbManagerOptions>();
builder.Services.AddSingleton<DbManager>();
builder.Services.AddSingleton<FieldPlotServiceOptions>();
builder.Services.AddSingleton<VendorManager>();
builder.Services.AddSingleton<ProjectManager>();
builder.Services.AddSingleton<SyncManager>();

// Add the ASP.NET services
builder.Services.AddControllers();
builder.Services.AddCors(o => o.AddPolicy(CorsPolicyName, builder =>
{
    builder.AllowAnyOrigin()
           .AllowAnyMethod()
           .AllowAnyHeader();
}));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Field Plot Service API", Version = "v1" });
    var filePath = Path.Combine(AppContext.BaseDirectory,  "Fri.FieldPlotService.Api.xml");
    c.IncludeXmlComments(filePath);
    c.SchemaFilter<AutoRestSchemaFilter>();

    // Create a custom operation ID generator
    c.CustomOperationIds((e) =>
    {
        return $"{e.ActionDescriptor.RouteValues["action"]}";
    });

    var scheme = new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        In = ParameterLocation.Header,
        Name = "Token Authentication",
        Description = "Azure Active Directory Token",
        Reference = new OpenApiReference
        {
            Id = "Token",
            Type = ReferenceType.SecurityScheme
        }
    };

    c.AddSecurityDefinition(scheme.Reference.Id, scheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {scheme, Array.Empty<string>() }
    });

});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(CorsPolicyName);
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
