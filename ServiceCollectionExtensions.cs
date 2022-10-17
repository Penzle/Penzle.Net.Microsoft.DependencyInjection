using System;
using System.Diagnostics;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Penzle.Net.Http;
using Penzle.Net.Http.Internal;
using Penzle.Net.Models;

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
    public static IServiceCollection AddPenzleClient(this IServiceCollection services, Action<PenzleServiceConfiguration> configurator)
    {
        if (configurator == null)
        {
            throw new ArgumentNullException(paramName: nameof(configurator), message: "The configurator cannot be null.");
        }

        var penzleServiceConfiguration = new PenzleServiceConfiguration();
        configurator(obj: penzleServiceConfiguration);

        if (string.IsNullOrWhiteSpace(value: penzleServiceConfiguration.ApiDeliveryKey) && string.IsNullOrWhiteSpace(value: penzleServiceConfiguration.ApiManagementKey))
        {
            throw new ArgumentNullException(paramName: nameof(configurator), message: "At least one key needs to be presented ApiDeliveryKey or ApiManagementKey.");
        }

        if (penzleServiceConfiguration.BaseUri == null)
        {
            throw new ArgumentNullException(paramName: nameof(penzleServiceConfiguration.BaseUri), message: "It is necessary to specify the base url of the API.");
        }

        if (string.IsNullOrWhiteSpace(value: penzleServiceConfiguration.Environment))
        {
            throw new ArgumentNullException(paramName: nameof(penzleServiceConfiguration.Environment), message: "It is necessary to specify the target environment.");
        }

        if (string.IsNullOrWhiteSpace(value: penzleServiceConfiguration.Project))
        {
            throw new ArgumentNullException(paramName: nameof(penzleServiceConfiguration.Project), message: "It is necessary to specify the target project.");
        }

        if (penzleServiceConfiguration.TimeOut == default)
        {
            penzleServiceConfiguration.TimeOut = TimeSpan.FromSeconds(value: 30);
            Debug.Write(message: "The default settings for request time out has been set. Default value is 10 seconds. In order to override this value please pass value to penzleServiceConfiguration.");
        }

        var connection = new Connection
        (
            baseAddress: penzleServiceConfiguration.BaseUri,
            apiOptions: new ApiOptions(project: penzleServiceConfiguration.Project, environment: penzleServiceConfiguration.Environment),
            credentialStore: new InMemoryCredentialStore(credentials: new BearerCredentials(apiDeliveryKey: penzleServiceConfiguration.ApiDeliveryKey, apiManagementKey: penzleServiceConfiguration.ApiManagementKey)),
            httpClient: new HttpClientAdapter(getHandler: () => new HttpClientHandler()),
            serializer: new MicrosoftJsonSerializer()
        );

        services.AddScoped<IConnection>(implementationFactory: provider => connection);
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

        if (configuration.GetSection(key: key).Exists() == false)
        {
            throw new Exception(message: "The configuration in order to fetch data for penzle api configuration missing. Refereed key: PenzleApiConfig");
        }

        var penzleServiceConfiguration = new PenzleServiceConfiguration();
        configuration.Bind(key: key, instance: penzleServiceConfiguration);

        return services.AddPenzleClient(configurator: serviceConfiguration =>
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