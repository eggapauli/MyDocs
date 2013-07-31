using MyDocs.Common.Contract.Storage;
using Windows.UI.Xaml.Media.Imaging;

namespace MyDocs.WindowsStore.Storage
{
	public class WindowsStoreBitmapImage : IBitmapImage
	{
		public object Image { get; private set; }
		public string FileName { get; private set; }

		public WindowsStoreBitmapImage(BitmapImage image, string fileName)
		{
			Image = image;
			FileName = fileName;
		}
	}
}
