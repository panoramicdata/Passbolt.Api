namespace Passbolt.Cli.Infrastructure;

/// <summary>The connect, fetch, render-as-JSON-or-table flow shared by the read-only commands.</summary>
public static class CommandRunner
{
	private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(30);

	/// <summary>
	/// Fetches a value with a connected client and writes it as JSON (or the result of
	/// <paramref name="jsonShape"/>) in JSON mode, otherwise renders it with <paramref name="renderTable"/>.
	/// </summary>
	public static async Task<int> RunAsync<T>(
		ConnectionSettings settings,
		CancellationToken cancellationToken,
		Func<PassboltClient, CancellationToken, Task<T>> fetch,
		Action<T> renderTable,
		Func<T, object>? jsonShape = null)
	{
		ArgumentNullException.ThrowIfNull(settings);
		ArgumentNullException.ThrowIfNull(fetch);
		ArgumentNullException.ThrowIfNull(renderTable);

		using var client = await ClientFactory.CreateAsync(settings, cancellationToken);
		using var cts = CommandCancellation.WithTimeout(cancellationToken, Timeout);

		var value = await fetch(client, cts.Token);

		if (!settings.Json)
		{
			renderTable(value);
		}
		else if (jsonShape is null)
		{
			Output.Json(value);
		}
		else
		{
			Output.Json(jsonShape(value));
		}

		return 0;
	}
}
