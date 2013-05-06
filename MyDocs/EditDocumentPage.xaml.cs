using MyDocs.Model;
using MyDocs.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace MyDocs
{
	public sealed partial class EditDocumentPage : MyDocs.Common.LayoutAwarePage
	{
		public EditDocumentPage()
		{
			this.InitializeComponent();
		}

		public EditDocumentViewModel ViewModel
		{
			get { return this.DataContext as EditDocumentViewModel; }
		}

		protected override void LoadState(object navigationParameter, Dictionary<string, object> pageState)
		{
			Document doc = navigationParameter as Document;
			ViewModel.EditingDocument = doc;
		}
	}
}
