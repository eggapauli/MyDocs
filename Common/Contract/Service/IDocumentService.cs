using MyDocs.Common.Collection;
using MyDocs.Common.Contract.Storage;
using MyDocs.Common.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyDocs.Common.Contract.Service
{
	public interface IDocumentService
	{
		SortedObservableCollection<Category> Categories { get; }
		Task LoadCategoriesAsync();
		IEnumerable<string> GetCategoryNames();
		Category GetCategoryByName(string name);
        Task RenameCategoryAsync(Category cat, string NewCategoryName);

		Task<Document> GetDocumentById(Guid id);
		void DetachDocument(Document doc);
		Task SaveDocumentAsync(Document doc);
		Task DeleteDocumentAsync(Document doc);

		Task RemovePhotosAsync(IEnumerable<IFile> photos);
    }
}
