using AutoMapper;
using HAS.Content.ApplicationServices.Messaging;
using HAS.Content.Data;
using HAS.Content.Feature.Azure;
using HAS.Content.Feature.EventLog;
using IdentityServer4.AccessTokenValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Logging;
using System;

namespace HAS.Content
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;

            var testConfig = Configuration["MPY:Version"];
            var hasTestConfig = Configuration["HAS:Version"];

            Environment = env;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddAutoMapper(typeof(Startup));
            services.AddMediatR(typeof(Startup));

            services.AddCors();

            services.AddTransient<ICloudStorageService, CloudStorageService>();
            services.AddScoped<ContentContext>();
            services.AddScoped<LibraryContext>();
            services.AddSingleton<IQueueService>(opt =>
            {
                var queueService = AzureStorageQueueService.Create(Configuration["Azure:Storage:ConnectionString"]);
                queueService.CreateQueue(Configuration["Azure:Storage:Queue:LogEventMPY:Name"]).Wait();

                return queueService;
            });

            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(EventLogBehavior<,>));

            services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = Configuration["MPY:IdentityServer:Authority"];
                    options.ApiName = "MPY.Content";
                    if (Environment.IsDevelopment())
                    {
                        options.RequireHttpsMetadata = false;
                    }
                });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("student", policyAdmin => policyAdmin.RequireClaim("role", "student"));
                options.AddPolicy("instructor", policyAdmin => policyAdmin.RequireClaim("role", "instructor"));
                options.AddPolicy("admin", policyAdmin => policyAdmin.RequireClaim("role", "admin"));
                options.AddPolicy("superadmin", policyAdmin => policyAdmin.RequireClaim("role", "superadmin"));
            });

            services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 6000000000;
            });

            services.AddAntiforgery(options =>
            {
                // Set Cookie properties using CookieBuilder properties†.
                options.FormFieldName = "AntiforgeryFieldname";
                options.HeaderName = "X-CSRF-TOKEN-HEADERNAME";
                options.SuppressXFrameOptionsHeader = false;
            });

            services.AddControllers(config =>
            {
                config.Filters.Add<GenerateAntiforgeryTokenCookieAttribute>();
                config.Filters.Add<DisableFormValueModelBindingAttribute>();
            }).SetCompatibilityVersion(Microsoft.AspNetCore.Mvc.CompatibilityVersion.Version_3_0);

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors(options =>
            {
                options.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
            });

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }

}
