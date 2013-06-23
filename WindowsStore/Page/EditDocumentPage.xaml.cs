using MyDocs.Common.Contract.Page;
using MyDocs.Common.Model;
using MyDocs.Common.ViewModel;
using MyDocs.WindowsStore.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyDocs.WindowsStore.Page
{
	public sealed partial class EditDocumentPage : LayoutAwarePage, IEditDocumentPage
	{
		public EditDocumentPage()
		{
			this.InitializeComponent();
		}

		public EditDocumentViewModel ViewModel
		{
			get { return this.DataContext as EditDocumentViewModel; }
		}

		protected override void LoadState(object navigationParameter, Dictionary<string, object> pageState)
		{
			ViewModel.LoadAsync().ContinueWith(t =>
			{
				if (t.IsFaulted) {
					// TODO show error
				}
			});
			if (pageState != null && pageState.ContainsKey("Id")) {
				pageState.ConvertToDocumentAsync().ContinueWith(t =>
				{
					if (t.IsFaulted) {
						// TODO show error
					}
					else {
						ViewModel.EditingDocument = t.Result;
						ViewModel.ShowNewCategoryInput = (bool)pageState["ShowNewCategoryInput"];
						ViewModel.UseCategoryName = (string)pageState["UseCategoryName"];
						ViewModel.NewCategoryName = (string)pageState["NewCategoryName"];
					}
				}, TaskScheduler.FromCurrentSynchronizationContext());

			}
			else if (navigationParameter != null) {
				ViewModel.EditingDocumentId = (Guid)navigationParameter;
			}
			else {
				ViewModel.EditingDocument = new Document();
			}
		}

		protected override void SaveState(Dictionary<string, object> pageState)
		{
			if (ViewModel.EditingDocument != null) {
				ViewModel.EditingDocument.ConvertToRestorableDocument(pageState);
			}
			pageState["ShowNewCategoryInput"] = ViewModel.ShowNewCategoryInput;
			pageState["UseCategoryName"] = ViewModel.UseCategoryName;
			pageState["NewCategoryName"] = ViewModel.NewCategoryName;
		}
	}
}
