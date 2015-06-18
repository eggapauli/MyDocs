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
            documentService = documentService ?? new DocumentServiceMock(Enumerable.Empty<Document>());
            navigator = navigator ?? new NavigationServiceMock();
            uiService = uiService ?? new UserInterfaceServiceMock();
            cameraService = cameraService ?? new CameraServiceMock();
            filePicker = filePicker ?? new FileOpenPickerServiceMock();
            settingsService = settingsService ?? new SettingsServiceMock();
            pageExtractor = pageExtractor ?? new PageExtractorMock();
            return new EditDocumentViewModel(documentService, navigator, uiService, cameraService, filePicker, settingsService, pageExtractor);
        }

        private void WaitForCommand()
        {
            Task.Delay(TimeSpan.FromMilliseconds(100)).Wait();
        }
    }
}
