using System;
using System.Diagnostics;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Penzle.Core;
using Penzle.Core.Http;
using Penzle.Core.Http.Internal;
using Penzle.Core.Models;

namespace Penzle.Net.Microsoft.DependencyInjection;

/// <summary>
///     Extensions to add Penzle Client to IoC.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Registers penzle client from the specified assemblies
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configurator">Configuration data</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection AddPenzleClient(this IServiceCollection services,
        Action<PenzleServiceConfiguration> configurator)
    {
        if (configurator == null)
            throw new ArgumentNullException(nameof(configurator), "The configurator cannot be null.");

        var penzleServiceConfiguration = new PenzleServiceConfiguration();
        configurator(penzleServiceConfiguration);

        if (string.IsNullOrWhiteSpace(penzleServiceConfiguration.ApiDeliveryKey) &&
            string.IsNullOrWhiteSpace(penzleServiceConfiguration.ApiManagementKey))
            throw new ArgumentNullException(nameof(configurator),
                "At least one key needs to be presented ApiDeliveryKey or ApiManagementKey.");

        if (penzleServiceConfiguration.BaseUri == null)
            throw new ArgumentNullException(nameof(penzleServiceConfiguration.BaseUri),
                "It is necessary to specify the base url of the API.");

        if (string.IsNullOrWhiteSpace(penzleServiceConfiguration.Environment))
            throw new ArgumentNullException(nameof(penzleServiceConfiguration.Environment),
                "It is necessary to specify the target environment.");

        if (string.IsNullOrWhiteSpace(penzleServiceConfiguration.Project))
            throw new ArgumentNullException(nameof(penzleServiceConfiguration.Project),
                "It is necessary to specify the target project.");

        if (penzleServiceConfiguration.TimeOut == default)
        {
            penzleServiceConfiguration.TimeOut = TimeSpan.FromSeconds(30);
            Debug.Write(
                "The default settings for request time out has been set. Default value is 10 seconds. In order to override this value please pass value to penzleServiceConfiguration.");
        }

        var connection = new Connection
        (
            penzleServiceConfiguration.BaseUri,
            new ApiOptions(penzleServiceConfiguration.Project, penzleServiceConfiguration.Environment),
            new InMemoryCredentialStore(new BearerCredentials(penzleServiceConfiguration.ApiDeliveryKey,
                penzleServiceConfiguration.ApiManagementKey)),
            new HttpClientAdapter(() => new HttpClientHandler()),
            new MicrosoftJsonSerializer()
        );

        services.AddScoped<IConnection>(provider => connection);
        return services.AddScoped<IPenzleClient, PenzleClient>();
    }


    /// <summary>
    ///     Registers penzle client from the specified assemblies.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="configuration">Pull the data dependencies from configuration.</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection AddPenzleClient(this IServiceCollection services, IConfigurationRoot configuration)
    {
        const string key = "PenzleApiConfig";

        if (configuration.GetSection(key).Exists() == false)
            throw new Exception(
                "The configuration in order to fetch data for penzle api configuration missing. Refereed key: PenzleApiConfig");

        var penzleServiceConfiguration = new PenzleServiceConfiguration();
        configuration.Bind(key, penzleServiceConfiguration);

        return services.AddPenzleClient(serviceConfiguration =>
        {
            serviceConfiguration.ApiDeliveryKey = penzleServiceConfiguration.ApiDeliveryKey;
            serviceConfiguration.ApiManagementKey = penzleServiceConfiguration.ApiManagementKey;
            serviceConfiguration.BaseUri = penzleServiceConfiguration.BaseUri;
            serviceConfiguration.Environment = penzleServiceConfiguration.Environment;
            serviceConfiguration.Project = penzleServiceConfiguration.Project;
            serviceConfiguration.TimeOut = penzleServiceConfiguration.TimeOut;
        });
    }
}