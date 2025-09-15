using Azure.Identity;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using MapelRestAPI.Entities;
using MapelRestAPI.Interfaces;
using Newtonsoft.Json;

namespace MapelRestAPI.Identity;

public class GraphUserService : IUserService
{
    private readonly ILogger<GraphUserService> _logger;
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly string _tenantId;
    private readonly string _roletId;
    private readonly string _redirectUrl;

    public GraphUserService(IConfiguration config, ILogger<GraphUserService> logger)
    {
        var azureAd = config.GetSection("AzureAd");
        _clientId = azureAd["ClientId"]!;
        _clientSecret = azureAd["ClientSecret"]!;
        _tenantId = azureAd["TenantId"]!;
        _roletId = azureAd["RoleId"]!;
        _redirectUrl = azureAd["RedirectUrl"]!;
        _logger = logger;
    }

    private GraphServiceClient GetClient()
    {
        var credential = new ClientSecretCredential(
            _tenantId,
            _clientId,
            _clientSecret
        );

        return new GraphServiceClient(credential, new[] { "https://graph.microsoft.com/.default" });
    }

    public async Task<List<string>> CreateUsersAsync(List<AppUser> users)
    {
        var client = GetClient();
        var results = new List<string>();

        foreach (var user in users)
        {
            try
            {
                var newUser = new User
                {
                    DisplayName = user.DisplayName,
                    UserPrincipalName = $"{Guid.NewGuid().ToString()}@maplecommercialafexid.onmicrosoft.com",
                    MailNickname = user.Email.Split('@')[0],
                    AccountEnabled = true,
                    PasswordProfile = new PasswordProfile
                    {
                        Password = user.Password,
                        ForceChangePasswordNextSignIn = true
                    },
                    Mail = user.Email,
                    OtherMails = new List<string> { user.Email }
                };

                var resultCreate = await client.Users.PostAsync(newUser);
                results.Add($"Created: {resultCreate?.UserPrincipalName}");

                if (!string.IsNullOrEmpty(resultCreate?.Id))
                {
                    var servicePrincipal = await client.ServicePrincipals
                        .GetAsync(requestConfig =>
                            requestConfig.QueryParameters.Filter = $"appId eq '{_clientId}'");

                    var resudltRole = await client.Users[resultCreate?.Id]
                        .AppRoleAssignments
                        .PostAsync(new AppRoleAssignment
                        {
                            PrincipalId = Guid.Parse(resultCreate?.Id!),     // user ID
                            ResourceId = Guid.Parse(servicePrincipal?.Value?[0].Id!),       // service principal ID of the target app
                            AppRoleId = Guid.Parse(_roletId)             // ID of the app role
                        });

                }
            }
            catch (Exception ex)
            {
                results.Add($"Failed: {user.Email} - {ex.Message}");
            }
        }

        return results;
    }

    public async Task<string> InviteExternalUserAsync(string email)
    {
        _logger.LogInformation("Starting invitation for {Email}", email);
        var client = GetClient();
        try
        {
            _logger.LogInformation($"Using _tenantId: {_tenantId}");
            _logger.LogInformation($"Using _clientId: {_clientId}");
            _logger.LogInformation($"Using _clientSecret: {_clientSecret}");
            _logger.LogInformation($"Using invite Email: {email}");
            _logger.LogInformation($"Using invite redirect URL: {_redirectUrl}");

            var invitation = new Invitation
            {
                InvitedUserEmailAddress = email,
                InviteRedirectUrl = _redirectUrl,
                SendInvitationMessage = true,
                InvitedUserDisplayName = email.Split('@')[0] // Optional
            };

            var result = await client.Invitations.PostAsync(invitation);

            return $"Invitation sent successfully to: {result?.InvitedUserEmailAddress}";
        }
        catch (Exception ex)
        {
            _logger.LogInformation($"Using Exception: {ex.Message} {ex.InnerException}");
            return $"Error sending invitation: {ex.Message}";
        }
    }

    public async Task<string> GetUserDetailsAsync(string email)
    {
       
        try
        {
            var userInfo = new UserInfo
            {
                UserId = Guid.NewGuid().ToString(),
                UserName = "Mapel User",
                RoleId = Guid.NewGuid().ToString(),
                RoleName = "Broker"
            };

            var result = JsonConvert.SerializeObject(userInfo);

            return result!;
        }
        catch (Exception ex)
        {
            return string.Empty;
        }
    }
}
