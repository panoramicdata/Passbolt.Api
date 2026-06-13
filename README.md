# Passbolt.Api

[![NuGet](https://img.shields.io/nuget/v/Passbolt.Api.svg)](https://www.nuget.org/packages/Passbolt.Api)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET 10](https://img.shields.io/badge/.NET-10.0-blue.svg)](https://dotnet.microsoft.com/download)
[![Codacy Badge](https://app.codacy.com/project/badge/Grade/86cd75f6039248d68769cd6e63784397)](https://app.codacy.com/gh/panoramicdata/Passbolt.Api/dashboard?utm_source=gh&utm_medium=referral&utm_content=&utm_campaign=Badge_grade)

Typed .NET client primitives for integrating with Passbolt APIs in a safe, testable, and DI-friendly way.

## Installation

```shell
dotnet add package Passbolt.Api
```

## Quick Start

```csharp
using Passbolt.Api;

var options = new PassboltClientOptions
{
    Uri = new Uri("https://passbolt.example.com"),
    Username = "user@example.com",
    Password = "password",
    PrivateKeyBlock = "-----BEGIN PGP PRIVATE KEY BLOCK-----\n..."
};

using var client = new PassboltClient(options);

// Get server status
var status = await client.Status.GetStatusAsync();
Console.WriteLine($"Passbolt version: {status.Body?.Version}");

// List all resources
var resources = await client.Resources.GetResourcesAsync();
foreach (var resource in resources.Body ?? [])
{
    Console.WriteLine($"Resource: {resource.Name} (ID: {resource.Id})");
}
```

## Features

- Strongly-typed `PassboltClient` for intuitive API access
- Refit-backed REST API interface for seamless HTTP endpoint mapping
- Built-in support for Passbolt's PGP authentication flow
- Async/await support throughout
- Dependency injection compatible design

## Quality

- Built with `TreatWarningsAsErrors`
- Nullable reference types enabled
- XML documentation generated
- CI validates restore, build, test, and package output

## Supported Endpoints

- **Status**: Server status and healthcheck information
- **Users**: Create, read, update, and list users
- **Groups**: Create, read, update, and list groups  
- **Resources**: Create, read, update, and list password resources
- **Folders**: Create, read, update, and list folders

## Links

- NuGet: https://www.nuget.org/packages/Passbolt.Api
- GitHub: https://github.com/panoramicdata/Passbolt.Api
- Issues: https://github.com/panoramicdata/Passbolt.Api/issues

## License

MIT - see [LICENSE](LICENSE).
