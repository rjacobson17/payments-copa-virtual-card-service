using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.Extensions.Options;
using COPA.APIManager.BLL.Interface;
using COPA.APIManager.BLL;
using COPA.APIManager.DAL.Interface;
using COPA.APIManager.DAL.Implementation;
using COPA.APIManager.BLL.Clients.Conservice;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;
using Conservice.COPA.VirtualCardAPI.Util;
using Microsoft.AspNetCore.Http;
using System.Linq;
using Coravel;
using Payment.Portal.API.Auth;
using Payment.Portal.CL.Utilities.Interfaces;
using COPA.APIManager.Utilities.Implementations;
using COPA.APIManager.Utilities.Interfaces;

namespace Conservice.COPA.VirtualCardAPI
{
    /// <summary>
    /// Initializes the requirements for the website
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Configures the services that will be used.
        /// </summary>
        public IConfiguration Configuration { get; }
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="configuration">The configuration parameters provided by the launchSettings.json file.</param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            // Check that we have the required Configuration values set in the environment
            var configList = new List<string>()
            {
                "HealthCheckURI",
                "PingURI",
            };

            EnsureConfiguration(configList, Configuration);

            ConfigureVersioning(services);
            services.AddMvc();
            services.AddControllers().AddNewtonsoftJson();
            ConfigureAuthentication(services);
            ConfigureSwagger(services);
            ConfigureDataAccess(services);
            ConfigureV2Dependencies(services);
            services.AddScheduler();
            services.AddSingleton<IHttpResiliencePolicy, HttpResiliencePolicy>();
            services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<IClaimsPrincipalService, JwtClaimsPrincipalService>();

            services.AddCors(options => options.AddPolicy("AllowStatusCheckOrigins", policy =>
            {
                var allowedOrigins = Configuration
                    .GetSection("HealthChecks_Cors_Hosts")
                    .Get<string[]>();

                policy.WithOrigins(allowedOrigins)
                    .SetIsOriginAllowedToAllowWildcardSubdomains()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            }));

            services.AddHealthChecks()
                .AddCheck<AppInfoHealthCheck>("Application Information")
                .AddUrlGroup(new Uri(Configuration.GetValue<string>("ServicePrefixes:CSI")), "CSI Service");
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">Defines a class that provides the mechanisms to configure an application's request.</param>
        /// <param name="env">Provides information about the web hosting environment an application is running in.</param>
        /// <param name="provider">Defines the behavior of a provider that discovers and describes API version information with an application.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApiVersionDescriptionProvider provider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                foreach (var description in provider.ApiVersionDescriptions)
                {
                    c.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
                }
            });

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseCors();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();

                endpoints
                    .MapHealthChecks(pattern: Configuration["HealthCheckURI"],
                        new HealthCheckOptions
                        {
                            Predicate = _ => true,
                            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                        }
                    )
                    .RequireCors("AllowStatusCheckOrigins");

                endpoints
                    .MapGet(Configuration["PingURI"], async context => { await context.Response.WriteAsync("OK"); })
                    .RequireCors("AllowStatusCheckOrigins");
            });

            app.Run(context =>
            {
                context.Response.Redirect("swagger");
                return Task.CompletedTask;
            });
        }

        private void ConfigureVersioning(IServiceCollection services)
        {
            services.AddApiVersioning(config =>
            {
                // Additional config options could be used, but these are some suggested best practice settings
                // See various reference links for more information

                // These lines are only necessary if there is a version of the API in use before versioning was implemented
                config.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
                config.AssumeDefaultVersionWhenUnspecified = true;

                // Returns in the response headers the available and deprecated versions of the API
                config.ReportApiVersions = true;
            });
        }

        private void ConfigureAuthentication(IServiceCollection services)
        {
            var tokenRolesPath = Configuration.GetValue<string>("Token_Roles_Path");

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(o =>
            {
                string keycloakPrefix = Configuration["Keycloak_Authority_Prefix"];
                string keycloakSuffix = Configuration["Keycloak_Authority_Suffix"];

                o.Authority = keycloakPrefix + keycloakSuffix;
                o.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidAudiences = Configuration.GetSection("Keycloak_Audiences").Get<string[]>()
                };

                o.RequireHttpsMetadata = true;
                o.Events = new JwtBearerEvents()
                {
                    OnAuthenticationFailed = c =>
                    {
                        if (c.Exception.GetType() == typeof(SecurityTokenExpiredException))
                        {
                            Console.WriteLine("OnTokenValidated: The access token provided has expired.");
                        }
                        else
                        {
                            Console.WriteLine("OnTokenValidated: Authentication failed. Most likely reason is that the access token provided is not valid.");
                        }
                        return Task.CompletedTask;  // Needed because OnAuthenticationFailed() returns a Task.
                    }
                };
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("CanViewVirtualCard", policy =>
                    policy.RequireClaim(tokenRolesPath, "CanViewVirtualCard"));
                options.AddPolicy("CanUpdateVirtualCard", policy =>
                    policy.RequireClaim(tokenRolesPath, "CanUpdateVirtualCard"));
                options.AddPolicy("CanCreateVirtualCard", policy =>
                    policy.RequireClaim(tokenRolesPath, "CanCreateVirtualCard"));
            });
        }

        private void ConfigureSwagger(IServiceCollection services)
        {
            services.AddVersionedApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";

                options.SubstituteApiVersionInUrl = true;
            });

            services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

            services.AddSwaggerGen(c =>
            {
                c.OperationFilter<SwaggerDefaultValues>();

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "",
                    In = ParameterLocation.Header,
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header,
                        },
                        new List<string>()
                    }
                });

                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });
        }

        private void ConfigureDataAccess(IServiceCollection services)
        {
            services.AddSingleton<ICredentialsManager, CredentialsManager>();
            services.AddSingleton<ICSIApiAccess, CSIApiAccess>();
            
        }

        private void ConfigureV2Dependencies(IServiceCollection services)
        {
            services.AddSingleton<ConserviceApiManager>();
        }

        private static void EnsureConfiguration(IEnumerable<string> settings, IConfiguration configuration)
        {
            var missing = new List<string>();
            foreach (var s in settings)
            {
                if (configuration.GetSection(s)?.Value == null
                    && configuration.GetSection(s)?.Get<string[]>() == null)
                {
                    missing.Add(s);
                }
            }
            if (missing.Any())
            {
                throw new Exception($"Missing the following configurations: {string.Join('\n', missing)}");
            }
        }
    }
}
