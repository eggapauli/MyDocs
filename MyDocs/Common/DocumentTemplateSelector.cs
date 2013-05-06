using MyDocs.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MyDocs.Common
{
	public class DocumentTemplateSelector : DataTemplateSelector
	{
		public DataTemplate DocumentTemplate { get; set; }
		public DataTemplate AdTemplate { get; set; }

		protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
		{
			if (item is AdDocument) {
				return AdTemplate;
			}

			return DocumentTemplate;
		}
	}
}
