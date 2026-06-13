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
BaseUrl = new Uri("https://passbolt.example.com"),
ApiVersion = "v2"
};

using var client = new PassboltClient(options);

var status = await client.GetServerStatusAsync();
Console.WriteLine($"Passbolt status: {status.Status}");
```

## Features

- Typed API client abstraction via `IPassboltClient`
- `System.Text.Json` serialization
- Refit-backed API interface for HTTP endpoint mapping
- DI-friendly constructor design
- xUnit v3 + AwesomeAssertions test coverage

## Quality

- Built with `TreatWarningsAsErrors`
- Nullable reference types enabled
- XML documentation generated
- CI validates restore, build, test, and package output

## Known Issues

- This initial version ships a focused status endpoint only; additional domain endpoints will be added iteratively.

## Links

- NuGet: https://www.nuget.org/packages/Passbolt.Api
- GitHub: https://github.com/panoramicdata/Passbolt.Api
- Issues: https://github.com/panoramicdata/Passbolt.Api/issues

## License

MIT - see [LICENSE](LICENSE).
