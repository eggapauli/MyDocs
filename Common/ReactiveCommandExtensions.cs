using ReactiveUI;
using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace MyDocs.Common
{
    public static class ReactiveCommandExtensions
    {
        public static ReactiveCommand<Unit> CreateCommand(this ICanBeBusy self, Action<object> execute)
        {
            return self.CreateAsyncCommand(async o => { execute(o); await Task.Yield(); }, Observable.Return(true));
        }

        public static ReactiveCommand<Unit> CreateCommand(this ICanBeBusy self, Action<object> execute, IObservable<bool> canExecute)
        {
            return self.CreateAsyncCommand(async o => { execute(o); await Task.Yield(); }, canExecute);
        }

        public static ReactiveCommand<Unit> CreateAsyncCommand(this ICanBeBusy self, Func<object, Task> execute)
        {
            return self.CreateAsyncCommand(execute, Observable.Return(true));
        }

        public static ReactiveCommand<Unit> CreateAsyncCommand(this ICanBeBusy self, Func<object, Task> execute, IObservable<bool> canExecute)
        {
            Func<object, Task> busyExecute = async o =>
            {
                self.IsBusy = true;
                using (Disposable.Create(() => self.IsBusy = false))
                {
                    await execute(o);
                }
            };
            return ReactiveCommand.CreateAsyncTask(canExecute, busyExecute);
        }
    }
}
