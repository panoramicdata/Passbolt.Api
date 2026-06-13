using Microsoft.Extensions.Logging;

namespace Passbolt.Api.Test;

/// <summary>
/// Writes ILogger messages to xUnit test output.
/// </summary>
/// <typeparam name="TCategoryName">Logger category type.</typeparam>
internal sealed class TestOutputHelperLogger<TCategoryName>(ITestOutputHelper output, LogLevel minimumLevel = LogLevel.Debug) : ILogger<TCategoryName>
{
	private readonly ITestOutputHelper _output = output;
	private readonly LogLevel _minimumLevel = minimumLevel;

	public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

	public bool IsEnabled(LogLevel logLevel) => logLevel >= _minimumLevel;

	public void Log<TState>(
		LogLevel logLevel,
		EventId eventId,
		TState state,
		Exception? exception,
		Func<TState, Exception?, string> formatter)
	{
		if (!IsEnabled(logLevel))
		{
			return;
		}

		var message = formatter(state, exception);
		if (string.IsNullOrWhiteSpace(message) && exception is null)
		{
			return;
		}

		var category = typeof(TCategoryName).FullName ?? typeof(TCategoryName).Name;

		try
		{
			_output.WriteLine($"[{DateTimeOffset.UtcNow:O}] {logLevel} {category}: {message}");
			if (exception is not null)
			{
				_output.WriteLine(exception.ToString());
			}
		}
		catch (InvalidOperationException)
		{
			// xUnit can close output after test completion; ignore late log writes.
		}
	}
}
