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

		Avatars = RestService.For<IPassboltAvatarsApi>(_httpClient);
		Comments = RestService.For<IPassboltCommentsApi>(_httpClient);
		Folders = RestService.For<IPassboltFoldersApi>(_httpClient);
		Groups = RestService.For<IPassboltGroupsApi>(_httpClient);
		Me = RestService.For<IPassboltMeApi>(_httpClient);
		Permissions = RestService.For<IPassboltPermissionsApi>(_httpClient);
		Resources = RestService.For<IPassboltResourcesApi>(_httpClient);
		Roles = RestService.For<IPassboltRolesApi>(_httpClient);
		Status = RestService.For<IPassboltStatusApi>(_httpClient);
		Users = RestService.For<IPassboltUsersApi>(_httpClient);
	}

	/// <summary>
	/// Gets the API for accessing Passbolt server status and healthcheck endpoints.
	/// </summary>
	public IPassboltStatusApi Status { get; }

	/// <summary>
	/// Gets the API for accessing Passbolt users endpoints.
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

	/// <summary>
	/// Gets the API for accessing Passbolt comments endpoints.
	/// </summary>
	public IPassboltCommentsApi Comments { get; }

	/// <summary>
	/// Gets the API for accessing Passbolt permissions endpoints.
	/// </summary>
	public IPassboltPermissionsApi Permissions { get; }

	/// <summary>
	/// Gets the API for accessing the current authenticated user's profile.
	/// </summary>
	public IPassboltMeApi Me { get; }

	/// <summary>
	/// Gets the API for accessing Passbolt avatars endpoints.
	/// </summary>
	public IPassboltAvatarsApi Avatars { get; }

	/// <summary>
	/// Gets the API for accessing Passbolt roles endpoints.
	/// </summary>
	public IPassboltRolesApi Roles { get; }

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
