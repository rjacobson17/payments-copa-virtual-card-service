using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Conservice.COPA.VirtualCardAPI
{
    /// <summary>
    /// Beginning class of application.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Main entry point of application.
        /// </summary>
        public static void Main()
        {
            CreateHostBuilder().Build().Run();
        }

        private static IHostBuilder CreateHostBuilder() =>
            Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
