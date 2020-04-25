using Graph;
using System;

namespace MyChatRecorder {
    class Program {
        private static ChatRetriever chatRetriever = new ChatRetriever();

        static void Main(string[] args) {
//          chatRetriever.GetAllViaHttpAsync();
            chatRetriever.GetAllChatReferences();
        }
    }
}
