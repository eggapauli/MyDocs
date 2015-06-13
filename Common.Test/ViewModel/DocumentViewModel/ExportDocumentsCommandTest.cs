using FakeItEasy;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyDocs.Common.Contract.Service;
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
            var licenseService = A.Fake<ILicenseService>();
            var exportDocumentService = A.Fake<IExportDocumentService>();
            A.CallTo(() => licenseService.Unlock("ExportImportDocuments")).Throws(new LicenseStatusException("Test", LicenseStatus.Locked));
            var sut = CreateSut(licenseService: licenseService, exportDocumentService: exportDocumentService);

            using (Fake.CreateScope())
            {
                sut.ExportDocumentsCommand.Execute(null);
                WaitForCommand();
                A.CallTo(() => exportDocumentService.ExportDocuments()).MustNotHaveHappened();
            }
        }

        [TestMethod]
        public void ExportDocumentsCommandShouldSucceedWhenLicensed()
        {
            var exportDocumentService = A.Fake<IExportDocumentService>();
            var sut = CreateSut(exportDocumentService: exportDocumentService);

            using (Fake.CreateScope())
            {
                sut.ExportDocumentsCommand.Execute(null);
                WaitForCommand();
                A.CallTo(() => exportDocumentService.ExportDocuments()).MustHaveHappened();
            }
        }


        [TestMethod]
        public void ViewModelShouldBeBusyWhileExportingDocuments()
        {
            var tcs = new TaskCompletionSource<object>();
            var exportDocumentService = A.Fake<IExportDocumentService>();
            A.CallTo(() => exportDocumentService.ExportDocuments()).Returns(tcs.Task);
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
