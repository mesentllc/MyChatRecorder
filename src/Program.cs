using Graph;
using Microsoft.Graph;

namespace MyChatRecorder {
    class Program {
        private static ChatRetriever chatRetriever = new ChatRetriever();
        private static FileRemover fileRemover = new FileRemover();

        static void Main(string[] args) {
            GraphServiceClient client = new GraphServiceClient(Graph.Authentication.GetCredentialProvider());
            chatRetriever.GetAllChatReferences(client);
//            fileRemover.RemoveControlFiles(client);
        }
    }
}
