using FakeItEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyDocs.Common.Contract.Service;
using MyDocs.Common.ViewModel;
using System;
using System.Threading.Tasks;

namespace MyDocs.Common.Test.ViewModel
{
    [TestClass]
    public partial class EditDocumentViewModelTest
    {
        private static EditDocumentViewModel CreateSut(
            IDocumentService documentService = null,
            INavigationService navigator = null,
            IUserInterfaceService uiService = null,
            ICameraService cameraService = null,
            IFileOpenPickerService filePicker = null,
            ISettingsService settingsService = null,
            IPageExtractor pageExtractor = null)
        {
            documentService = documentService ?? A.Fake<IDocumentService>();
            navigator = navigator ?? A.Fake<INavigationService>();
            uiService = uiService ?? A.Fake<IUserInterfaceService>();
            cameraService = cameraService ?? A.Fake<ICameraService>();
            filePicker = filePicker ?? A.Fake<IFileOpenPickerService>();
            settingsService = settingsService ?? A.Fake<ISettingsService>();
            pageExtractor = pageExtractor ?? A.Fake<IPageExtractor>();
            return new EditDocumentViewModel(documentService, navigator, uiService, cameraService, filePicker, settingsService, pageExtractor);
        }

        private void WaitForCommand()
        {
            Task.Delay(TimeSpan.FromMilliseconds(100)).Wait();
        }
    }
}
