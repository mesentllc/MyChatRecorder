using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace Graph {
	public class Authentication {
		private const string asureActiveDirectory = "https://login.microsoftonline.com/{0}";
		private const string clientId = "811e4cbe-a823-499b-b6fe-7d90fb5b239f";
		private const string tenantId = "b945c813-dce6-41f8-8457-5a12c2fe15bf";
		private const string appKey = "I8bt:2uln]SBN2RTAk1HLYBsZQ@HuPn?";
		private const string resource = "https://graph.windows.net";
		static string[] apiScopes = new string[] { "Chat.Read", "Files.Read", "Files.ReadWrite", "Files.Read.All", "Files.ReadWrite.All", "Sites.Read.All", "Sites.ReadWrite.All" };
		static string authority = String.Format(CultureInfo.InvariantCulture, asureActiveDirectory, tenantId);

		private static AuthenticationContext context = null;
		private static Microsoft.IdentityModel.Clients.ActiveDirectory.ClientCredential credential = null;

		private static async Task<string> GetToken() {
			Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationResult result = await context.AcquireTokenAsync(resource, credential);
			return result.AccessToken;
		}

		public static string GetAccessToken() {
			context = new AuthenticationContext(authority);
			credential = new Microsoft.IdentityModel.Clients.ActiveDirectory.ClientCredential(clientId, appKey);
			Task<string> token = GetToken();
			token.Wait();
			return token.Result;
		}

		public static InteractiveAuthenticationProvider GetCredentialProvider() {
			IPublicClientApplication publicClientApplication = PublicClientApplicationBuilder.Create(clientId).WithTenantId(tenantId).WithDefaultRedirectUri().Build();
			return new InteractiveAuthenticationProvider(publicClientApplication);
		}
	}
}
