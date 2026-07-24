using FlightPlan.Services;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Azure.Identity;
using FlightPlan.Models.Config;
using FlightPlan.Core.Interfaces;

namespace FlightPlan.Infrastructure.Services;

public abstract class MicrosoftGraphBase
{
    protected readonly MicrosoftGraphConfig Config;
    protected readonly ILogger Logger;
    protected readonly IStorageService StorageService;
    private GraphServiceClient? _graphClient;

    protected MicrosoftGraphBase(DashConfig config, ILogger logger, IStorageService storageService)
    {
        Config = config.MicrosoftGraph;
        Logger = logger;
        StorageService = storageService;
    }

    private string AuthRecordPath => StorageService.GetAuthRecordPath();

    protected async Task<GraphServiceClient> GetClientAsync()
    {
        if (_graphClient != null) return _graphClient;

        var scopes = new[] { "Mail.Read", "Calendars.Read", "User.Read", "Calendars.Read.Shared", "Offline_Access", "Mail.ReadWrite" };
        var options = new InteractiveBrowserCredentialOptions
        {
            TenantId = Config.TenantId,
            ClientId = Config.ClientId,
            RedirectUri = new Uri("http://localhost"),
            TokenCachePersistenceOptions = new TokenCachePersistenceOptions()
        };

        if (File.Exists(AuthRecordPath))
        {
            try
            {
                using var authRecordStream = new FileStream(AuthRecordPath, FileMode.Open, FileAccess.Read);
                options.AuthenticationRecord = await AuthenticationRecord.DeserializeAsync(authRecordStream);
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Failed to deserialize auth record.");
            }
        }
        else if (File.Exists("auth_record_flightplan.bin"))
        {
            // Migration
            try
            {
                File.Copy("auth_record_flightplan.bin", AuthRecordPath);
                using var authRecordStream = new FileStream(AuthRecordPath, FileMode.Open, FileAccess.Read);
                options.AuthenticationRecord = await AuthenticationRecord.DeserializeAsync(authRecordStream);
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Failed to migrate auth record.");
            }
        }

        var credential = new InteractiveBrowserCredential(options);

        try
        {
            if (options.AuthenticationRecord == null)
            {
                var authRecord = await credential.AuthenticateAsync(new Azure.Core.TokenRequestContext(scopes));
                using var authRecordStream = new FileStream(AuthRecordPath, FileMode.Create, FileAccess.Write);
                await authRecord.SerializeAsync(authRecordStream);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Authentication failed.");
            if (File.Exists(AuthRecordPath)) File.Delete(AuthRecordPath);
            throw;
        }

        _graphClient = new GraphServiceClient(credential, scopes);
        return _graphClient;
    }
}
