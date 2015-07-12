using JsonNetDal;
using MyDocs.Common.Contract.Service;
using MyDocs.WindowsStore.Service;
using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;

namespace MyDocs.WindowsStore.Common
{
    public abstract class CancellableAsyncTask : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();
            using (var cts = new CancellationTokenSource())
            {
                var cancelSubscription = Observable.FromEventPattern<BackgroundTaskCanceledEventHandler, IBackgroundTaskInstance, BackgroundTaskCancellationReason>(
                    h => taskInstance.Canceled += h,
                    h => taskInstance.Canceled -= h)
                .Subscribe(_ => cts.Cancel());

                using (cancelSubscription)
                {
                    await Run(taskInstance, cts.Token);
                }
            }
            deferral.Complete();
        }

        public abstract Task Run(IBackgroundTaskInstance taskInstance, CancellationToken ct);
    }

    public sealed class CleanupDocumentTask : CancellableAsyncTask
    {
        public override async Task Run(IBackgroundTaskInstance taskInstance, CancellationToken ct)
        {
            IDocumentService documentService = new DocumentService(new JsonDocumentDb(new SubDocumentService()));
            await documentService.RemoveOutdatedDocuments();
        }
    }
}