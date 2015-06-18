using Common.Test.Mocks;
using FluentAssertions;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using MyDocs.Common.Model;
using System.Threading.Tasks;

namespace MyDocs.Common.Test.ViewModel
{
    public partial class DocumentViewModelTest
    {
        [TestMethod]
        public void ExportDocumentsCommandShouldBeEnabled()
        {
            var sut = CreateSut();

            sut.ExportDocumentsCommand.CanExecute(null).Should().BeTrue();
        }

        [TestMethod]
        public void ExportDocumentsCommandShouldFailWhenUnlicensed()
        {
            var licenseService = new LicenseServiceMock();
            licenseService.UnlockFunc = async featureName =>
            {
                if (featureName == "ExportImportDocuments")
                {
                    throw new LicenseStatusException("Test", LicenseStatus.Locked);
                }
                await Task.Yield();
            };
            var exportDocumentService = new ExportDocumentServiceMock();
            var exportedDocuments = false;
            exportDocumentService.ExportDocumentsFunc = async delegate 
            {
                exportedDocuments = true;
                await Task.Yield();
            };
            var sut = CreateSut(licenseService: licenseService, exportDocumentService: exportDocumentService);

            sut.ExportDocumentsCommand.Execute(null);
            WaitForCommand();
            exportedDocuments.Should().BeFalse();
        }

        [TestMethod]
        public void ExportDocumentsCommandShouldSucceedWhenLicensed()
        {
            var exportDocumentService = new ExportDocumentServiceMock();
            var exportedDocuments = false;
            exportDocumentService.ExportDocumentsFunc = async delegate
            {
                exportedDocuments = true;
                await Task.Yield();
            };
            var sut = CreateSut(exportDocumentService: exportDocumentService);

            sut.ExportDocumentsCommand.Execute(null);
            WaitForCommand();
            exportedDocuments.Should().BeTrue();
        }


        [TestMethod]
        public void ViewModelShouldBeBusyWhileExportingDocuments()
        {
            var tcs = new TaskCompletionSource<object>();
            var exportDocumentService = new ExportDocumentServiceMock();
            exportDocumentService.ExportDocumentsFunc = delegate { return tcs.Task; };
            var sut = CreateSut(exportDocumentService: exportDocumentService);

            sut.IsBusy.Should().BeFalse();
            sut.ExportDocumentsCommand.Execute(null);
            WaitForCommand();
            sut.IsBusy.Should().BeTrue();
            tcs.SetResult(null);
            WaitForCommand();
            sut.IsBusy.Should().BeFalse();
        }
    }
}
