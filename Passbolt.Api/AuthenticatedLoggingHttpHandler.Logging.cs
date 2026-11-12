
namespace Passbolt.Api;

/// <summary>Request and response logging.</summary>
internal sealed partial class AuthenticatedLoggingHttpHandler
{
	private async Task LogRequestAsync(string logPrefix, HttpRequestMessage request, CancellationToken cancellationToken)
	{
		_logger.Log(_levelToLogAt, "{LogPrefix}{Method} {Uri}", logPrefix, request.Method, request.RequestUri);
		_logger.Log(_levelToLogAt, "{LogPrefix}Request Headers:\r\n{Headers}", logPrefix, FormatHeaders(request.Headers));

		if (request.Content is null)
		{
			return;
		}

		_logger.Log(_levelToLogAt, "{LogPrefix}Request Content Headers:\r\n{Headers}", logPrefix, FormatHeaders(request.Content.Headers));
		var requestContent = await request.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
		_logger.Log(_levelToLogAt, "{LogPrefix}Request Body:\r\n{RequestContent}", logPrefix, requestContent);
	}

	private async Task LogResponseAsync(string logPrefix, HttpResponseMessage response, CancellationToken cancellationToken)
	{
		_logger.Log(_levelToLogAt, "{LogPrefix}Response {StatusCode}", logPrefix, (int)response.StatusCode);
		_logger.Log(_levelToLogAt, "{LogPrefix}Response Headers:\r\n{Headers}", logPrefix, FormatHeaders(response.Headers));

		if (response.Content is null)
		{
			return;
		}

		_logger.Log(_levelToLogAt, "{LogPrefix}Response Content Headers:\r\n{Headers}", logPrefix, FormatHeaders(response.Content.Headers));
		var responseContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
		_logger.Log(_levelToLogAt, "{LogPrefix}Response Body:\r\n{ResponseContent}", logPrefix, responseContent);
	}

	private static string FormatHeaders(HttpHeaders headers)
	{
		var builder = new StringBuilder();

		foreach (var header in headers)
		{
			var value = string.Equals(header.Key, "Authorization", StringComparison.OrdinalIgnoreCase)
				? "***MASKED***"
				: string.Join(", ", header.Value);

			builder.AppendLine($"  {header.Key}: {value}");
		}

		return builder.ToString().TrimEnd();
	}
}
