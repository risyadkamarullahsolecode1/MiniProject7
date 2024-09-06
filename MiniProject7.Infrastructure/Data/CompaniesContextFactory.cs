using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniProject7.Infrastructure.Data
{
    public class CompaniesContextFactory: IDesignTimeDbContextFactory<CompaniesContext>
    {
        public CompaniesContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("C:\\Users\\Risyad Kamarullah\\source\\repos\\MiniProject7\\MiniProject7.WebAPI\\appsettings.json")
                .Build();

            var services = new ServiceCollection();

            services.ConfigureInfrastructure(configuration);    
            var serviceProvider = services.BuildServiceProvider();
            return serviceProvider.GetRequiredService<CompaniesContext>();  
        }
    }
}
