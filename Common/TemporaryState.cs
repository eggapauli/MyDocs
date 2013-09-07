using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyDocs.Common
{
    public class TemporaryState : IDisposable
    {
        private Action stateRevertAction;
        private bool isDisposed;

        public TemporaryState(Action stateChangeAction, Action stateRevertAction)
        {
            this.stateRevertAction = stateRevertAction;
            stateChangeAction();
        }

        public void Dispose()
        {
            if (!isDisposed) {
                isDisposed = true;
                stateRevertAction();
            }
        }
    }
}
