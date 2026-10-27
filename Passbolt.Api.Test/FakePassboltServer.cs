using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace Passbolt.Api.Test;

/// <summary>
/// A loopback Passbolt that logs a client in with the throwaway GPGAuth fixtures and then demands a second
/// factor until the request carries the <c>passbolt_mfa</c> cookie it has issued.
/// </summary>
internal sealed class FakePassboltServer : IAsyncDisposable
{
	public const string AcceptedCode = "123456";

	public const string RememberedValue = "remembered-device-1";

	private readonly HttpListener _listener = new();

	private readonly Task _loop;

	private FakePassboltServer(Uri address)
	{
		Address = address;
		_listener.Prefixes.Add(address.AbsoluteUri);
		_listener.Start();
		_loop = Task.Run(ServeAsync);
	}

	public Uri Address { get; }

	/// <summary>The body of every one-time code the server was asked to verify.</summary>
	public List<string> VerifyRequests { get; } = [];

	public static Task<FakePassboltServer> StartAsync()
	{
		var probe = new TcpListener(IPAddress.Loopback, 0);
		probe.Start();
		var port = ((IPEndPoint)probe.LocalEndpoint).Port;
		probe.Stop();
		return Task.FromResult(new FakePassboltServer(new Uri($"http://127.0.0.1:{port}/")));
	}

	private async Task ServeAsync()
	{
		while (_listener.IsListening)
		{
			HttpListenerContext context;
			try
			{
				context = await _listener.GetContextAsync();
			}
			catch (Exception ex) when (ex is HttpListenerException or ObjectDisposedException)
			{
				return;
			}

			await HandleAsync(context);
		}
	}

	private async Task HandleAsync(HttpListenerContext context)
	{
		var request = context.Request;
		var response = context.Response;
		var path = request.Url!.AbsolutePath;
		var body = request.HasEntityBody ? await new StreamReader(request.InputStream).ReadToEndAsync() : string.Empty;

		switch ((request.HttpMethod, path))
		{
			case ("POST", "/auth/login.json"):
				using (var document = JsonDocument.Parse(body))
				{
					if (!document.RootElement.GetProperty("gpg_auth").TryGetProperty("user_token_result", out _))
					{
						response.AddHeader("X-GPGAuth-User-Auth-Token", Uri.EscapeDataString(TestFixtures.Read("test-message.asc")));
						await WriteAsync(response, 200, """{"header":{"status":"success"}}""");
						break;
					}
				}

				response.AppendCookie(new Cookie("passbolt_session", "session-1", "/"));
				response.AppendCookie(new Cookie("csrfToken", "csrf-1", "/"));
				await WriteAsync(response, 200, """{"header":{"status":"success"},"body":{}}""");
				break;

			case ("GET", "/users/me.json"):
				if (request.Cookies["passbolt_mfa"]?.Value == RememberedValue)
				{
					await WriteAsync(response, 200, """{"header":{"status":"success"},"body":{"id":"user-1"}}""");
				}
				else
				{
					await WriteAsync(response, 403, """{"header":{"status":"error","code":403},"body":{"providers":{"totp":"/mfa/verify/totp.json"}}}""");
				}

				break;

			case ("POST", "/mfa/verify/totp.json"):
				VerifyRequests.Add(body);
				using (var document = JsonDocument.Parse(body))
				{
					if (document.RootElement.GetProperty("totp").GetString() != AcceptedCode)
					{
						await WriteAsync(response, 400, """{"header":{"status":"error"}}""");
						break;
					}
				}

				response.AppendCookie(new Cookie("passbolt_mfa", RememberedValue, "/"));
				await WriteAsync(response, 200, """{"header":{"status":"success"}}""");
				break;

			default:
				await WriteAsync(response, 404, "{}");
				break;
		}
	}

	private static async Task WriteAsync(HttpListenerResponse response, int status, string json)
	{
		var bytes = Encoding.UTF8.GetBytes(json);
		response.StatusCode = status;
		response.ContentType = "application/json";
		response.ContentLength64 = bytes.Length;
		await response.OutputStream.WriteAsync(bytes);
		response.Close();
	}

	public async ValueTask DisposeAsync()
	{
		_listener.Close();
		await _loop;
	}
}