using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json.Serialization;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;
using System.Text;
using Microsoft.OpenApi.Models;
using RizkyAPI.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddCors(
    o =>
    {
        o.AddPolicy("AllowAllCorsPolicy", builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()
        );
    }
    );
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

AddSwaggerDoc(builder.Services);

void AddSwaggerDoc(IServiceCollection services)
{
    builder.Services.AddSwaggerGen(c =>
    {
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = @"Jwt Authorization Header using the Bearer Scheme, Enter your token ini here. Example : '123kodeacak456!'",
            Name = "Authorization",
            In = ParameterLocation.Header,
            //Type = SecuritySchemeType.ApiKey,
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT"
        }); ;
        c.AddSecurityRequirement(new OpenApiSecurityRequirement() {
            {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference{ Type = ReferenceType.SecurityScheme, Id = "Bearer"},
                Scheme = "0auth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new List<string>()
            }
        });
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "Tutorial API", Version = "v1" });
    });
}

builder.Host.UseSerilog(
    (ctx, lc) => lc
    .WriteTo.Console()
    .WriteTo.File(
        path: "logs\\log-.txt",
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
        rollingInterval: RollingInterval.Day,
        restrictedToMinimumLevel: LogEventLevel.Information
    )
);
builder.Services.AddControllersWithViews().AddNewtonsoftJson(options => options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore)
    .AddNewtonsoftJson(options => options.SerializerSettings.ContractResolver = new DefaultContractResolver());

//====================================================
var tokenValidParam = new TokenValidationParameters()
{
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWTConfig:Key"])),
    ValidateIssuer = false, //Cek alamat lokasi server false
    ValidateAudience = false, //Cek alamat lokasi server false
    ValidateLifetime = true,
    ValidateIssuerSigningKey = true,
    ValidIssuer = builder.Configuration["JWTConfig:Issuer"],
    ValidAudience = builder.Configuration["JWTConfig:Audience"]
};
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(
        authenticationScheme: JwtBearerDefaults.AuthenticationScheme,
        configureOptions: opt =>
        {
            opt.IncludeErrorDetails = true;
            opt.TokenValidationParameters = tokenValidParam;
        }
    );
builder.Services.AddSingleton(tokenValidParam);
//====================================================
builder.Services.AddAuthorization();
//builder.Services.AddMvc();
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//app.UseCors(options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
app.UseCors("AllowAllCorsPolicy");

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
//app.MapControllerRoute(name:"default", pattern:"{controller=Home}/{action=Index}/{id?}");

app.UseStaticFiles(new StaticFileOptions { FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "Photos")), RequestPath = "/Photos" });
//app.UseEndpoints(
//    endpoints => { endpoints.MapControllers(); }
//);
app.MapControllers();
app.Run();
