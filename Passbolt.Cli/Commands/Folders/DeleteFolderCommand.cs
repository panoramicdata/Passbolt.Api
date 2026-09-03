namespace Passbolt.Cli.Commands.Folders;

/// <summary>Deletes a folder by id (destructive; prompts unless --yes).</summary>
public sealed class DeleteFolderCommand() : DeleteEntityCommand("folder")
{
	protected override async Task<Refit.IApiResponse> DeleteAsync(
		PassboltClient client,
		string id,
		CancellationToken cancellationToken)
		=> await client.Folders.DeleteFolderAsync(id, cancellationToken);
}
