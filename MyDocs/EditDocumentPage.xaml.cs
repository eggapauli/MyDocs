using MyDocs.Common;
using MyDocs.Model;
using MyDocs.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace MyDocs
{
	public sealed partial class EditDocumentPage : MyDocs.Common.LayoutAwarePage
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
			if (pageState != null) {
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
