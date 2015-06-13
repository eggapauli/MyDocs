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
        public void ImportDocumentsCommandShouldBeEnabled()
        {
            var sut = CreateSut();

            sut.ImportDocumentsCommand.CanExecute(null).Should().BeTrue();
        }

        [TestMethod]
        public void ImportDocumentsCommandShouldFailWhenUnlicensed()
        {
            var licenseService = A.Fake<ILicenseService>();
            var importDocumentService = A.Fake<IImportDocumentService>();
            A.CallTo(() => licenseService.Unlock("ExportImportDocuments")).Throws(new LicenseStatusException("Test", LicenseStatus.Locked));
            var sut = CreateSut(licenseService: licenseService, importDocumentService: importDocumentService);

            using (Fake.CreateScope())
            {
                sut.ImportDocumentsCommand.Execute(null);
                WaitForCommand();
                A.CallTo(() => importDocumentService.ImportDocuments()).MustNotHaveHappened();
            }
        }

        [TestMethod]
        public void ImportDocumentsCommandShouldSucceedWhenLicensed()
        {
            var importDocumentService = A.Fake<IImportDocumentService>();
            var sut = CreateSut(importDocumentService: importDocumentService);

            using (Fake.CreateScope())
            {
                sut.ImportDocumentsCommand.Execute(null);
                WaitForCommand();
                A.CallTo(() => importDocumentService.ImportDocuments()).MustHaveHappened();
            }
        }


        [TestMethod]
        public void ViewModelShouldBeBusyWhileImportingDocuments()
        {
            var tcs = new TaskCompletionSource<object>();
            var importDocumentService = A.Fake<IImportDocumentService>();
            A.CallTo(() => importDocumentService.ImportDocuments()).Returns(tcs.Task);
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
