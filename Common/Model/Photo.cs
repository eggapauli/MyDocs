using GalaSoft.MvvmLight;
using MyDocs.Common.Contract.Storage;
using System.Threading.Tasks;

namespace MyDocs.Common.Model
{
	public class Photo : ObservableObject
	{
		private IFile file;

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
				}
			}
		}
	}
}
