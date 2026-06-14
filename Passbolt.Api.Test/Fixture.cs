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
		settings.ServerUrl = GetConfiguredValue("PASSBOLT__SERVERURL", "ServerUrl") ?? settings.ServerUrl;
		settings.ApiVersion = GetConfiguredValue("PASSBOLT__APIVERSION", "ApiVersion") ?? settings.ApiVersion;
		settings.AccessToken = GetConfiguredValue("PASSBOLT__ACCESSTOKEN", "AccessToken") ?? settings.AccessToken;
		settings.Username = GetConfiguredValue("PASSBOLT__USERNAME", "Username") ?? settings.Username;
		settings.Password = GetConfiguredValue("PASSBOLT__PASSWORD", "Password") ?? settings.Password;
		settings.PrivateKeyBlock = GetConfiguredValue("PASSBOLT__PRIVATEKEYBLOCK", "PrivateKeyBlock") ?? settings.PrivateKeyBlock;
		settings.KnownUserId = GetConfiguredValue("PASSBOLT__KNOWNUSERID", "KnownUserId") ?? settings.KnownUserId;
		settings.KnownGroupId = GetConfiguredValue("PASSBOLT__KNOWNGROUPID", "KnownGroupId") ?? settings.KnownGroupId;
		settings.KnownResourceId = GetConfiguredValue("PASSBOLT__KNOWNRESOURCEID", "KnownResourceId") ?? settings.KnownResourceId;
		settings.ResourceLookupName = GetConfiguredValue("PASSBOLT__RESOURCELOOKUPNAME", "ResourceLookupName") ?? settings.ResourceLookupName;
		settings.ResourceLookupUri = GetConfiguredValue("PASSBOLT__RESOURCELOOKUPURI", "ResourceLookupUri") ?? settings.ResourceLookupUri;
		settings.ResourceLookupUsername = GetConfiguredValue("PASSBOLT__RESOURCELOOKUPUSERNAME", "ResourceLookupUsername") ?? settings.ResourceLookupUsername;
		if (bool.TryParse(GetConfiguredValue("PASSBOLT__RUNMUTATINGINTEGRATIONTESTS", "RunMutatingIntegrationTests"), out var runMutating))
		{
			settings.RunMutatingIntegrationTests = runMutating;
		}
	}

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
