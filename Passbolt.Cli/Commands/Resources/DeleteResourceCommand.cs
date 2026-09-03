namespace Passbolt.Cli.Commands.Resources;

/// <summary>Deletes a resource by id (destructive; prompts unless --yes).</summary>
public sealed class DeleteResourceCommand() : DeleteEntityCommand("resource")
{
	protected override async Task<Refit.IApiResponse> DeleteAsync(
		PassboltClient client,
		string id,
		CancellationToken cancellationToken)
		=> await client.Resources.DeleteAsync(id, cancellationToken);
}
