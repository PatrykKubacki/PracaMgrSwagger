using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Converter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Microsoft.VisualBasic;
using PracaMgrSwagger.FakeData;
using PracaMgrSwagger.Hubs;
using PracaMgrSwagger.Models;
using FakeDatas = PracaMgrSwagger.FakeDater.FakeData;

namespace PracaMgrSwagger
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Praca magisterska", Version = "1.0.0" });
            });

            services.AddCors(options => options.AddPolicy("CorsPolicy", builder => builder.WithOrigins("http://localhost:3000")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials()
                    .SetIsOriginAllowed((host) => true)));

            services.AddSignalR(hubOptions => {
                hubOptions.EnableDetailedErrors = true;
            }).AddNewtonsoftJsonProtocol();

            services.AddSingleton<ChartHubConnections>();
            services.AddSingleton<DataSourceFromFiles>();
            services.AddMediatR(Converter.AssemblyInfo.Assembly);
            services.AddMediatR(AssemblyInfo.Assembly);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime hostApplicationLifetime)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });

            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseAuthorization();
            app.UseCors("CorsPolicy");

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<ChartHub>("/chart");
                endpoints.MapControllers();  
            });

            hostApplicationLifetime.ApplicationStarted.Register(() =>
            {

                var serviceProvider = app.ApplicationServices;
                var chartHub = (IHubContext<ChartHub>)serviceProvider.GetService(typeof(IHubContext<ChartHub>));
                var chartHubConnections = (ChartHubConnections)serviceProvider.GetService(typeof(ChartHubConnections));

                QMeterProtocol.QMeterProtocol qMeterProtocol = new(chartHub, chartHubConnections);
                qMeterProtocol.Run();

           // var timer = new System.Timers.Timer
          //  {
           //     Interval = 500,
           //     Enabled = true
            //};
            //timer.Elapsed += delegate (object sender, System.Timers.ElapsedEventArgs e)
           // {
              //  var chartHubConnections = (ChartHubConnections)serviceProvider.GetService(typeof(ChartHubConnections));
              //  var dataSourceFromFiles = (DataSourceFromFiles)serviceProvider.GetService(typeof(DataSourceFromFiles));

                //    foreach (var conn in chartHubConnections.Connections)
                //    {
                //        //v1.0
                       //var chart = FakeDatas.GetChartData(chartHubConnections.Connection);

                //v2.0
                //var chart = FakeDatas.GetChartDataFromS2PFile(dataSourceFromFiles, chartHubConnections.Connection);

                //        //v3.0
                //        QMeterProtocol.QMeterProtocol qMeterProtocol = new(chartHub);
                //        qMeterProtocol.Run();

                //        chartHub.Clients.Client(conn.Key).SendAsync("SendChart", qMeterProtocol.ChartData);
                //    }

                   // chartHub.Clients.All.SendAsync("SendChart", chart);
               // };
                //timer.Start();
            });
        }

    }
}
