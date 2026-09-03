namespace Passbolt.Cli.Commands.Groups;

/// <summary>Deletes a group by id (destructive; prompts unless --yes).</summary>
public sealed class DeleteGroupCommand() : DeleteEntityCommand("group")
{
	protected override async Task<Refit.IApiResponse> DeleteAsync(
		PassboltClient client,
		string id,
		CancellationToken cancellationToken)
		=> await client.Groups.DeleteAsync(id, cancellationToken);
}
