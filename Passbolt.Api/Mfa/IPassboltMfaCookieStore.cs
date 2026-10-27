namespace Passbolt.Api.Mfa;

/// <summary>
/// Keeps the Passbolt "remembered device" cookie (<c>passbolt_mfa</c>) between runs, so a one-time
/// code is only needed once per device rather than once per login. Implementations are expected to
/// protect the value at rest: it is a bearer credential for the second factor.
/// </summary>
public interface IPassboltMfaCookieStore
{
	/// <summary>
	/// Returns the remembered cookie value, or null when none is stored or it has expired.
	/// </summary>
	/// <param name="cancellationToken">Cancellation token.</param>
	Task<string?> LoadAsync(CancellationToken cancellationToken);

	/// <summary>
	/// Stores the cookie value the server issued when a one-time code was verified with remember-me.
	/// </summary>
	/// <param name="cookieValue">The <c>passbolt_mfa</c> cookie value.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	Task SaveAsync(string cookieValue, CancellationToken cancellationToken);

	/// <summary>
	/// Forgets the stored cookie, for example after the server refused it.
	/// </summary>
	/// <param name="cancellationToken">Cancellation token.</param>
	Task ClearAsync(CancellationToken cancellationToken);
}