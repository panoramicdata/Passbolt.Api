using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Microsoft.DependencyInjection;
using Xunit.Microsoft.DependencyInjection.Abstracts;

namespace Passbolt.Api.Test;

/// <summary>
/// Fixture class for integration tests that sets up the test environment and configures services.
/// </summary>
public sealed class Fixture : TestBedFixture
{
	private IConfigurationRoot? _configuration;

    /// <summary>
    /// Adds services to the test bed, including configuration for Passbolt integration settings and a cancellation token source.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <exception cref="InvalidOperationException"></exception>
	protected override void AddServices(IServiceCollection services, IConfiguration? configuration)
	{
		if (_configuration is null)
		{
			throw new InvalidOperationException("Configuration is null");
		}

		services
			.AddScoped<CancellationTokenSource>()
			.Configure<PassboltIntegrationSettings>(_configuration.GetSection("Passbolt"))
			.PostConfigure<PassboltIntegrationSettings>(ApplyConfigurationOverrides);
	}

	private void ApplyConfigurationOverrides(PassboltIntegrationSettings settings)
	{
		settings.ServerUrl = ApplyOverride(settings.ServerUrl, "PASSBOLT__SERVERURL", "ServerUrl");
		settings.ApiVersion = ApplyOverride(settings.ApiVersion, "PASSBOLT__APIVERSION", "ApiVersion");
		settings.AccessToken = ApplyOverride(settings.AccessToken, "PASSBOLT__ACCESSTOKEN", "AccessToken");
		settings.Username = ApplyOverride(settings.Username, "PASSBOLT__USERNAME", "Username");
		settings.Password = ApplyOverride(settings.Password, "PASSBOLT__PASSWORD", "Password");
		settings.PrivateKeyBlock = ApplyOverride(settings.PrivateKeyBlock, "PASSBOLT__PRIVATEKEYBLOCK", "PrivateKeyBlock");
		settings.KnownUserId = ApplyOverride(settings.KnownUserId, "PASSBOLT__KNOWNUSERID", "KnownUserId");
		settings.KnownGroupId = ApplyOverride(settings.KnownGroupId, "PASSBOLT__KNOWNGROUPID", "KnownGroupId");
		settings.KnownResourceId = ApplyOverride(settings.KnownResourceId, "PASSBOLT__KNOWNRESOURCEID", "KnownResourceId");
		settings.ResourceLookupName = ApplyOverride(settings.ResourceLookupName, "PASSBOLT__RESOURCELOOKUPNAME", "ResourceLookupName");
		settings.ResourceLookupUri = ApplyOverride(settings.ResourceLookupUri, "PASSBOLT__RESOURCELOOKUPURI", "ResourceLookupUri");
		settings.ResourceLookupUsername = ApplyOverride(settings.ResourceLookupUsername, "PASSBOLT__RESOURCELOOKUPUSERNAME", "ResourceLookupUsername");

		if (bool.TryParse(GetConfiguredValue("PASSBOLT__RUNMUTATINGINTEGRATIONTESTS", "RunMutatingIntegrationTests"), out var runMutating))
		{
			settings.RunMutatingIntegrationTests = runMutating;
		}
	}

	private string? ApplyOverride(string? currentValue, string envKey, string logicalKey)
		=> GetConfiguredValue(envKey, logicalKey) ?? currentValue;

    /// <inheritdoc />
	protected override ValueTask DisposeAsyncCore() => default;

    /// <inheritdoc />
	protected override IEnumerable<TestAppSettings> GetTestAppSettings()
	{
		_configuration = new ConfigurationBuilder()
			.AddUserSecrets<Fixture>(optional: true)
			.AddEnvironmentVariables()
			.Build();

		return
		[
			new TestAppSettings
			{
				IsOptional = true,
				Filename = null,
			}
		];
	}

	private string? GetConfiguredValue(string envKey, string logicalKey)
		=> _configuration?[envKey] ?? _configuration?[logicalKey];
}
