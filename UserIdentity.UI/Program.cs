using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace UserIdentity.Api
{
    public class Program //NOSONAR
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
    }
}