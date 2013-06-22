using MyDocs.Common.Contract.Service;
using System;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace MyDocs.WindowsStoreFrontend.Service
{
	public class ModernUIService : IUserInterfaceService
	{
		private ITranslatorService translator;

		public ModernUIService(ITranslatorService translator)
		{
			this.translator = translator;
		}

		public async Task ShowErrorAsync(string msgKey)
		{
			string msg = translator.Translate(msgKey);
			if (String.IsNullOrEmpty(msg)) {
				msg = "An error occured.";
			}
			await new MessageDialog(msg).ShowAsync();
		}
	}
}
