using GalaSoft.MvvmLight;
using MyDocs.Common.Contract.Storage;
using System.Threading.Tasks;

namespace MyDocs.Common.Model
{
	public class Photo : ObservableObject
	{
		private IFile file;
		private IBitmapImage image;
		private IBitmapImage thumbnail;

		private bool imageGenerationStarted;
		private bool imageLoaded;
		private bool thumbnailGenerationStarted;
		private bool thumbnailLoaded;

		public Photo(IFile file)
		{
			this.file = file;
		}

		public IFile File
		{
			get { return file; }
			set
			{
				if (file != value) {
					file = value;
					RaisePropertyChanged(() => File);
					imageGenerationStarted = false;
					thumbnailGenerationStarted = false;
				}
			}
		}

		public IBitmapImage Image
		{
			get
			{
				var tmp = GenerateImageAsync();
				return image;
			}
			private set
			{
				if (image != value) {
					image = value;
					RaisePropertyChanged(() => Image);
				}
			}
		}

		public bool ImageLoaded
		{
			get { return imageLoaded; }
			private set
			{
				if (imageLoaded != value) {
					imageLoaded = value;
					RaisePropertyChanged(() => ImageLoaded);
				}
			}
		}

		public IBitmapImage Thumbnail
		{
			get
			{
				var tmp = GenerateThumbnailAsync();
				return thumbnail;
			}
			private set
			{
				if (thumbnail != value) {
					thumbnail = value;
					RaisePropertyChanged(() => Thumbnail);
				}
			}
		}

		public bool ThumbnailLoaded
		{
			get { return thumbnailLoaded; }
			private set
			{
				if (thumbnailLoaded != value) {
					thumbnailLoaded = value;
					RaisePropertyChanged(() => ThumbnailLoaded);
				}
			}
		}

		private async Task GenerateImageAsync()
		{
			if (!imageGenerationStarted) {
				imageGenerationStarted = true;
				Image = await file.GetResizedBitmapImageAsync(FileSize.BIG);
				ImageLoaded = true;
			}
		}

		private async Task GenerateThumbnailAsync()
		{
			if (!thumbnailGenerationStarted) {
				thumbnailGenerationStarted = true;
				Thumbnail = await file.GetResizedBitmapImageAsync(FileSize.SMALL);
				ThumbnailLoaded = true;
			}
		}
	}
}
