using System.Collections.Concurrent;

namespace Passbolt.Api.Test;

internal static class TestFolderCacheService
{
	private static readonly ConcurrentDictionary<string, Lazy<Task<string>>> FolderIdByKey = new(StringComparer.OrdinalIgnoreCase);

	public static Task<string> GetOrAddAsync(string cacheKey, Func<Task<string>> valueFactory)
	{
		var lazy = FolderIdByKey.GetOrAdd(cacheKey, _ =>
			new Lazy<Task<string>>(valueFactory, LazyThreadSafetyMode.ExecutionAndPublication));

		return lazy.Value;
	}
}
