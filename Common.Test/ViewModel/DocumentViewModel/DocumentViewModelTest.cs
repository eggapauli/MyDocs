using Common.Test.Mocks;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using MyDocs.Common.Contract.Service;
using MyDocs.Common.Model.Logic;
using MyDocs.Common.ViewModel;
using System;
using System.Linq;
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
            IImportDocumentService importDocumentService = null,
            IFileOpenPickerService fileOpenPickerService = null)
        {
            documentService = documentService ?? new DocumentServiceMock(Enumerable.Empty<Document>());
            uiService = uiService ?? new UserInterfaceServiceMock();
            navigationService = navigationService ?? new NavigationServiceMock();
            licenseService = licenseService ?? new LicenseServiceMock();
            exportDocumentService = exportDocumentService ?? new ExportDocumentServiceMock();
            importDocumentService = importDocumentService ?? new ImportDocumentServiceMock();
            fileOpenPickerService = fileOpenPickerService ?? new FileOpenPickerServiceMock();
            var sut = new DocumentViewModel(documentService, uiService, navigationService, licenseService, exportDocumentService, importDocumentService, fileOpenPickerService);
            return sut;
        }

        private void WaitForCommand()
        {
            Task.Delay(TimeSpan.FromMilliseconds(100)).Wait();
        }

        private DocumentServiceMock CreateDocumentServiceMock()
        {
            var docs = Enumerable.Range(1, 10)
                .Select(i => new Document(
                    Guid.NewGuid(),
                    "Category " + (i % 3),
                    DateTime.Today.AddHours(-i * 5),
                    TimeSpan.FromDays(i * 5),
                    i % 2 == 0,
                    Enumerable.Range(1, i).Select(j => "Tag " + j)
                ));
            return new DocumentServiceMock(docs);
        }
    }
}
