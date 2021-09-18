using AspNetCoreRateLimit;
using AutoMapper;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Diagnostics.CodeAnalysis;
using WebApiDemo.Api.ErrorHandling;
using WebApiDemo.Api.Security;
using WebApiDemo.Data;
using WebApiDemo.Domain.Cache;
using WebApiDemo.Domain.Mappings;
using WebApiDemo.Domain.Repositories;
using WebApiDemo.Domain.Validators;
using WebApiDemo.Service.Movie;

namespace WebApiDemo.Api
{
    [ExcludeFromCodeCoverage]
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            // serilog
            var logger = new LoggerConfiguration()
                .ReadFrom.Configuration(Configuration)
                .CreateLogger();
            services.AddSingleton<ILogger>(logger);

            // basic auth
            services.AddAuthentication("BasicAuthentication")
                .AddScheme<AuthenticationSchemeOptions,
                BasicAuthenticationHandler>("BasicAuthentication", null);

            // fluent validation
            services.AddMvc()
                .AddFluentValidation(
                    fv => fv.RegisterValidatorsFromAssemblyContaining<AddMovieRequestMovieValidator>())
                .AddFluentValidation(
                    fv => fv.RegisterValidatorsFromAssemblyContaining<UpdateMovieRequestMovieValidator>());

            // rate limit
            services.Configure<IpRateLimitOptions>(Configuration.GetSection("IpRateLimiting"));
            services.Configure<IpRateLimitPolicies>(Configuration.GetSection("IpRateLimitPolicies"));
            services.AddInMemoryRateLimiting();
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

            // distributed cache
            services.AddDistributedMemoryCache();
            services.AddSingleton<IDistributedCacheProvider, DistributedCacheProvider>();

            // automapper
            var mapperConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new MovieMappingProfile());
            });
            IMapper mapper = mapperConfig.CreateMapper();
            services.AddSingleton(mapper);

            // db & repoitory
            services.AddTransient<IMovieDataSource, MovieDataSource>();
            services.AddTransient<IMovieRepository, MovieRepository>();

            // services
            services.AddTransient<IMovieService, MovieService>();

            // swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "WebApi Demo - Movie API", Version = "v1" });
                c.AddSecurityDefinition("basic", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "basic",
                    In = ParameterLocation.Header,
                    Description = "Basic Authorization header using the Bearer scheme."
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                          new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "basic"
                                }
                            },
                            new string[] {}
                    }
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //The default ASP.NET Web API template registers the UseHttpsRedirection
            //middleware by default. However, when running the application in a container,
            //things like proper HTTPS configuration are in the hosting environment¡¯s
            //responsibility (eg. Kubernetes, Azure Container Instances, or the App Services runtime)
            // app.UseHsts();
            // app.UseHttpsRedirection();

            app.UseIpRateLimiting();

            // error handling middleware
            app.UseMiddleware<ErrorHandlingMiddleware>();

            app.UseAuthentication();

            app.UseRouting();

            app.UseCors();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebApi Demo - Movie API");
            });
        }
    }
}