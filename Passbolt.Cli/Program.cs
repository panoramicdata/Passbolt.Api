using Passbolt.Cli.Commands;
using Passbolt.Cli.Commands.Audit;
using Passbolt.Cli.Commands.Folders;
using Passbolt.Cli.Commands.Groups;
using Passbolt.Cli.Commands.Permissions;
using Passbolt.Cli.Commands.Resources;
using Passbolt.Cli.Commands.Users;

var app = new CommandApp();
app.Configure(config =>
{
	config.SetApplicationName("passbolt");
	config.PropagateExceptions();

	config.AddCommand<StatusCommand>("status")
		.WithDescription("Show the Passbolt server status and healthcheck.");
	config.AddCommand<WhoAmICommand>("whoami")
		.WithDescription("Show the currently authenticated user.");
	config.AddCommand<ConfigureCommand>("configure")
		.WithDescription("Save server/username/private-key settings (never the passphrase).");

	config.AddBranch("resource", branch =>
	{
		branch.SetDescription("List, read and delete resources (passwords).");
		branch.AddCommand<ListResourcesCommand>("list").WithDescription("List all resources.");
		branch.AddCommand<GetResourceCommand>("get").WithDescription("Get a resource by id.");
		branch.AddCommand<DeleteResourceCommand>("delete").WithDescription("Delete a resource by id.");
	});

	config.AddBranch("user", branch =>
	{
		branch.SetDescription("List and read users.");
		branch.AddCommand<ListUsersCommand>("list").WithDescription("List all users.");
		branch.AddCommand<GetUserCommand>("get").WithDescription("Get a user by id.");
	});

	config.AddBranch("group", branch =>
	{
		branch.SetDescription("List and read groups.");
		branch.AddCommand<ListGroupsCommand>("list").WithDescription("List all groups.");
		branch.AddCommand<GetGroupCommand>("get").WithDescription("Get a group by id.");
	});

	config.AddBranch("folder", branch =>
	{
		branch.SetDescription("List and read folders.");
		branch.AddCommand<ListFoldersCommand>("list").WithDescription("List all folders.");
		branch.AddCommand<GetFolderCommand>("get").WithDescription("Get a folder by id.");
	});

	config.AddBranch("permission", branch =>
	{
		branch.SetDescription("Inspect permissions.");
		branch.AddCommand<ListPermissionsCommand>("list").WithDescription("List permissions for a resource or user.");
	});

	config.AddBranch("audit", branch =>
	{
		branch.SetDescription("Governance audits.");
		branch.AddCommand<OwnershipAuditCommand>("ownership")
			.WithDescription("Flag resources with too few owners or no group owner.");
	});
});

try
{
	return await app.RunAsync(args);
}
catch (CliException ex)
{
	Console.Error.WriteLine(ex.Message);
	return 1;
}
catch (Exception ex)
{
	Console.Error.WriteLine($"{ex.GetType().Name}: {ex.Message}");
	return 1;
}
