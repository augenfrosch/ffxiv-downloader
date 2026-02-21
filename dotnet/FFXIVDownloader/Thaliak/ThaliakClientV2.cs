using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace FFXIVDownloader.Thaliak;

public sealed partial class ThaliakClientV2 : IDisposable
{
    private const string BASE_URL = "https://thaliak.xiv.dev/api/v2beta";
    private HttpClient Client { get; }

    public ThaliakClientV2()
    {
        Client = new();
    }

    internal sealed record PatchesV2
    {
        [property: JsonPropertyName("patches")]
        public required List<PatchV2> Patches { get; init; }
        [property: JsonPropertyName("total")]
        public required long Total { get; init; }
        [property: JsonPropertyName("total_size")]
        public required long TotalSize { get; init; }
    }

    internal sealed record PatchV2
    {
        [property: JsonPropertyName("version_string")]
        public required string VersionString { get; init; }
        [property: JsonPropertyName("remote_url")]
        public required string RemoteUrl { get; init; }
        [property: JsonPropertyName("size")]
        public required long Size { get; init; }
    }

    internal sealed record LatestPatchV2
    {
        [property: JsonPropertyName("version_string")]
        public required string VersionString { get; init; }
        // ...
    }

    internal sealed record RepositoryV2
    {
        [property: JsonPropertyName("name")]
        public required string Name { get; init; }
        [property: JsonPropertyName("description")]
        public required string Description { get; init; }
        [property: JsonPropertyName("latest_patch")]
        public required LatestPatchV2 LatestPatch{ get; init; }
    }

    [JsonSerializable(typeof(RepositoryV2))]
    internal sealed partial class RepositoryResponseV2 : JsonSerializerContext { }

    [JsonSerializable(typeof(PatchesV2))]
    internal sealed partial class PatchesResponseV2 : JsonSerializerContext { }

    public async Task<Repository> GetRepositoryMetadataAsync(string slug, CancellationToken token = default)
    {
        var test = await Client.GetStringAsync($"{BASE_URL}/repositories/{slug}", token);
        Log.Debug(test);
        var repository = await Client.GetFromJsonAsync($"{BASE_URL}/repositories/{slug}", RepositoryResponseV2.Default.RepositoryV2, token);

        return repository != null ? new Repository() { Name = repository.Name, Description = repository.Description, LatestVersion = new Version() { VersionString = new GameVersion(repository.LatestPatch.VersionString)}} : new();
    }

    public async Task<List<(GameVersion, Patch)>> GetPatchChainAsync(string slug, GameVersion version, CancellationToken token = default)
    {
        var patches = await Client.GetFromJsonAsync($"{BASE_URL}/repositories/{slug}/patches?to={version}", PatchesResponseV2.Default.PatchesV2, token);
        
        return (patches?.Patches ?? []).ConvertAll((patchV2) => (new GameVersion(patchV2.VersionString), new Patch() { Size =  patchV2.Size, Url = patchV2.RemoteUrl}));
    }

    public void Dispose()
    {
        Client.Dispose();
    }
}