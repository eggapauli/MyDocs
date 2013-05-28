using MyDocs.Common;
using MyDocs.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.Storage;

namespace MyDocs.Contract.Service
{
	public interface IDocumentService
	{
		SortedObservableCollection<Category> Categories { get; }
		Task LoadCategoriesAsync();
		IEnumerable<string> GetCategoryNames();
		Category GetCategoryByName(string name);

		Task<Document> GetDocumentById(Guid id);
		void DetachDocument(Document doc);
		Task SaveDocumentAsync(Document doc);
		Task DeleteDocumentAsync(Document doc);

		Task RemovePhotosAsync(IEnumerable<StorageFile> photos);
	}
}
