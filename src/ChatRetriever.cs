using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Directory = System.IO.Directory;
using File = System.IO.File;

namespace Graph {

	class ChatRetriever {
		private const string rootUri = "https://graph.windows.net/b945c813-dce6-41f8-8457-5a12c2fe15bf/users?api-version=1.6";
		private static HttpClient httpClient = new HttpClient();
		private static DateTime today = DateTime.Today;
		private static int sequence = 1;

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

		public void GetAllChatReferences(GraphServiceClient client) {
			IList<Chat> chatReferences;

			Task<IUserChatsCollectionPage> chatPage = client.Me.Chats.Request().GetAsync();
			chatPage.Wait();
			while(true) {
				IUserChatsCollectionPage chats = chatPage.Result;
				chatReferences = chats.CurrentPage;
				foreach (Chat chat in chatReferences) {
					List<MessageDetail> messageDetailList = GetMessageDetails(client, chat.Id);
					List<string> memberList = GetMembers(client, chat.Id);
					if (memberList.Count > 0 && messageDetailList.Count > 0) {
						WriteChatMessages(BuildFilename(memberList), messageDetailList);
					}
				}
				if (chats.NextPageRequest == null) {
					break;
				}
				chatPage = chats.NextPageRequest.GetAsync();
				chatPage.Wait();
			}
		}

		private void WriteChatMessages(string filename, List<MessageDetail> messageDetailList) {
			SortedSet<string> sortedMessages = new SortedSet<string>();
			string lineFormat = "[{0}] {1}: {2}";
			foreach(MessageDetail detail in messageDetailList) {
				string dateTimeString = "NULL";
				if (detail.Date is DateTimeOffset dateTimeOffset) {
					dateTimeString = dateTimeOffset.ToString("yyyy-MM-dd HH:mm:ss");
				}
				sortedMessages.Add(String.Format(CultureInfo.InvariantCulture, lineFormat, dateTimeString, detail.From, detail.Body));
			}
			string volume = Directory.GetDirectoryRoot(Directory.GetCurrentDirectory());
			string pathString = "{0}Chats\\{1}";
			string path = String.Format(CultureInfo.InvariantCulture, pathString, volume, today.ToString("yyyyMMdd"));
			Directory.CreateDirectory(path);
			string completePath = path + "\\" + filename + ".txt";
			Console.WriteLine("Writing file: " + completePath);
			using (StreamWriter writer = File.AppendText(completePath)) {
				foreach (string detail in sortedMessages) {
					writer.WriteLine(detail);
				}
			}
		}

		private List<MessageDetail> GetMessageDetails(GraphServiceClient client, string chatId) {
			IList<ChatMessage> messageReference;
			List<MessageDetail> messageDetailList = new List<MessageDetail>();

			Task<IChatMessagesCollectionPage> chatMessagePage = client.Chats[chatId].Messages.Request().GetAsync();
			chatMessagePage.Wait();
			while (true) {
				IChatMessagesCollectionPage messages = chatMessagePage.Result;
				messageReference = messages.CurrentPage;
				foreach (ChatMessage message in messageReference) {
					MessageDetail detail = new MessageDetail();
					detail.Body = message.Body.Content;
					detail.From = message.From.User.DisplayName;
					detail.Date = message.CreatedDateTime;
					messageDetailList.Add(detail);
				}
				if (messageReference.Count == 0) {
					break;
				}
				try {
					chatMessagePage = messages.NextPageRequest.GetAsync();
					chatMessagePage.Wait();
				}
				catch (Exception ae) {
					Console.WriteLine("Exception Caught: " + ae.Message);
					break;
				}
			}
			return messageDetailList;
		}

		private string BuildFilename(List<string> memberList) {
			SortedSet<string> nameSet = new SortedSet<string>();
			string members = "";
			string[] parts;

			foreach (string member in memberList) {
				if (member == null) {
					parts = new string[] { "null" };
				}
				else {
					parts = member.Split(' ');
				}
				int lastNamePos = parts.Length;
				while (lastNamePos > 0 && parts[lastNamePos - 1].StartsWith('(')) {
					lastNamePos--;
				}
				if (lastNamePos > 0) {
					nameSet.Add(parts[lastNamePos - 1]);
				}
			}
			foreach(string name in nameSet) {
				members += name + "_";
			}
			string proposedFilename = members.Substring(0, members.Length - 1);
			if (proposedFilename.Length > 100) {
				proposedFilename = proposedFilename.Substring(0, 100) + "-" + sequence++;
			}
			return proposedFilename;
		}

		private List<string> GetMembers(GraphServiceClient client, string chatId) {
			IList<ConversationMember> memberReference;

			Task<IChatMembersCollectionPage> chatMemberPage = client.Chats[chatId].Members.Request().GetAsync();
			chatMemberPage.Wait();
			IChatMembersCollectionPage members = chatMemberPage.Result;
			memberReference = members.CurrentPage;
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
