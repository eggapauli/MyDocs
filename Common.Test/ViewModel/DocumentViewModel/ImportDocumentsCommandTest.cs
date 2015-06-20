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
        public void ImportDocumentsCommandShouldBeEnabled()
        {
            var sut = CreateSut();

            sut.ImportDocumentsCommand.CanExecute(null).Should().BeTrue();
        }

        [TestMethod]
        public void ImportDocumentsCommandShouldFailWhenUnlicensed()
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
            var importDocumentService = new ImportDocumentServiceMock();
            var importedDocuments = false;
            importDocumentService.ImportDocumentsFunc = async delegate
            {
                importedDocuments = true;
                await Task.Yield();
            };
            var sut = CreateSut(licenseService: licenseService, importDocumentService: importDocumentService);

            sut.ImportDocumentsCommand.Execute(null);
            WaitForCommand();
            importedDocuments.Should().BeFalse();
        }

        [TestMethod]
        public void ImportDocumentsCommandShouldSucceedWhenLicensed()
        {
            var importDocumentService = new ImportDocumentServiceMock();
            var importedDocuments = false;
            importDocumentService.ImportDocumentsFunc = async delegate
            {
                importedDocuments = true;
                await Task.Yield();
            };
            var sut = CreateSut(importDocumentService: importDocumentService);

            sut.ImportDocumentsCommand.Execute(null);
            WaitForCommand();
            importedDocuments.Should().BeTrue();
        }


        [TestMethod]
        public void ViewModelShouldBeBusyWhileImportingDocuments()
        {
            var tcs = new TaskCompletionSource<object>();
            var importDocumentService = new ImportDocumentServiceMock();
            importDocumentService.ImportDocumentsFunc = delegate { return tcs.Task; };
            var sut = CreateSut(importDocumentService: importDocumentService);

            sut.IsBusy.Should().BeFalse();
            sut.ImportDocumentsCommand.Execute(null);
            WaitForCommand();
            sut.IsBusy.Should().BeTrue();
            tcs.SetResult(null);
            WaitForCommand();
            sut.IsBusy.Should().BeFalse();
        }
    }
}
