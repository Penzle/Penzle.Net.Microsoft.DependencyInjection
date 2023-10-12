# Penzle.Net.Microsoft.DependencyInjection

![Build and Test](https://github.com/Penzle/Penzle.Net.Microsoft.DependencyInjection/actions/workflows/build-ci.yml/badge.svg)
![Licence](https://camo.githubusercontent.com/238290f8deb751619ca04ad3d316f1246a498b13d2ab49c0348e2b4311bd08f4/68747470733a2f2f696d672e736869656c64732e696f2f6769746875622f6c6963656e73652f6a6f6e6772616365636f782f616e7962616467652e737667)
![W3C](https://img.shields.io/badge/w3c-validated-brightgreen)
![Paradigm](https://img.shields.io/badge/accessibility-yes-brightgreen)

## **Getting started**

Installation of the penzle package with support for dependency injection using the Visual Studio Package Manager
Console:

```
Install-Package Penzle.Net.Microsoft.DependencyInjection
```

Installation using .NET CLI:

```
dotnet add <your project> package Penzle.Net.Microsoft.DependencyInjection
```
## Usage

You can access data from the Penzle APIs by employing the IDeliveryPenzleClient, which offers functions for fetching information from the Penzle Delivery API. For tasks involving entries creation, updating, and deletion, you'll utilize the IManagementPenzleClient.

### Reference the Penzle Dependency Injection Namespace

In your project files (e.g., Startup.cs or Program.cs), add a reference to the Penzle.Net.Microsoft.DependencyInjection namespace:

```csharp
using Penzle.Net.Microsoft.DependencyInjection;
```

### Configure the Service Collection
Set up the Penzle client by calling service.AddPenzleClient and providing it with your configuration:

```csharp
using Microsoft.Extensions.Configuration;
using Penzle.Net.Microsoft.DependencyInjection;

// Retrieve your configuration, typically from an appsettings.json file
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .Build();

// Configure the Penzle client with your PenzleApiConfig
service.AddPenzleClient((IConfigurationRoot)configuration);
```

### Configuration in appsettings.json

To use the Penzle.Net.Microsoft.DependencyInjection package, it's essential to have the necessary Penzle API configuration properties in your `appsettings.json` file. These properties are used to configure the Penzle client. Here's an example configuration structure:

```json
{
  "PenzleApiConfig": {
    "ApiManagementKey": "YourApiManagementKey",
    "ApiDeliveryKey": "YourApiDeliveryKey",
    "BaseUri": "https://api.penzle.com",
    "Environment": "YourEnvironment",
    "Project": "YourProject"
  }
}
```
### Example of usage IDeliveryPenzleClient and IManagementPenzleClient
```csharp
public class PenzleDataManagementRepository : IPenzleDataManagementRepository
{
    private readonly IDeliveryPenzleClient _deliveryPenzleClient;
    private readonly IManagementPenzleClient _managementPenzleClient;

    public PenzleDataManagementRepository(IManagementPenzleClient managementPenzleClient,
        IDeliveryPenzleClient deliveryPenzleClient)
    {
        _managementPenzleClient = managementPenzleClient;
        _deliveryPenzleClient = deliveryPenzleClient;
    }

    public async Task<TResponse> GetEntry<TResponse>(Guid entityId, CancellationToken cancellationToken) where TResponse : new()
    {
        return await _deliveryPenzleClient.Entry.GetEntry<TResponse>(entityId, cancellationToken: cancellationToken);
    }

    public async Task<Guid> SaveEntry<TInput>(TInput fields, string name, Guid? parentId = null, Guid? id = null, CancellationToken cancellationToken = default) where TInput : new()
    {
        var entry = new CreateEntryRequest<TInput>
        {
            Fields = fields,
            Name = name,
            Template = GetTemplateNameFromType(typeof(TInput).Name),
            ParentId = parentId,
            Id = id
        };
        return await _managementPenzleClient.Entry.CreateEntry(entry, cancellationToken);
    }

    public async Task<bool> UpdateEntry<TInput>(Guid entityId, TInput fields, string name, CancellationToken cancellationToken)
        where TInput : new()
    {
        var updateEntry = new UpdateEntryRequest<TInput>
        {
            Fields = fields,
            Name = name,
            Template = GetTemplateNameFromType(typeof(TInput).Name),
            Id = entityId
        };

        var response = await _managementPenzleClient.Entry.UpdateEntry(entityId, updateEntry,
            cancellationToken);

        return response == HttpStatusCode.NoContent;
    }

    public async Task<bool> DeleteEntry(Guid entryId, CancellationToken cancellationToken)
    {
        var response = await _managementPenzleClient.Entry.DeleteEntry(entryId, cancellationToken);

        return response == HttpStatusCode.NoContent;
    }

    public async Task<IReadOnlyList<TResponse>> GetEntriesByQuery<TResponse>(QueryEntryBuilder<TResponse> query, CancellationToken cancellationToken)
        where TResponse : new()
    {
        return await _deliveryPenzleClient.Entry.GetEntries(query, cancellationToken: cancellationToken);
    }
}
```

## **Contributing to Penzle.Net.Microsoft.DependencyInjection**

Your thoughts and ideas are much appreciated. If you're interested in helping out with this project in any way, we'd
like to make it as clear and straightforward as possible for you to do so, whether that's by:

- Bug reporting
- Addressing the present codebase
- Offering a patch
- Advancing ideas for brand new capabilities
- Taking on the role of a maintainer

Github is where we host our code, manage bug reports and feature requests, and incorporate changes suggested by our
users.
Report bugs using Github's issues. We host our code on Github, which is also where we manage user bug reports and
feature requests and incorporate modifications made by users. In general, high-quality bug reports consist of the
following components: background information; reproducible steps; an example of the code, if the reporter possesses such
an example.

## **License**

MIT License

Copyright (c) 2022 Penzle LLC

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
