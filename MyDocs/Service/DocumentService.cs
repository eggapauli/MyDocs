using MyDocs.Common;
using MyDocs.Contract;
using MyDocs.Contract.Service;
using MyDocs.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace MyDocs.Service
{
	public class DocumentService : IDocumentService
	{
		private ISettingsService settingsService;
		private ApplicationDataContainer docsDataContainer;

		private SortedObservableCollection<Category> categories;

		public SortedObservableCollection<Category> Categories
		{
			get { return categories; }
		}

		public DocumentService(ISettingsService settingsService)
		{
			this.settingsService = settingsService;

			docsDataContainer = settingsService.SettingsContainer
				.CreateContainer("docs", ApplicationDataCreateDisposition.Always);

			categories = new SortedObservableCollection<Category>(new CategoryComparer());
		}

		private async Task ClearAllData()
		{
			docsDataContainer.Values.Clear();

			StorageFolder[] folders = new[] {
				ApplicationData.Current.LocalFolder,
				ApplicationData.Current.RoamingFolder,
				ApplicationData.Current.TemporaryFolder
			};

			List<Task> tasks = new List<Task>();
			foreach (StorageFolder folder in folders) {
				Task<IReadOnlyList<StorageFile>> t = folder.GetFilesAsync().AsTask();
				tasks.AddRange(t.Result.Select(f => f.DeleteAsync().AsTask()));
			}
			await Task.WhenAll(tasks);
		}

		private async Task InsertTestData()
		{
			MyDocs.Service.Design.DesignDocumentService service = new MyDocs.Service.Design.DesignDocumentService();
			await service.LoadCategoriesAsync();
			IEnumerable<Document> docs = service.Categories.SelectMany(c => c.Documents);
			IList<Task> tasks = new List<Task>();
			foreach (Document doc in docs) {
				tasks.Add(SaveDocumentAsync(doc));
			}
			await Task.WhenAll(tasks);
		}

		public async Task LoadCategoriesAsync()
		{
			//await ClearAllData();
			//await InsertTestData();
			if (categories.Count > 0) {
				return;
			}
			//await Task.Delay(2000);
			//categories.Clear();

			var data = from obj in docsDataContainer.Values.Values
					   let item = obj as ApplicationDataCompositeValue
					   let categoryName = (string)(item["Category"])
					   where item != null
					   orderby categoryName
					   group item by categoryName into category
					   select category;

			var i = 0;
			foreach (var group in data) {
				IList<Task<Document>> tasks = new List<Task<Document>>();
				foreach (ApplicationDataCompositeValue item in group) {
					tasks.Add(item.ConvertToDocumentAsync());
					if (++i % 5 == 0) {
						tasks.Add(Task.Run<Document>(() => new AdDocument()));
					}
				}
				categories.Add(new Category(group.Key, await Task.WhenAll(tasks)));
			}

			await RemoveOutdatedDocuments(categories);
		}

		public IEnumerable<string> GetCategoryNames()
		{
			return from obj in docsDataContainer.Values.Values
				   let item = obj as ApplicationDataCompositeValue
				   let categoryName = (string)(item["Category"])
				   group item by categoryName into category
				   orderby category.Key
				   select category.Key;
		}

		public Category GetCategoryByName(string name)
		{
			Category cat = Categories.FirstOrDefault(c => c.Name == name);
			if (cat == null) {
				cat = new Category(name);
				categories.Add(cat);
			}
			return cat;
		}

		public async Task<Document> GetDocumentById(Guid id)
		{
			await LoadCategoriesAsync();
			return Categories.SelectMany(c => c.Documents).FirstOrDefault(d => d.Id == id);
		}

		public void DetachDocument(Document doc)
		{
			Document docRef = Categories.SelectMany(c => c.Documents).FirstOrDefault(d => d.Id == doc.Id);
			Category cat = Categories.FirstOrDefault(c => c.Documents.Contains(docRef));
			if (cat != null) {
				cat.Documents.Remove(docRef);
				if (!cat.Documents.Any(d => !(d is AdDocument))) {
					categories.Remove(cat);
				}
			}
		}

		public async Task SaveDocumentAsync(Document doc)
		{
			List<Task> tasks = new List<Task>();
			foreach (Photo photo in doc.Photos) {
				if (!photo.File.IsInFolder(settingsService.PhotoFolder)) {
					string name = Path.GetRandomFileName() + Path.GetExtension(photo.File.Path);
					Task task;
					if (photo.File.IsInFolder(ApplicationData.Current.TemporaryFolder)) {
						task = photo.File.MoveAsync(settingsService.PhotoFolder, name).AsTask();
					}
					else {
						task = photo.File.CopyAsync(settingsService.PhotoFolder, name).AsTask().ContinueWith(t =>
						{
							photo.File = t.Result;
						}, TaskScheduler.FromCurrentSynchronizationContext());
					}
					tasks.Add(task);
				}
			}
			await Task.WhenAll(tasks);

			docsDataContainer.Values[doc.Id.ToString()] = doc.ConvertToStoredDocument();
		}

		public async Task DeleteDocumentAsync(Document doc)
		{
			List<Task> tasks = new List<Task>();
			foreach (Photo photo in doc.Photos) {
				Task t = photo.File.MoveAsync(ApplicationData.Current.TemporaryFolder).AsTask();
				tasks.Add(t);
			}
			await Task.WhenAll(tasks);

			docsDataContainer.Values.Remove(doc.Id.ToString());

			DetachDocument(doc);
		}

		public async Task RemovePhotosAsync(IEnumerable<StorageFile> photos)
		{
			IList<Task> tasks = new List<Task>();
			foreach (StorageFile photo in photos) {
				//if (!photo.IsInFolder(ApplicationData.Current.TemporaryFolder)) {
				tasks.Add(photo.DeleteAsync().AsTask());
				//}
			}
			await Task.WhenAll(tasks);
		}

		private async Task RemoveOutdatedDocuments(ICollection<Category> categories)
		{
			IEnumerable<Document> docs = categories.SelectMany(c => c.Documents).Where(d => !(d is AdDocument) && d.HasLimitedLifespan && d.DateRemoved < DateTime.Today).ToList();
			List<Task> tasks = new List<Task>();
			foreach (Document doc in docs) {
				Category cat = categories.First(c => c.Name == doc.Category);
				cat.Documents.Remove(doc);
				tasks.Add(DeleteDocumentAsync(doc));
				if (cat.Documents.Count == 0) {
					categories.Remove(cat);
				}
			}
			await Task.WhenAll(tasks);
		}
	}
}
