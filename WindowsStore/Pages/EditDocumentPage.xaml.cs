using MyDocs.Common.Contract.Page;
using MyDocs.Common.Model;
using MyDocs.Common.ViewModel;
using MyDocs.WindowsStore.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyDocs.WindowsStore.Pages
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

		protected override void LoadState(object sender, LoadStateEventArgs args)
		{
			ViewModel.LoadAsync().ContinueWith(t =>
			{
				if (t.IsFaulted) {
					// TODO show error
				}
			});
			if (args.PageState != null && args.PageState.ContainsKey("Id")) {
				args.PageState.ConvertToDocumentAsync().ContinueWith(t =>
				{
					if (t.IsFaulted) {
						// TODO show error
					}
					else {
						ViewModel.EditingDocument = t.Result;
						ViewModel.ShowNewCategoryInput = (bool)args.PageState["ShowNewCategoryInput"];
                        ViewModel.UseCategoryName = (string)args.PageState["UseCategoryName"];
                        ViewModel.NewCategoryName = (string)args.PageState["NewCategoryName"];
					}
				}, TaskScheduler.FromCurrentSynchronizationContext());

			}
			else if (args.NavigationParameter != null) {
                ViewModel.EditingDocumentId = (Guid)args.NavigationParameter;
			}
			else {
				ViewModel.EditingDocument = new Document();
			}
		}

		protected override void SaveState(object sender, SaveStateEventArgs args)
		{
			if (ViewModel.EditingDocument != null) {
				ViewModel.EditingDocument.ConvertToRestorableDocument(args.PageState);
			}
            args.PageState["ShowNewCategoryInput"] = ViewModel.ShowNewCategoryInput;
            args.PageState["UseCategoryName"] = ViewModel.UseCategoryName;
            args.PageState["NewCategoryName"] = ViewModel.NewCategoryName;
		}
	}
}
