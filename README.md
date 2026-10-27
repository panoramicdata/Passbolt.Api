# Passbolt.Api

[![NuGet](https://img.shields.io/nuget/v/Passbolt.Api.svg)](https://www.nuget.org/packages/Passbolt.Api)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET 10](https://img.shields.io/badge/.NET-10.0-blue.svg)](https://dotnet.microsoft.com/download)
[![Codacy Badge](https://app.codacy.com/project/badge/Grade/86cd75f6039248d68769cd6e63784397)](https://app.codacy.com/gh/panoramicdata/Passbolt.Api/dashboard?utm_source=gh&utm_medium=referral&utm_content=&utm_campaign=Badge_grade)

Typed .NET client primitives for integrating with the Passbolt API in a safe, testable, and DI-friendly way.
Targets the **Passbolt REST API v2** (the JSON endpoints, e.g. `/resources.json`).

> [!IMPORTANT]
> **Community-maintained project.** `Passbolt.Api` and `Passbolt.Cli` are developed and published
> independently by [Panoramic Data Limited](https://github.com/panoramicdata). They are **not affiliated
> with, endorsed by, or sponsored by Passbolt S.A.**
>
> Passbolt and the Passbolt logo are registered trademarks of Passbolt S.A. The name is used here only to
> describe compatibility with the Passbolt API.
>
> Maintenance, releases and support for this client come from this repository — not from Passbolt S.A.
> Report issues with *this client* [here](https://github.com/panoramicdata/Passbolt.Api/issues), and see
> [SECURITY.md](SECURITY.md) for vulnerability reporting. Vulnerabilities in **Passbolt itself** should go to
> [Passbolt S.A.](https://www.passbolt.com/security).

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

using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
using var client = new PassboltClient(options);

// Get server status
var status = await client.Status.GetStatusAsync(cts.Token);
Console.WriteLine($"Passbolt status: {status.Status}");

// List all resources
var resources = await client.Resources.GetAllAsync(cts.Token);
foreach (var resource in resources.Value)
{
    Console.WriteLine($"Resource: {resource.Name} (ID: {resource.Id})");
}
```

> API responses are wrapped in a `Response<T>` envelope — the payload is on `.Value`, with metadata on `.Header`.

## Command-line tool (`Passbolt.Cli`)

The same client also ships as a cross-platform command-line tool, installable as a .NET global tool:

```shell
dotnet tool install --global Passbolt.Cli
```

This installs a `passbolt` command. Configure it once (the passphrase is never stored):

```shell
passbolt configure --server https://passbolt.example.com --username user@example.com --private-key-file ~/.passbolt/private.asc
```

Connection settings resolve in the order **option → environment variable → config file**, with the private key falling back to `~/.passbolt/private.asc`. Environment variables: `PASSBOLT_SERVER`, `PASSBOLT_USERNAME`, `PASSBOLT_PASSWORD`, `PASSBOLT_PRIVATE_KEY_FILE`.

```shell
# Server + identity
passbolt status
passbolt whoami

# Browse (add --json to any command for machine-readable output)
passbolt resource list
passbolt resource get <id>
passbolt user list
passbolt group get <id>
passbolt folder list
passbolt role list
passbolt permission list --resource <id>

# Manage (non-secret writes)
passbolt user create --email new@example.com --first-name Ada --last-name Lovelace
passbolt user update <id> --last-name Byron
passbolt group create --name "Engineering" --manager <user-id>
passbolt group add-member <group-id> --user <user-id> --admin
passbolt folder create --name "Shared" --parent <folder-id>
passbolt resource update <id> --name "New name" --uri https://example.com
passbolt resource delete <id> --yes

# Governance: flag resources with fewer than two owners or no group owner
passbolt audit ownership
passbolt audit ownership --min-owners 2 --json
```

### Command coverage

| Area | Commands |
|------|----------|
| Server | `status`, `whoami`, `configure` |
| Resources | `list`, `get`, `update` (metadata), `delete` |
| Users | `list`, `get`, `create`, `update`, `delete` |
| Groups | `list`, `get`, `create`, `add-member`, `delete` |
| Folders | `list`, `get`, `create`, `update`, `delete` |
| Roles | `list` |
| Permissions | `list --resource\|--user` |
| Audit | `ownership` |

**Library only for now:** creating a resource, rotating a resource's secret and sharing (which re-encrypt the secret for each recipient) are in the library (`PassboltClient.CreateResourceAsync`, `RotateResourceSecretAsync`, `ShareResourceAsync`, issue #31) but have no CLI command yet.

Every command accepts `--server/-s`, `--username/-u`, `--password/-p`, `--private-key-file/-k`, `--config/-c` and `--json/-j`. In `--json` mode stdout carries only JSON (status text goes to stderr), so it pipes cleanly into `jq`.

### Second factor (MFA)

A server that requires a one-time code answers a signed-in request with a 403 that lists its providers. Give the client a `PassboltMfaOptions` and it handles that at sign-in:

```csharp
var options = new PassboltClientOptions
{
	Uri = new Uri("https://passbolt.example.com"),
	Username = "user@example.com",
	Password = passphrase,
	PrivateKeyBlock = privateKey,
	Mfa = new PassboltMfaOptions
	{
		CookieStore = myStore,                        // keeps the remembered-device cookie between runs
		TotpProvider = ct => ReadCodeAsync(ct)         // return null when no code can be given
	}
};
```

A stored cookie the server accepts needs no code. Otherwise `TotpProvider` is asked once, the code is verified with remember-me (set `RememberDevice` to false to opt out) and the new cookie is saved through `IPassboltMfaCookieStore`, which you implement so the value is protected at rest (it is a bearer credential for the second factor). With no code available the client fails with `PassboltMfaRequiredException` (carried as the inner exception of Refit's `ApiRequestException`).

## Features

### Library

- Strongly-typed `PassboltClient` for intuitive API access
- Refit-backed REST API interface for seamless HTTP endpoint mapping
- Built-in support for Passbolt's PGP authentication flow
- Async/await support throughout
- Dependency injection compatible design

### CLI (`Passbolt.Cli`)

- Installable as a .NET global tool (`passbolt`)
- Cobra-style subcommands with rich help
- Human-friendly tables **and** clean `--json` for scripting/`jq`
- Config file + environment variables + per-command options
- Ownership audit that flags under-owned resources — a governance view not offered by the reference Go CLI

## Quality

- Built with `TreatWarningsAsErrors`
- Nullable reference types enabled
- XML documentation generated
- CI validates restore, build, test, and package output

## Supported Endpoints

### Core Entities
- **Status**: Server status and healthcheck information
- **Users**: CRUD operations and listing for user management
- **Groups**: CRUD operations and listing for group management
- **Resources**: CRUD operations, listing, searching by name/URI, and resource sharing
- **Folders**: CRUD operations and listing for folder management

### Related Features
- **Comments**: Create, read, and delete comments on resources
- **Permissions**: Query and filter permissions by resource or user
- **Roles**: List available roles and retrieve role details
- **Avatars**: Retrieve user profile images
- **Me**: Access current authenticated user's profile information

## Links

- NuGet: https://www.nuget.org/packages/Passbolt.Api
- GitHub: https://github.com/panoramicdata/Passbolt.Api
- Issues: https://github.com/panoramicdata/Passbolt.Api/issues

## Acknowledgements & related projects

- [Passbolt](https://www.passbolt.com/) — the open-source password manager this client talks to. 💙
- [`passbolt/go-passbolt`](https://github.com/passbolt/go-passbolt) and [`passbolt/go-passbolt-cli`](https://github.com/passbolt/go-passbolt-cli) — the excellent, mature Go SDK and CLI. They're the reference implementation we learned from, and remain the go-to for Go users. `Passbolt.Api`/`Passbolt.Cli` exist to give the .NET ecosystem a first-class, idiomatic option — with a bit of friendly cross-language rivalry. 🤝

## Trademarks & disclaimer

> Passbolt and the Passbolt logo are registered trademarks of Passbolt S.A. This project is community
> maintained and is not affiliated with, endorsed by, or sponsored by Passbolt S.A.

The Passbolt name appears in this repository, in the package identifiers and in this documentation only
nominatively — to describe compatibility with the Passbolt API — consistent with Passbolt's
[Trademark Guidelines](https://www.passbolt.com/credits/trademark-guidelines). No official status is claimed
or implied, and no Passbolt S.A. branding, logo or visual identity is used.

## License

MIT - see [LICENSE](LICENSE).
