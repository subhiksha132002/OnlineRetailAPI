using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OnlineRetailAPI.Data;
using OnlineRetailAPI.Services.Implementations;
using OnlineRetailAPI.Services.Interfaces;
using System.Security.Claims;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null; // Keeps PascalCase in JSON
    });

builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();

//For Keycloak
builder.Services.AddHttpClient<IKeycloakAdminService, KeycloakAdminService>();
builder.Services.AddScoped<IKeycloakAdminService, KeycloakAdminService>();


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger with JWT Bearer Authentication - FIXED
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Online Retail API",
        Version = "v1",
        Description = "API for Online Retail Store with Keycloak Authentication"
    });

    // Add JWT Bearer security definition
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token (without 'Bearer' prefix)"
    });
    // Make all endpoints require authentication
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            new string[] {}
        }
    });
});

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))) ;

//To connect application with redis and utilize it
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("RedisConn");
    options.InstanceName = builder.Configuration["RedisCacheSettings:InstanceName"];
});

//Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins("http://localhost:4208")
                                .AllowAnyHeader()
                                .AllowAnyMethod()
                                .AllowCredentials();
                      });
});


//Configure Keycloak Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "http://localhost:8080/realms/onlineretail";
        options.Audience = "angular-app"; 
        options.RequireHttpsMetadata = false; // for local development

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false, // Set to true in production
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.Zero,
            RoleClaimType = ClaimTypes.Role // This tells .NET where to find roles
        };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                Console.WriteLine(" Token validated successfully");

                var claimsIdentity = context.Principal?.Identity as ClaimsIdentity;
                if (claimsIdentity != null)
                {
                    // Keycloak puts roles in "realm_access" JSON object
                    // We need to extract them and add as role claims
                    var realmAccessClaim = claimsIdentity.FindFirst("realm_access");

                    if (realmAccessClaim != null)
                    {
                        Console.WriteLine($" Found realm_access claim");

                        try
                        {
                            // Parse the realm_access JSON
                            var realmAccess = JsonDocument.Parse(realmAccessClaim.Value);

                            if (realmAccess.RootElement.TryGetProperty("roles", out var rolesElement))
                            {
                                Console.WriteLine(" Extracting roles from realm_access:");

                                // Add each role as a proper role claim
                                foreach (var role in rolesElement.EnumerateArray())
                                {
                                    var roleValue = role.GetString();
                                    if (!string.IsNullOrEmpty(roleValue))
                                    {
                                        Console.WriteLine($"  Adding role: {roleValue}");
                                        claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, roleValue));
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($" Error parsing realm_access: {ex.Message}");
                        }
                    }

                    // All final claims for debugging
                    Console.WriteLine("\n Final Claims:");
                    foreach (var claim in claimsIdentity.Claims)
                    {
                        Console.WriteLine($"  {claim.Type} = {claim.Value}");
                    }
                    Console.WriteLine();
                }

                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                Console.WriteLine($" Challenge: {context.Error}, {context.ErrorDescription}");
                return Task.CompletedTask;
            },
            OnForbidden = context =>
            {
                Console.WriteLine("Access forbidden - User doesn't have required role");

                // Debug: print user's roles
                var user = context.HttpContext.User;
                var roles = user.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value);
                Console.WriteLine($"User has these roles: {string.Join(", ", roles)}");
                Console.WriteLine($"Checking for role 'admin-onlineretail': {user.IsInRole("admin-onlineretail")}");

                return Task.CompletedTask;
            }
        };
    });



// Configure Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("admin-onlineretail"));

    options.AddPolicy("AuthenticatedUser", policy =>
        policy.RequireAuthenticatedUser());
});


var app = builder.Build();


// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{

    app.UseSwagger(options =>
    {
        options.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi2_0;  
    });

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Online Retail API v1");
        options.RoutePrefix = "swagger";
        options.DocumentTitle = "Online Retail API";
    });
}

app.UseRouting();

app.UseCors(MyAllowSpecificOrigins); //Middleware(MUST come early! Before Authentication/Authorization)


app.UseHttpsRedirection(); //Must come before UseAuthorization and UseAuthorization
                               //Authentication - Check who you are
app.UseAuthentication();// Must come before UseAuthorization

//Authorization - Check what you can do
app.UseAuthorization();


//Controllers - Handle requests
app.MapControllers();

app.Run();


