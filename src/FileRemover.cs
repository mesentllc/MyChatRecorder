using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyChatRecorder {
	public class FileRemover {
		
		public void RemoveControlFiles(GraphServiceClient client) {
			IList<DriveItem> driveItems;

			Console.WriteLine("URI: " + client.Me.Drive.Root.Children.RequestUrl);
			Task<IDriveItemChildrenCollectionPage> childrenPage = client.Me.Drive.Root.Children.Request().GetAsync();
			childrenPage.Wait();

			while(true) {
				IDriveItemChildrenCollectionPage children = childrenPage.Result;
				driveItems = children.CurrentPage;
				foreach (DriveItem item in driveItems) {
					Console.WriteLine("Found: " + item.Name);
					if (item.Name.StartsWith("Cntl_")) {
						Console.WriteLine("Want to delete: " + item.Name);
					}
				}
				if (children.NextPageRequest == null) {
					break;
				}
				childrenPage = children.NextPageRequest.GetAsync();
			}
		}
	}
}