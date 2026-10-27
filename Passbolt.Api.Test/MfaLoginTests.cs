using Passbolt.Api.Mfa;

namespace Passbolt.Api.Test;

/// <summary>
/// A server that asks for a second factor (a failure reaches the caller as a Refit exception whose inner exception is the MFA one): the remembered-device cookie is reused, a one-time code is asked for
/// only when it must be, and a device is remembered only when the caller wants it.
/// </summary>
public sealed class MfaLoginTests
{
	private sealed class MemoryCookieStore(string? initial = null) : IPassboltMfaCookieStore
	{
		public string? Value { get; private set; } = initial;

		public int Clears { get; private set; }

		public Task<string?> LoadAsync(CancellationToken cancellationToken) => Task.FromResult(Value);

		public Task SaveAsync(string cookieValue, CancellationToken cancellationToken)
		{
			Value = cookieValue;
			return Task.CompletedTask;
		}

		public Task ClearAsync(CancellationToken cancellationToken)
		{
			Value = null;
			Clears++;
			return Task.CompletedTask;
		}
	}

	private static PassboltClient ClientFor(FakePassboltServer server, PassboltMfaOptions? mfa)
		=> new(new PassboltClientOptions
		{
			Uri = server.Address,
			Username = "tester@example.invalid",
			Password = string.Empty,
			PrivateKeyBlock = TestFixtures.Read("test-private.asc"),
			Mfa = mfa
		});

	private static Task SignInAsync(PassboltClient client) => client.Me.GetAsync(TestContext.Current.CancellationToken);

	/// <summary>A stored remembered-device cookie that the server accepts needs no code.</summary>
	[Fact]
	public async Task ARememberedDevice_IsReused_WithoutAskingForACode()
	{
		await using var server = await FakePassboltServer.StartAsync();
		var asked = 0;
		var store = new MemoryCookieStore(FakePassboltServer.RememberedValue);
		using var client = ClientFor(server, new PassboltMfaOptions
		{
			CookieStore = store,
			TotpProvider = _ => { asked++; return Task.FromResult<string?>(FakePassboltServer.AcceptedCode); }
		});

		await SignInAsync(client);

		asked.Should().Be(0);
		server.VerifyRequests.Should().BeEmpty();
		store.Clears.Should().Be(0);
	}

	/// <summary>With no stored cookie a code is verified, the server is asked to remember the device, and the cookie is stored.</summary>
	[Fact]
	public async Task ACodeIsVerified_AndTheDeviceRemembered_WhenNoCookieIsStored()
	{
		await using var server = await FakePassboltServer.StartAsync();
		var store = new MemoryCookieStore();
		using var client = ClientFor(server, new PassboltMfaOptions
		{
			CookieStore = store,
			TotpProvider = _ => Task.FromResult<string?>(" " + FakePassboltServer.AcceptedCode + " ")
		});

		await SignInAsync(client);

		server.VerifyRequests.Should().ContainSingle().Which.Should().Contain("\"remember\":1").And.Contain($"\"totp\":\"{FakePassboltServer.AcceptedCode}\"");
		store.Value.Should().Be(FakePassboltServer.RememberedValue);
	}

	/// <summary>A stored cookie the server refuses is cleared, then a code is asked for and the new cookie stored.</summary>
	[Fact]
	public async Task AStaleRememberedCookie_IsForgotten_AndACodeAsked()
	{
		await using var server = await FakePassboltServer.StartAsync();
		var store = new MemoryCookieStore("expired-device");
		using var client = ClientFor(server, new PassboltMfaOptions
		{
			CookieStore = store,
			TotpProvider = _ => Task.FromResult<string?>(FakePassboltServer.AcceptedCode)
		});

		await SignInAsync(client);

		store.Clears.Should().Be(1);
		store.Value.Should().Be(FakePassboltServer.RememberedValue);
		server.VerifyRequests.Should().ContainSingle();
	}

	/// <summary>When no code can be given the client fails with the MFA exception and never calls verify.</summary>
	[Fact]
	public async Task NoCodeAvailable_FailsWithTheMfaException_WithoutCallingVerify()
	{
		await using var server = await FakePassboltServer.StartAsync();
		using var client = ClientFor(server, new PassboltMfaOptions { CookieStore = new MemoryCookieStore(), TotpProvider = _ => Task.FromResult<string?>(null) });

		var act = () => SignInAsync(client);

		(await act.Should().ThrowAsync<Refit.ApiRequestException>()).Which.InnerException.Should().BeOfType<PassboltMfaRequiredException>().Which.Message.Should().Contain("one-time code");
		server.VerifyRequests.Should().BeEmpty();
	}

	/// <summary>A server that wants a second factor fails with the MFA exception when no MFA options were given.</summary>
	[Fact]
	public async Task NoMfaOptionsAtAll_FailsWithTheMfaException()
	{
		await using var server = await FakePassboltServer.StartAsync();
		using var client = ClientFor(server, new PassboltMfaOptions());

		var act = () => SignInAsync(client);

		(await act.Should().ThrowAsync<Refit.ApiRequestException>()).Which.InnerException.Should().BeOfType<PassboltMfaRequiredException>();
	}

	/// <summary>A code the server refuses fails with the MFA exception and stores nothing.</summary>
	[Fact]
	public async Task AWrongCode_IsRefused_AndNothingIsRemembered()
	{
		await using var server = await FakePassboltServer.StartAsync();
		var store = new MemoryCookieStore();
		using var client = ClientFor(server, new PassboltMfaOptions { CookieStore = store, TotpProvider = _ => Task.FromResult<string?>("000000") });

		var act = () => SignInAsync(client);

		(await act.Should().ThrowAsync<Refit.ApiRequestException>()).Which.InnerException.Should().BeOfType<PassboltMfaRequiredException>().Which.Message.Should().Contain("refused");
		store.Value.Should().BeNull();
	}

	/// <summary>With RememberDevice off the verify request says remember 0 and no cookie is stored.</summary>
	[Fact]
	public async Task RememberDeviceOff_AsksTheServerNotToRemember_AndStoresNothing()
	{
		await using var server = await FakePassboltServer.StartAsync();
		var store = new MemoryCookieStore();
		using var client = ClientFor(server, new PassboltMfaOptions
		{
			CookieStore = store,
			RememberDevice = false,
			TotpProvider = _ => Task.FromResult<string?>(FakePassboltServer.AcceptedCode)
		});

		await SignInAsync(client);

		server.VerifyRequests.Should().ContainSingle().Which.Should().Contain("\"remember\":0");
		store.Value.Should().BeNull();
	}
}