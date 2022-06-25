using food_truck_api;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

[assembly: FunctionsStartup(typeof(Startup))]

namespace food_truck_api
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            // Get configuration manager
            IConfiguration config = builder.GetContext().Configuration;

            // Add IHttpClientFactory into the dependency injection container
            builder.Services.AddHttpClient("truckdata", ftd =>
            {
                ftd.BaseAddress = new Uri(config["dataUrl"]);
            });

            // Add the CosmosDB client instance into the dependency injection container
            builder.Services.AddSingleton((cc) => {
                return new CosmosClientBuilder(config["endpointUrl"], config["authKey"])
                        .Build();
            });
        }
    }
}
