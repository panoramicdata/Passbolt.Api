namespace Passbolt.Cli.Infrastructure;

/// <summary>The connect, fetch, render-as-JSON-or-table flow shared by the read-only commands.</summary>
public static class CommandRunner
{
	private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(30);

	/// <summary>
	/// Fetches a value with a connected client and writes it as JSON in JSON mode,
	/// otherwise renders it with <paramref name="renderTable"/>.
	/// </summary>
	public static Task<int> RunAsync<T>(
		ConnectionSettings settings,
		CancellationToken cancellationToken,
		Func<PassboltClient, CancellationToken, Task<T>> fetch,
		Action<T> renderTable)
		=> RunCoreAsync(settings, cancellationToken, fetch, renderTable, null);

	/// <summary>
	/// As the overload without a shape, but writes the result of <paramref name="jsonShape"/> in JSON mode.
	/// </summary>
	public static Task<int> RunAsync<T>(
		ConnectionSettings settings,
		CancellationToken cancellationToken,
		Func<PassboltClient, CancellationToken, Task<T>> fetch,
		Action<T> renderTable,
		Func<T, object> jsonShape)
	{
		ArgumentNullException.ThrowIfNull(jsonShape);
		return RunCoreAsync(settings, cancellationToken, fetch, renderTable, jsonShape);
	}

	private static async Task<int> RunCoreAsync<T>(
		ConnectionSettings settings,
		CancellationToken cancellationToken,
		Func<PassboltClient, CancellationToken, Task<T>> fetch,
		Action<T> renderTable,
		Func<T, object>? jsonShape)
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
