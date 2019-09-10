using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Jaeger;
using Jaeger.Samplers;
using OpenTracing;
using OpenTracing.Contrib.NetCore.CoreFx;
using OpenTracing.Util;
using ServiceD.Handlers;

namespace ServiceD {
    public class Startup {
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            services.AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddTransient<RequestTracingHandler>();
            services.AddHttpClient("default")
                .AddHttpMessageHandler<RequestTracingHandler>();

            services.AddSingleton<ITracer>(serviceProvider => {
                var serviceName = serviceProvider
                    .GetRequiredService<IHostingEnvironment>()
                    .ApplicationName;

                var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

                var tracer = new Tracer.Builder(serviceName)
                    .WithSampler(new ConstSampler(true))
                    .WithLoggerFactory(loggerFactory)
                    .Build();

                // Allows code that can't use DI to also access the tracer.
                GlobalTracer.Register(tracer);

                return tracer;
            });

            services.Configure<HttpHandlerDiagnosticOptions>(options => {
                options.IgnorePatterns.Add(x => !x.RequestUri.IsLoopback);
            });
            
            services.AddOpenTracing(b => b.AddAspNetCore());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            } else {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            // app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}