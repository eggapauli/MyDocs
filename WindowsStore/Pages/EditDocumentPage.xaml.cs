using Autofac;
using MyDocs.Common.Contract.Page;
using MyDocs.Common.Model.View;
using MyDocs.Common.ViewModel;
using MyDocs.WindowsStore.Common;
using MyDocs.WindowsStore.ViewModel;
using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Windows.UI.Xaml.Navigation;

namespace MyDocs.WindowsStore.Pages
{
    public sealed partial class EditDocumentPage : LayoutAwarePage, IEditDocumentPage
    {
        private EditDocumentViewModel ViewModel
        {
            get { return (EditDocumentViewModel)DataContext; }
            set { DataContext = value; }
        }

        public EditDocumentPage()
        {
            InitializeComponent();
        }

        protected override IEnumerable<IDisposable> Activate()
        {
            yield return ViewModel = ViewModelLocator.Container.Resolve<EditDocumentViewModel>();
        }

        protected override void LoadState(object sender, LoadStateEventArgs args)
        {
            if (args.PageState != null && args.PageState.ContainsKey("Id")) {
                args.PageState.ConvertToDocumentAsync().ContinueWith(t =>
                {
                    ViewModel.EditingDocument = Document.FromLogic(t.Result);
                    ViewModel.ShowNewCategoryInputValue = (bool)args.PageState["ShowNewCategoryInput"];
                    ViewModel.UseCategoryName = (string)args.PageState["UseCategoryName"];
                    ViewModel.NewCategoryName = (string)args.PageState["NewCategoryName"];
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
                ViewModel.EditingDocument.ToLogic().ConvertToRestorableDocument(args.PageState);
            }
            args.PageState["ShowNewCategoryInput"] = ViewModel.ShowNewCategoryInputValue;
            args.PageState["UseCategoryName"] = ViewModel.UseCategoryName;
            args.PageState["NewCategoryName"] = ViewModel.NewCategoryName;
        }
    }
}
