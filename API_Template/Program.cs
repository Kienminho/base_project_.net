using Common.Authorization;
using Common.Settings;
using Entity;
using Infrastructure;
using log4net;
using log4net.Appender;
using log4net.Config;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Model.RequestModel;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using Asp.Versioning;
using Swashbuckle.AspNetCore.Filters;

[assembly: XmlConfigurator(ConfigFile = "log4net.config")]

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.PropertyNamingPolicy = null;
});
builder.Services.AddRouting(options => options.LowercaseUrls = true);

builder.Services.Configure<StrJWT>(builder.Configuration.GetSection("StrJWT"));
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
var strJwtSettingSection = builder.Configuration.GetSection("StrJWT");
var strJwtSettings = strJwtSettingSection.Get<StrJWT>();
var appSettings = builder.Configuration.GetSection("AppSettings").Get<AppSettings>();
var connectionStrings = builder.Configuration.GetConnectionString("WebApiDatabase");
// Load configuration
var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));
foreach (var appender in logRepository.GetAppenders())
{
    if (appender is AdoNetAppender adoNetAppender)
    {
        adoNetAppender.ConnectionString = connectionStrings;
        adoNetAppender.ActivateOptions(); // Apply changes
    }
} // set connection string for log4net

builder.Services.AddDbContext<ApiTemplateContext>(
    option => option.UseSqlServer(connectionStrings),
    ServiceLifetime.Scoped);
builder.Services.AddTransient<DbContext, ApiTemplateContext>();
builder.Services.RegisterInfrastructureServices(builder.Configuration);
builder.Services.ValidatorsServiceRegistration();
//builder.Services.AddCors(o => o.AddPolicy("MyCors", build =>
//{
//    build.WithOrigins("*").AllowAnyMethod().AllowAnyHeader();
//}));
//builder.Services.AddApplicationInsightsTelemetry(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]);
//builder.Services.AddHttpContextAccessor();
//builder.Services.AddEndpointsApiExplorer();

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = strJwtSettings.Issuer,
            ValidAudience = strJwtSettings.Audience,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(strJwtSettings.Key)),
            ClockSkew = TimeSpan.Zero
        };
    });
builder.Services.AddAuthorization();

//config api versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(2, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("X-Api-Version")
    );
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});
// set Token on swagger
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddSwaggerGen(s =>
{
    s.SwaggerDoc("v1", new OpenApiInfo { Title = "Template API - V1", Version = "v1.0" });
    s.SwaggerDoc("v2", new OpenApiInfo { Title = "Template API - V2", Version = "v2.0" });
    // To Enable authorization using Swagger (JWT)
    s.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        In = ParameterLocation.Header,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        Description = "JWT Authorization header using the Bearer scheme."
    });
    s.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    s.IncludeXmlComments(xmlPath);
    s.OperationFilter<SecurityRequirementsOperationFilter>();
    s.OperationFilter<CustomHeaderParameter>();
});

builder.Services.Configure<IISServerOptions>(options => { options.AllowSynchronousIO = true; });

builder.Services.Configure<FormOptions>(options =>
{
    options.MemoryBufferThreshold = int.MaxValue;
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartBodyLengthLimit = long.MaxValue;
    options.MultipartBoundaryLengthLimit = int.MaxValue;
    options.MultipartHeadersCountLimit = int.MaxValue;
    options.MultipartHeadersLengthLimit = int.MaxValue;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");        
        c.SwaggerEndpoint("/swagger/v2/swagger.json", "API v2");

    });
}
else if (app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "DocV1"); });
}

app.UseHttpsRedirection();
app.UseStaticFiles();

//get body from request middleware
app.UseMiddleware<ReadBodyFromRequestMiddleware>();
// custom jwt auth middleware
app.UseMiddleware<JwtMiddleware>();
// global error handler
app.UseMiddleware<ErrorHandlerMiddleware>();

app.UseAuthentication();
app.UseAuthorization();


app.UseCors(x => x
    .AllowAnyMethod()
    .AllowAnyHeader()
    .SetIsOriginAllowed(origin => true) // allow any origin
    .AllowCredentials());

// authentication Token
if(appSettings?.EnableMicrosoftCheckJwt ?? false) 
    app.MapControllers();
else
    app.MapControllers().RequireAuthorization();

app.Run();