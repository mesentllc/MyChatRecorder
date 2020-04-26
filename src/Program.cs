using Graph;

namespace MyChatRecorder {
    class Program {
        private static ChatRetriever chatRetriever = new ChatRetriever();

        static void Main(string[] args) {
            chatRetriever.GetAllChatReferences();
        }
    }
}
