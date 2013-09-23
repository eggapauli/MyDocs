using StorageContract = MyDocs.Common.Contract.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using MyDocs.Common.Model;
using Windows.UI.Xaml.Media.Imaging;
using MyDocs.Common;
using Windows.Storage;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace MyDocs.WindowsStore.Controls
{
    public sealed partial class FilePreview : UserControl
    {
        public static readonly DependencyProperty FileProperty =
            DependencyProperty.Register("File", typeof(Photo), typeof(FilePreview), new PropertyMetadata(null, FilePropertyChanged));

        public static readonly DependencyProperty FileSizeProperty =
            DependencyProperty.Register("FileSize", typeof(StorageContract.FileSize), typeof(FilePreview), new PropertyMetadata(StorageContract.FileSize.Small, FileSizePropertyChanged));

        public Photo File
        {
            get { return (Photo)GetValue(FileProperty); }
            set { SetValue(FileProperty, value); }
        }

        public StorageContract.FileSize FileSize
        {
            get { return (StorageContract.FileSize)GetValue(FileSizeProperty); }
            set { SetValue(FileSizeProperty, value); }
        }

        public FilePreview()
        {
            this.InitializeComponent();
        }

        private static async void FilePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = (FilePreview)d;
            var photo = (Photo)e.NewValue;

            if (photo != null) {
                using (new TemporaryState(() => self.Loading.IsActive = true, () => self.Loading.IsActive = false)) {
                    self.Preview.Source = (BitmapImage)(await photo.Preview.GetResizedBitmapImageAsync(self.FileSize)).Image;
                    await System.Threading.Tasks.Task.Delay(500); // image is not shown immediately
                }
            }
        }

        private static void FileSizePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = (FilePreview)d;
            var size = (StorageContract.FileSize)e.NewValue;
            if (size == StorageContract.FileSize.Big) {
                self.Loading.Width = 100;
                self.Loading.Height = 100;
            }
            else {
                self.Loading.Width = Double.NaN; // Auto
                self.Loading.Height = Double.NaN; // Auto
            }
        }
    }
}
