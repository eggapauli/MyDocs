using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace MyDocs.WindowsStore.Common
{
	public static class VisualTreeHelperEx
	{
		public static IEnumerable<DependencyObject> GetChildren(DependencyObject parent)
		{
			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++) {
				var child = VisualTreeHelper.GetChild(parent, i);
				yield return child;

				foreach (var subChild in GetChildren(child)) {
					yield return subChild;
				}
			}
		}
	}
}
