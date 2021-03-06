﻿using System;
using System.IO;
using LidarrAPI.Database;
using LidarrAPI.Release;
using LidarrAPI.Release.Azure;
using LidarrAPI.Release.Github;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Octokit;
using Serilog;

namespace LidarrAPI
{
    public class Startup
    {
        public Startup(IHostEnvironment env)
        {
            // Loading .NetCore style of config variables from json and environment
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Config = builder.Build();
            ConfigLidarr = Config.GetSection("Lidarr").Get<Config>();

            SetupDataDirectory();

            Log.Debug($@"Config Variables
            ----------------
            DataDirectory  : {ConfigLidarr.DataDirectory}
            Database       : {ConfigLidarr.Database}
            APIKey         : {ConfigLidarr.ApiKey}");
        }

        public IConfiguration Config { get; }
        
        public Config ConfigLidarr { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<Config>(Config.GetSection("Lidarr"));
            services.AddDbContextPool<DatabaseContext>(o => o.UseMySql(ConfigLidarr.Database));
            services.AddSingleton(new GitHubClient(new ProductHeaderValue("LidarrAPI")));

            services.AddTransient<ReleaseService>();
            services.AddTransient<GithubReleaseSource>();
            services.AddTransient<AzureReleaseSource>();

            services
                .AddControllers()
                .AddJsonOptions(options => options.JsonSerializerOptions.IgnoreNullValues = true);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostEnvironment env, ILoggerFactory loggerFactory)
        {
            UpdateDatabase(app);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSerilogRequestLogging();

            app.UseRouting();
            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }

        private void SetupDataDirectory()
        {
            // Check data path
            if (!Path.IsPathRooted(ConfigLidarr.DataDirectory))
            {
                throw new Exception($"DataDirectory path must be absolute.\nDataDirectory: {ConfigLidarr.DataDirectory}");
            }

            // Create
            Directory.CreateDirectory(ConfigLidarr.DataDirectory);
        }

        private static void UpdateDatabase(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices
                   .GetRequiredService<IServiceScopeFactory>()
                   .CreateScope())
            {
                using (var context = serviceScope.ServiceProvider.GetService<DatabaseContext>())
                {
                    context.Database.Migrate();
                }
            }
        }
    }
}
