using Microsoft.Graph;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Graph {
	class ChatRetriever {
		private const string rootUri = "https://graph.windows.net/b945c813-dce6-41f8-8457-5a12c2fe15bf/users?api-version=1.6";
		private static HttpClient httpClient = new HttpClient();

		private async Task<string> GetViaHttp() {
			string json = null;
			httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Authentication.GetAccessToken());
			var httpResponse = await httpClient.GetAsync(rootUri);
			if (httpResponse.Content != null) {
				json = await httpResponse.Content.ReadAsStringAsync();
			}
			return json;
		}

		public void GetAllViaHttpAsync() {
			Task<string> chatJson = GetViaHttp();
			chatJson.Wait();
			Console.WriteLine(chatJson.Result);
		}

		public void GetAllChatReferences() {
			IList<Chat> chatReferences;
			IList<ChatMessage> messageReference;
			IList<ConversationMember> memberReference;
			GraphServiceClient client = new GraphServiceClient(Authentication.GetCredentialProvider());
			Task<IUserChatsCollectionPage> chatPage = client.Me.Chats.Request().GetAsync();
			chatPage.Wait();
			while(true) {
				IUserChatsCollectionPage chats = chatPage.Result;
				chatReferences = chats.CurrentPage;
				foreach (Chat chat in chatReferences) {
					Task<IChatMessagesCollectionPage> chatMessagePage = client.Chats[chat.Id].Messages.Request().GetAsync();
					chatMessagePage.Wait();
					List<MessageDetail> messageDetailList = new List<MessageDetail>();
					while (true) {
						IChatMessagesCollectionPage messages = chatMessagePage.Result;
						messageReference = messages.CurrentPage;
						foreach(ChatMessage message in messageReference) {
							MessageDetail detail = new MessageDetail();
							detail.Body = message.Body.Content;
							detail.From = message.From.User.DisplayName;
							messageDetailList.Add(detail);
						}
						if (messageReference.Count == 0) {
							break;
						}
						chatMessagePage = messages.NextPageRequest.GetAsync();
						chatMessagePage.Wait();
					}

					Task<IChatMembersCollectionPage> chatMemberPage = client.Chats[chat.Id].Members.Request().GetAsync();
					chatMemberPage.Wait();
					IChatMembersCollectionPage members = chatMemberPage.Result;
					memberReference = members.CurrentPage;
					List<string> memberList = GetMembers(memberReference);
					if (memberList.Count > 0 && messageDetailList.Count > 0) {
						Console.WriteLine("Chat between " + DisplayMembers(memberList));
						foreach (MessageDetail detail in messageDetailList) {
							Console.WriteLine(detail.From + ": " + detail.Body);
						}
					}
				}
				if (chats.NextPageRequest == null) {
					break;
				}
				chatPage = chats.NextPageRequest.GetAsync();
				chatPage.Wait();
			}
		}

		private string DisplayMembers(List<string> memberList) {
			string members = "";
			foreach(string member in memberList) {
				members += member + ", ";
			}
			return members.Substring(0, members.Length - 2);
		}

		private List<MessageDetail> GetMessages(IChatMessagesCollectionPage messagePage) {
			List<MessageDetail> messageList = new List<MessageDetail>();
			IList<ChatMessage> messageReferences;
			if (messagePage != null) {
				messageReferences = messagePage.CurrentPage;
				foreach (ChatMessage message in messageReferences) {
					MessageDetail detail = new MessageDetail();
					detail.Body = message.Body.Content;
					detail.From = message.From.User.DisplayName;
				}
			}
			return messageList;
		}

		private List<string> GetMembers(IList<ConversationMember> memberReference) {
			List<string> memberList = new List<string>();
			if (memberReference != null) {
				foreach (ConversationMember member in memberReference) {
					memberList.Add(member.DisplayName);
				}
			}
			return memberList;
		}
	}
}
