﻿using MyDocs.Common.Collection;
using MyDocs.Common.Comparer;
using MyDocs.Common.Contract.Service;
using MyDocs.Common.Contract.Storage;
using MyDocs.Common.Model;
using MyDocs.WindowsStore.Common;
using MyDocs.WindowsStore.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace MyDocs.WindowsStore.Service
{
	public class DocumentService : IDocumentService
	{
		private ISettingsService settingsService;
		private IApplicationDataContainer docsDataContainer;

		private readonly IFolder tempFolder = new WindowsStoreFolder(ApplicationData.Current.TemporaryFolder);

		public SortedObservableCollection<Category> Categories { get; private set; }

		public DocumentService(ISettingsService settingsService)
		{
			this.settingsService = settingsService;
			Categories = new SortedObservableCollection<Category>(new CategoryComparer());

			docsDataContainer = settingsService.SettingsContainer.CreateContainer("docs");

		}

		private async Task ClearAllData()
		{
			docsDataContainer.Values.Clear();

			var folders = new[] {
				ApplicationData.Current.LocalFolder,
				ApplicationData.Current.RoamingFolder,
				ApplicationData.Current.TemporaryFolder
			};

			var tasks = new List<Task>();
			foreach (var folder in folders) {
				var task = folder.GetFilesAsync().AsTask();
				tasks.AddRange(task.Result.Select(f => f.DeleteAsync().AsTask()));
			}
			await Task.WhenAll(tasks);
		}

		private async Task InsertTestData()
		{
			var service = new MyDocs.WindowsStore.Service.Design.DesignDocumentService();
			await service.LoadCategoriesAsync();
			var docs = service.Categories.SelectMany(c => c.Documents);

			var tasks = docs.Select(d => SaveDocumentAsync(d));
			await Task.WhenAll(tasks);
		}

		public async Task LoadCategoriesAsync()
		{
#if DEBUG
			//await ClearAllData();
			//await InsertTestData();
#endif
			if (Categories.Count > 0) {
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
				var tasks = new List<Task<Document>>();
				foreach (ApplicationDataCompositeValue item in group) {
					tasks.Add(item.ConvertToDocumentAsync());
					if (++i % 5 == 0) {
						tasks.Add(Task.Run<Document>(() => new AdDocument()));
					}
				}
				Categories.Add(new Category(group.Key, await Task.WhenAll(tasks)));
			}

			await RemoveOutdatedDocuments(Categories);
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
			var cat = Categories.FirstOrDefault(c => c.Name == name);
			if (cat == null) {
				cat = new Category(name);
				Categories.Add(cat);
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
			var docRef = Categories.SelectMany(c => c.Documents).FirstOrDefault(d => d.Id == doc.Id);
			var cat = Categories.FirstOrDefault(c => c.Documents.Contains(docRef));
			if (cat != null) {
				cat.Documents.Remove(docRef);
				if (!cat.Documents.Any(d => !(d is AdDocument))) {
					Categories.Remove(cat);
				}
			}
		}

		public async Task SaveDocumentAsync(Document doc)
		{
			var tasks = new List<Task>();
			foreach (var photo in doc.Photos) {
				if (!photo.File.IsInFolder(settingsService.PhotoFolder)) {
					string name = Path.GetRandomFileName() + Path.GetExtension(photo.File.Path);

					Task task;
					if (photo.File.IsInFolder(tempFolder)) {
						task = photo.File.MoveAsync(settingsService.PhotoFolder, name);
					}
					else {
						task = photo.File.CopyAsync(settingsService.PhotoFolder, name).ContinueWith(t =>
						{
							// TODO is this the correct `photo`?
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
			var tasks = doc.Photos.Select(p => p.File.MoveAsync(tempFolder));
			await Task.WhenAll(tasks);

			docsDataContainer.Values.Remove(doc.Id.ToString());

			DetachDocument(doc);
		}

		public async Task RemovePhotosAsync(IEnumerable<IFile> photos)
		{
			var tasks = photos.Select(p => p.DeleteAsync());
			await Task.WhenAll(tasks);
		}

		private async Task RemoveOutdatedDocuments(ICollection<Category> categories)
		{
			var docs = (from cat in categories
						from doc in cat.Documents
						where !(doc is AdDocument)
						where doc.HasLimitedLifespan
						where doc.DateRemoved < DateTime.Today
						select doc).ToList();

			var tasks = new List<Task>();
			foreach (var doc in docs) {
				var cat = categories.First(c => c.Name == doc.Category);
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