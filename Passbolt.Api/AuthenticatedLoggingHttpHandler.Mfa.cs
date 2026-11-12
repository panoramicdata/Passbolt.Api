using Passbolt.Api.Mfa;

namespace Passbolt.Api;

/// <summary>Second-factor (MFA) handling for the sign-in.</summary>
internal sealed partial class AuthenticatedLoggingHttpHandler
{
	// A signed-in request to /users/me.json seeds the CSRF cookie and shows whether the server wants a second factor.
	private async Task<bool> ProbeForSecondFactorAsync(CancellationToken cancellationToken)
	{
		var meUri = new Uri(_options.Uri, UsersMePath);
		using var request = new HttpRequestMessage(HttpMethod.Get, meUri);
		request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

		using var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
		if (response.StatusCode == HttpStatusCode.Forbidden
			&& PassboltMfaChallenge.OffersTotp(await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false)))
		{
			return true;
		}

		response.EnsureSuccessStatusCode();
		return false;
	}

	private async Task EnsureSecondFactorAsync(CancellationToken cancellationToken)
	{
		if (!await ProbeForSecondFactorAsync(cancellationToken).ConfigureAwait(false))
		{
			return;
		}

		var mfa = _options.Mfa;
		if (mfa?.CookieStore is { } store)
		{
			await store.ClearAsync(cancellationToken).ConfigureAwait(false);
		}

		var code = mfa?.TotpProvider is { } provider
			? await provider(cancellationToken).ConfigureAwait(false)
			: null;
		if (string.IsNullOrWhiteSpace(code))
		{
			throw new PassboltMfaRequiredException("Passbolt requires a one-time code for this device and none was supplied. Sign in once interactively with a TotpProvider and RememberDevice set to remember this device.");
		}

		await VerifyTotpAsync(code.Trim(), cancellationToken).ConfigureAwait(false);

		if (await ProbeForSecondFactorAsync(cancellationToken).ConfigureAwait(false))
		{
			throw new PassboltMfaRequiredException("Passbolt still requires a second factor after the one-time code was accepted.");
		}
	}

	private async Task VerifyTotpAsync(string code, CancellationToken cancellationToken)
	{
		var remember = _options.Mfa?.RememberDevice ?? true;

		using var request = BuildTotpVerifyRequest(code, remember);
		using var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
		if (!response.IsSuccessStatusCode)
		{
			throw new PassboltMfaRequiredException($"Passbolt refused the one-time code (HTTP {(int)response.StatusCode}).");
		}

		if (remember)
		{
			await RememberDeviceAsync(cancellationToken).ConfigureAwait(false);
		}
	}

	private HttpRequestMessage BuildTotpVerifyRequest(string code, bool remember)
	{
		var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_options.Uri, MfaVerifyTotpPath))
		{
			Content = JsonContent.Create(new MfaVerifyRequest { Totp = code, Remember = remember ? 1 : 0 }, options: JsonSerializerOptions)
		};
		request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
		if (GetCookie(new Uri(_options.Uri, LoginPath), "csrfToken")?.Value is { Length: > 0 } csrfToken)
		{
			request.Headers.TryAddWithoutValidation("X-CSRF-Token", csrfToken);
		}

		return request;
	}

	private async Task RememberDeviceAsync(CancellationToken cancellationToken)
	{
		if (_options.Mfa?.CookieStore is { } store
			&& GetCookie(new Uri(_options.Uri, LoginPath), MfaCookieName)?.Value is { Length: > 0 } rememberedValue)
		{
			await store.SaveAsync(rememberedValue, cancellationToken).ConfigureAwait(false);
		}
	}

	private async Task RestoreRememberedMfaCookieAsync(CancellationToken cancellationToken)
	{
		if (_options.Mfa?.CookieStore is not { } store
			|| await store.LoadAsync(cancellationToken).ConfigureAwait(false) is not { Length: > 0 } value)
		{
			return;
		}

		_cookieContainer.Add(_options.Uri, new Cookie(MfaCookieName, value, "/", _options.Uri.Host)
		{
			Secure = _options.Uri.Scheme == Uri.UriSchemeHttps
		});
	}

	private sealed class MfaVerifyRequest
	{
		[JsonPropertyName("totp")]
		public required string Totp { get; init; }

		[JsonPropertyName("remember")]
		public int Remember { get; init; }
	}
}
