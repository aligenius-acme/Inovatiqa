using Inovatiqa.Services.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Inovatiqa.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                }).ConfigureServices(services =>
                {
                    services.AddHostedService<EmailSenderTaskService>();
                }).ConfigureServices(services =>
                {
                    services.AddHostedService<DeleteGuestsTaskService>();
                }).ConfigureServices(services =>
                {
                    services.AddHostedService<UpdateEntityCounterTaskService>();
                }).ConfigureServices(services =>
                {
                    services.AddHostedService<UpdateRootCategoryIdsForProductsTaskService>();
                }).ConfigureServices(services =>
                {
                    services.AddHostedService<UpdateRolesForEachCategoryTaskService>();
                });
        //public static IHostBuilder CreateHostBuilder(string[] args) =>
        //    Host.CreateDefaultBuilder(args)
        //        .ConfigureWebHostDefaults(webBuilder =>
        //        {
        //            webBuilder.UseStartup<Startup>();
        //        });
    }
}
