using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Text;
using System.Threading.Tasks;

namespace CapiOnline
{
    class Program
    {
        static void Main(string[] args)
        {
            //Console.WriteLine("Hello World!");
            BuildWebHost(args).Run();

        }

        public static IWebHost BuildWebHost(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
        }
    }

    public class Startup
    {
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            //app.UseAuthentication();
            //app.UseResponseCompression();
            app.UseStaticFiles();

            //app.UseMiddleware<MeuMiddleware>();
            app.UseMeuMiddleware();

            app.UseMvc(routes =>
             {
                 routes.MapRoute(
                     name: "default",
                     template: "{controller=Home}/{action=Index}/{id?}");
             });
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
        }

    }

    public class MeuMiddleware
    {
        private RequestDelegate _next;

        public MeuMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task Invoke(HttpContext httpContext)
        {
            //Antes
            httpContext.Request.EnableRewind();

            var request = await FormatRequest(httpContext.Request);
            var log = new LoggerConfiguration()
            .WriteTo.Logentries("xxxxx-yyyyy-uuuuu-vvvvv")
            .CreateLogger();
            log.Information($"request {request}");

            httpContext.Request.Body.Position = 0;
            await _next(httpContext);
            //Depois
        }

        private static async Task<string> FormatRequest(HttpRequest request)
        {
            var body = request.Body;
            request.EnableRewind();
            var buffer = new byte[Convert.ToInt32(request.ContentLength)];
            await request.Body.ReadAsync(buffer, 0, buffer.Length);
            var bodyAsText = Encoding.UTF8.GetString(buffer);
            request.Body = body;

            var messageObjToLog = new { scheme = request.Scheme, host = request.Host, path = request.Path, queryString = request.Query, requestBody = bodyAsText };

            return JsonConvert.SerializeObject(messageObjToLog);
        }
    }


    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseMeuMiddleware(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<MeuMiddleware>();
        }

    }
}
