using MyDocs.Common.Contract.Storage;
using Windows.UI.Xaml.Media.Imaging;

namespace MyDocs.WindowsStore.Storage
{
	public class WindowsStoreBitmapImage : IBitmapImage
	{
		public BitmapImage Image { get; private set; }

		public WindowsStoreBitmapImage(BitmapImage image)
		{
			Image = image;
		}
	}
}
