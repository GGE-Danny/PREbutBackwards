using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PropertyService.Application.Abstractions;
using PropertyService.Infrastructure.Persistence;
using PropertyService.Infrastructure.Security;
using PropertyService.Infrastructure.Services;
using PropertyService.Infrastructure.Clients;



var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger + JWT
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "PropertyService API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header. Example: Bearer {token}",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new List<string>()
        }
    });
});

builder.Services.AddHttpClient("TenantService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:TenantService"]!);
});


builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantProvider, HttpTenantProvider>();
builder.Services.AddScoped<ITimelineWriter, TimelineWriter>();
builder.Services.AddScoped<ITenantServiceClient, TenantServiceClient>();

builder.Services.AddDbContext<PropertyDbContext>(opt =>
{
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// JWT validation (matches AuthService)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
            ),

            NameClaimType = ClaimTypes.NameIdentifier, // optional
            RoleClaimType = ClaimTypes.Role
        };
    });

// Role-based policies (fast path)
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("property.create", p => p.RequireRole("super_admin", "manager", "sales", "owner"));
    options.AddPolicy("property.read", p => p.RequireRole("super_admin", "manager", "sales", "support", "owner"));
    options.AddPolicy("property.update", p => p.RequireRole("super_admin", "manager", "sales", "owner"));
    options.AddPolicy("property.delete", p => p.RequireRole("super_admin", "manager"));

    options.AddPolicy("unit.create", p => p.RequireRole("super_admin", "manager", "sales", "owner"));
    options.AddPolicy("unit.read", p => p.RequireRole("super_admin", "manager", "sales", "support", "owner"));

    options.AddPolicy("occupancy.assign", p => p.RequireRole("super_admin", "manager", "sales", "owner"));
    options.AddPolicy("occupancy.read", p => p.RequireRole("super_admin", "manager", "sales", "support", "owner"));
    options.AddPolicy("tenant.internal.write", p => p.RequireRole("super_admin", "manager", "sales"));

});


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// ✅ correct order
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
