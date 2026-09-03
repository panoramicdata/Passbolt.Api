namespace Passbolt.Cli.Commands.Users;

/// <summary>Deletes a user by id (destructive; prompts unless --yes).</summary>
public sealed class DeleteUserCommand() : DeleteEntityCommand("user")
{
	protected override async Task<Refit.IApiResponse> DeleteAsync(
		PassboltClient client,
		string id,
		CancellationToken cancellationToken)
		=> await client.Users.DeleteAsync(id, cancellationToken);
}
