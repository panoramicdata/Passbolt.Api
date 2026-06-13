namespace Passbolt.Api;

/// <summary>
/// Represents a client for interacting with the Passbolt API.
/// </summary>
public sealed class PassboltClient : IDisposable
{
	private readonly HttpClient _httpClient;
	private readonly bool _ownsHttpClient;
	private readonly AuthenticatedLoggingHttpHandler? _httpClientHandler;
	/// <summary>
	/// Creates a client with internally managed <see cref="HttpClient"/>.
	/// </summary>
	/// <param name="options">Passbolt client options.</param>
	public PassboltClient(PassboltClientOptions options)
	{
		if (options is null)
		{
			throw new ArgumentNullException(nameof(options));
		}

		_ownsHttpClient = options.HttpClient is null;

		if (_ownsHttpClient)
		{
			_httpClientHandler = new AuthenticatedLoggingHttpHandler(options);
			_httpClient = new HttpClient(_httpClientHandler)
			{
				BaseAddress = options.Uri,
				Timeout = options.Timeout
			};
		}
		else
		{
			_httpClient = options.HttpClient!;
		}

		Status = RestService.For<IPassboltStatusApi>(_httpClient);
		Users = RestService.For<IPassboltUsersApi>(_httpClient);
		Groups = RestService.For<IPassboltGroupsApi>(_httpClient);
		Resources = RestService.For<IPassboltResourcesApi>(_httpClient);
		Folders = RestService.For<IPassboltFoldersApi>(_httpClient);
	}

	/// <summary>
	/// Gets the API for accessing Passbolt server status and healthcheck endpoints.
	/// </summary>
	public IPassboltStatusApi Status { get; }

	/// <summary>
	/// Gets the API for accessing Passbolt users, groups, resources, and folders endpoints.
	/// </summary>
	public IPassboltUsersApi Users { get; }

	/// <summary>
	/// Gets the API for accessing Passbolt groups endpoints.
	/// </summary>
	public IPassboltGroupsApi Groups { get; }

	/// <summary>
	/// Gets the API for accessing Passbolt resources endpoints.
	/// </summary>
	public IPassboltResourcesApi Resources { get; }

	/// <summary>
	/// Gets the API for accessing Passbolt folders endpoints.
	/// </summary>
	public IPassboltFoldersApi Folders { get; }

	/// <inheritdoc />
	public void Dispose()
	{
		if (_ownsHttpClient)
		{
			_httpClient.Dispose();
			_httpClientHandler?.Dispose();
		}
	}
}
