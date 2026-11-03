using Passbolt.Api.Mfa;

namespace Passbolt.Api.Test;

/// <summary>
/// <see cref="PassboltClientOptions.IsReadOnly"/> refuses a request that would change the server before anything is sent.
/// </summary>
public sealed class ReadOnlyOptionTests
{
	private static PassboltClient ClientFor(Uri address, bool isReadOnly)
		=> new(new PassboltClientOptions
		{
			Uri = address,
			Username = "tester@example.invalid",
			Password = string.Empty,
			PrivateKeyBlock = TestFixtures.Read("test-private.asc"),
			IsReadOnly = isReadOnly,
			Mfa = new PassboltMfaOptions { CookieStore = new RememberedStore() }
		});

	private sealed class RememberedStore : IPassboltMfaCookieStore
	{
		public Task<string?> LoadAsync(CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			return Task.FromResult<string?>(FakePassboltServer.RememberedValue);
		}

		public Task SaveAsync(string cookieValue, CancellationToken cancellationToken)
		{
			ArgumentNullException.ThrowIfNull(cookieValue);
			cancellationToken.ThrowIfCancellationRequested();
			return Task.CompletedTask;
		}

		public Task ClearAsync(CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			return Task.CompletedTask;
		}
	}

	private static bool IsRefused(Exception e)
		=> e is InvalidOperationException { Message: var m } && m.Contains("read-only", StringComparison.Ordinal)
			|| e.InnerException is InvalidOperationException { Message: var n } && n.Contains("read-only", StringComparison.Ordinal);

	/// <summary>The option is off unless asked for.</summary>
	[Fact]
	public void IsReadOnly_DefaultsToFalse()
		=> new PassboltClientOptions
		{
			Uri = new Uri("https://example.com"),
			Username = "u",
			Password = "p",
			PrivateKeyBlock = "k"
		}.IsReadOnly.Should().BeFalse();

	/// <summary>A write is refused before any connection is made (nothing listens on the address).</summary>
	[Fact]
	public async Task ReadOnly_Delete_IsRefusedBeforeSending()
	{
		using var client = ClientFor(new Uri("http://127.0.0.1:1/"), isReadOnly: true);

		var act = () => client.Comments.CreateAsync(new CreateCommentRequest { ResourceId = "resource-1", Content = "c" }, TestContext.Current.CancellationToken);

		var thrown = await act.Should().ThrowAsync<Exception>();
		IsRefused(thrown.Which).Should().BeTrue();
	}

	/// <summary>A write whose Refit signature returns an ApiResponse reports the refusal in Error rather than throwing.</summary>
	[Fact]
	public async Task ReadOnly_DeleteReturningApiResponse_ReportsRefusalInError()
	{
		using var client = ClientFor(new Uri("http://127.0.0.1:1/"), isReadOnly: true);

		using var response = await client.Resources.DeleteAsync("resource-1", TestContext.Current.CancellationToken);

		response.IsSuccessStatusCode.Should().BeFalse();
		IsRefused(response.Error!).Should().BeTrue();
	}

	/// <summary>With the option off the same write is sent and the server answers 404, so the failure is not the refusal.</summary>
	[Fact]
	public async Task NotReadOnly_Delete_IsSent()
	{
		await using var server = await FakePassboltServer.StartAsync();
		using var client = ClientFor(server.Address, isReadOnly: false);

		using var response = await client.Resources.DeleteAsync("resource-1", TestContext.Current.CancellationToken);

		response.Error.Should().NotBeNull();
		IsRefused(response.Error!).Should().BeFalse();
	}

	/// <summary>A read still goes through, signing in as usual.</summary>
	[Fact]
	public async Task ReadOnly_Get_StillGoesThrough()
	{
		await using var server = await FakePassboltServer.StartAsync();
		using var client = ClientFor(server.Address, isReadOnly: true);

		var act = () => client.Me.GetAsync(TestContext.Current.CancellationToken);

		await act.Should().NotThrowAsync();
	}

	/// <summary>A share simulation is a POST that changes nothing, so it is allowed by path.</summary>
	[Fact]
	public async Task ReadOnly_ShareSimulation_IsNotRefused()
	{
		await using var server = await FakePassboltServer.StartAsync();
		using var client = ClientFor(server.Address, isReadOnly: true);

		var act = () => client.Resources.SimulateShareAsync("resource-1", new ShareResourceRequest { Permissions = [] }, TestContext.Current.CancellationToken);

		var thrown = await act.Should().ThrowAsync<Exception>();
		IsRefused(thrown.Which).Should().BeFalse();
	}
}
