using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Bux.Dbo;
using MySqlConnector;
using Serilog;
using Bux.Middleware;
using Microsoft.AspNetCore.HttpOverrides;
using Bux.Simulate;

if (false)
{
    Bux.Tests.Test.TestAll();
    Console.WriteLine("Press any key to exit program");
    Console.ReadKey();
    Environment.Exit(0);
}

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
//builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);
builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: false, reloadOnChange: true);
builder.Configuration.AddEnvironmentVariables();



if (builder.Configuration["db_connection_string"] == null)
{
    throw new Exception("missing configuration value: db_connection_string");

}
if (builder.Configuration["db_user"] == null)
{
    throw new Exception("missing configuration value: db_user");

}
if (builder.Configuration["db_password"] == null)
{
    throw new Exception("missing configuration value: db_password");

}


if (builder.Configuration["db_major_version"] == null)
{
    throw new Exception("missing configuration value: db_major_version");
}
var dbMajorVersion = builder.Configuration.GetValue<int>("db_major_version");

if (builder.Configuration["db_minor_version"] == null)
{
    throw new Exception("missing configuration value: db_minor_version");
}
var dbMinorVersion = builder.Configuration.GetValue<int>("db_minor_version");

var dbServerVersion = new MariaDbServerVersion(new Version(dbMajorVersion, dbMinorVersion));

var dbConnectionString = builder.Configuration.GetValue<string>("db_connection_string");
dbConnectionString += $";user={builder.Configuration.GetValue<string>("db_user")}";
dbConnectionString += $";password={Bux.Encryption.EncryptionHelper.Decrypt(builder.Configuration.GetValue<string>("db_password"))}";

if (builder.Environment.EnvironmentName == "Test")
{
    var section = builder.Configuration.GetSection("Kestrel:Certificates:Default");
    var encryptedPassword = section.GetValue<string>("Password");
    var decryptedPassword = Bux.Encryption.EncryptionHelper.Decrypt(encryptedPassword);
    builder.Configuration["Kestrel:Certificates:Default:Password"] = decryptedPassword;
}


void ConfigureDbContextOptions(DbContextOptionsBuilder opt)
{
    opt.UseMySql(dbConnectionString, dbServerVersion)
        //.LogTo(Console.WriteLine, LogLevel.Information)
        .LogTo(Console.WriteLine, LogLevel.Warning)
        //.LogTo(Console.WriteLine, LogLevel.Debug)
        //.EnableSensitiveDataLogging()
        .EnableDetailedErrors();
}

DbContextOptions<Db>  createDbContextOptions()
{
    var optionsBuilder = new DbContextOptionsBuilder<Db>();
    ConfigureDbContextOptions(optionsBuilder);
    return optionsBuilder.Options;
}

builder.Services.AddDbContext<Db>(ConfigureDbContextOptions);
builder.Services.AddMySqlDataSource(dbConnectionString);

// this registers IHttpClientFactory with a default 3s timeout
//builder.Services.AddHttpClient();
builder.Services.AddHttpClient("my_client", c =>
{
    c.Timeout = TimeSpan.FromSeconds(5);
})
.ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
{
    ConnectTimeout = TimeSpan.FromSeconds(10)
});



builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Logging.ClearProviders();
Log.Logger = new LoggerConfiguration()
    //.MinimumLevel.Debug()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", Serilog.Events.LogEventLevel.Warning)
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss.fff} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.Debug()
    .CreateLogger();
builder.Host.UseSerilog();
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);


builder.Services.AddHttpContextAccessor();


builder.Services.AddDataProtection()
    .PersistKeysToDbContext<Db>();

builder.Services.AddScoped<Bux.Templates.TemplateService>(provider =>
{
    Bux.Dbo.Db db = provider.GetService<Bux.Dbo.Db>();
    Bux.Lang.LanguageService languageService = provider.GetService<Bux.Lang.LanguageService>();
    return new Bux.Templates.TemplateService(builder.Configuration, db, languageService);
});

builder.Services.AddScoped<Bux.Lang.LanguageService>(provider =>
{
    return new Bux.Lang.LanguageService();
});

builder.Services.AddScoped<Bux.Email.EmailService>(provider =>
{
    Bux.Lang.LanguageService languageService = provider.GetService<Bux.Lang.LanguageService>();
    Bux.Templates.TemplateService templateService = provider.GetService<Bux.Templates.TemplateService>();
    return new Bux.Email.EmailService(builder.Configuration, languageService, templateService);
});

builder.Services.AddScoped<Bux.VerificationCode.UserVerificationCodeService>(provider =>
{
    Bux.Dbo.Db db = provider.GetService<Bux.Dbo.Db>();
    return new Bux.VerificationCode.UserVerificationCodeService(db);
});

builder.Services.AddScoped<Bux.Sessionn.SessionService>(provider =>
{
    Bux.Dbo.Db db = provider.GetService<Bux.Dbo.Db>();
    HttpContext context = provider.GetRequiredService<IHttpContextAccessor>().HttpContext;
    return new Bux.Sessionn.SessionService(db, context);
});

builder.Services.AddSingleton<Bux.Auth.TokenService>(provider =>
{
    var dbContextOptions = createDbContextOptions();
    return new Bux.Auth.TokenService(builder.Configuration, dbContextOptions);
});

builder.Services.AddScoped<Bux.Auth.ApiKeyService>(provider =>
{
    MySqlDataSource dataSource = provider.GetService<MySqlDataSource>();
    return new Bux.Auth.ApiKeyService(dataSource);
});

builder.Services.AddHostedService<SimulatePlayingUsersService>(provider =>
{
    MySqlDataSource dataSource = provider.GetService<MySqlDataSource>();
    return new SimulatePlayingUsersService(dataSource);
});




// Set the JSON serializer options
builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = null;
});

builder.Services.AddControllers();


var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
    //ForwardLimit = 1, // one proxy: Nginx
    // Trust only your proxy:
    //KnownProxies = { IPAddress.Parse("127.0.0.1") }
});

app.UseMiddleware<SessionMiddleware>();
app.UseMiddleware<AuthMiddleware>();


app.MapControllers();



app.Run();
