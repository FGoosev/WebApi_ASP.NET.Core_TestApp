using WebApiApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

string connection = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=WebApi;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

// добавляем контекст ApplicationContext в качестве сервиса в приложение
builder.Services.AddDbContext<ApplicationContext>(options => options.UseSqlServer(connection));


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options => {
    options.TokenValidationParameters = new TokenValidationParameters
    {
        // укзывает, будет ли валидироваться издатель при валидации токена
        ValidateIssuer = true,
        // строка, представляющая издателя
        ValidIssuer = "https://localhost:7159/",

        // будет ли валидироваться потребитель токена
        ValidateAudience = true,
        // установка потребителя токена
        ValidAudience = "https://localhost:7159/",
        // будет ли валидироваться время существования
        ValidateLifetime = true,

        // установка ключа безопасности
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("WVuRBzGfCqy1AWTgs1gs")),
        // валидация ключа безопасности
        ValidateIssuerSigningKey = true,
    };

});

builder.Services.AddSwaggerGen(options =>
{ 
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Description = "Bearer authentication with JWT token",
        Type = SecuritySchemeType.Http
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Id = "Bearer",
                    Type = ReferenceType.SecurityScheme
                }
            },
            new List<string>()
        }
    });
});

builder.Services.AddMvc();
// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddControllers().AddNewtonsoftJson(options => options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.UseAuthorization();
app.UseAuthentication();

app.MapControllers();

app.Run();
