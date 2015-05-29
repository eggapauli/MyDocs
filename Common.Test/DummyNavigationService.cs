using MyDocs.Common.Contract.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyDocs.Common.Test
{
    public class DummyNavigationService : INavigationService
    {
        public bool CanGoBack
        {
            get { throw new NotImplementedException(); }
        }

        public void GoBack()
        {
            throw new NotImplementedException();
        }

        public void Navigate<T>()
        {
            throw new NotImplementedException();
        }

        public void Navigate<T>(object parameter)
        {
            throw new NotImplementedException();
        }
    }
}
