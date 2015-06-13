using FakeItEasy;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyDocs.Common.Contract.Service;
using MyDocs.Common.Model;

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
            var exportDocumentsService = A.Fake<IExportDocumentService>();
            A.CallTo(() => licenseService.Unlock(A<string>._)).Throws(new LicenseStatusException("Test", LicenseStatus.Locked));
            var sut = CreateSut(licenseService: licenseService, exportDocumentService: exportDocumentsService);

            using (Fake.CreateScope())
            {
                sut.ExportDocumentsCommand.Execute(null);
                WaitForCommand();
                A.CallTo(() => exportDocumentService.ExportDocuments()).MustNotHaveHappened();
            }
        }
    }
}
