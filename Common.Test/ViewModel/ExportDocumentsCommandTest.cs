using FakeItEasy;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyDocs.Common.Contract.Page;
using MyDocs.Common.Contract.Service;
using MyDocs.Common.Model;
using System;

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
                // TODO find a better way to wait until the async command has finished
                System.Threading.Tasks.Task.Delay(TimeSpan.FromMilliseconds(500)).Wait();
                A.CallTo(() => exportDocumentsService.ExportDocuments()).MustNotHaveHappened();
            }
        }
    }
}
