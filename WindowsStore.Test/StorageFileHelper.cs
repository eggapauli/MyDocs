using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace WindowsStore.Test
{
    public class StorageFileHelper
    {
        public static async Task DoWithTempFile(StorageFile file, Func<Task> action)
        {
            ExceptionDispatchInfo exInfo = null;
            try {
                await action();
            }
            catch (Exception e) {
                exInfo = ExceptionDispatchInfo.Capture(e);
            }

            await file.DeleteAsync();

            if (exInfo != null) {
                exInfo.Throw();
            }
        }
    }
}
