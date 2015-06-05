using FakeItEasy;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyDocs.Common.Contract.Service;
using MyDocs.Common.ViewModel;
using System.Threading.Tasks;

namespace MyDocs.Common.Test.ViewModel
{
    [TestClass]
    public partial class DocumentViewModelTest
    {
        private static DocumentViewModel CreateSut(
            IDocumentService documentService = null,
            IUserInterfaceService uiService = null,
            INavigationService navigationService = null,
            ILicenseService licenseService = null,
            IExportDocumentService exportDocumentService = null,
            IImportDocumentService importDocumentService = null)
        {
            documentService = documentService ?? A.Fake<IDocumentService>();
            uiService = uiService ?? A.Fake<IUserInterfaceService>();
            navigationService = navigationService ?? A.Fake<INavigationService>();
            licenseService = licenseService ?? A.Fake<ILicenseService>();
            exportDocumentService = exportDocumentService ?? A.Fake<IExportDocumentService>();
            importDocumentService = importDocumentService ?? A.Fake<IImportDocumentService>();
            return new DocumentViewModel(documentService, uiService, navigationService, licenseService, exportDocumentService, importDocumentService);
        }
    }
}
