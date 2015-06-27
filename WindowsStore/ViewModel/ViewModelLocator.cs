using Autofac;
using MyDocs.Common.Contract.Service;
using MyDocs.WindowsStore.Service;
using MyDocs.WindowsStore.Service.Design;
using Splat;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MyDocs.WindowsStore.ViewModel
{
    public static class ViewModelLocator
    {
        private static readonly IContainer container;

        public static IContainer Container
        {
            get { return container; }
        }

        static ViewModelLocator()
        {
            var builder = new ContainerBuilder();
            if (ModeDetector.InDesignMode())
            {
                builder.RegisterType<DesignDocumentService>()
                    .As<IDocumentService>();
            }
            else
            {
                builder.RegisterType<DocumentService>()
                    .As<IDocumentService>();
            }

            var manuallyAddedTypes = new[]
            {
                typeof(DesignDocumentService),
                typeof(DocumentService),
                typeof(PageExtractorListService),
                typeof(PdfPageExtractorService),
                typeof(ImagePageExtractorService),
            };

            var thisAssembly = typeof(ViewModelLocator).GetTypeInfo().Assembly;

            builder.RegisterAssemblyTypes(thisAssembly)
                .Where(t => t.Name.EndsWith("Service"))
                .Where(t => !manuallyAddedTypes.Contains(t))
                .AsImplementedInterfaces()
                .SingleInstance();

            builder.Register(_ => new PageExtractorListService(GetPageExtractors()))
                .As<IPageExtractorService>()
                .SingleInstance();

            builder.RegisterType<ApplicationDataContainerDocumentStorageService>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<JsonNetDal.JsonDocumentDb>()
                .AsSelf()
                .As<IDocumentDb>()
                .SingleInstance();

            builder.RegisterAssemblyTypes(thisAssembly)
                .Where(t => t.Name.EndsWith("Page"))
                .AsImplementedInterfaces()
                .SingleInstance();

            builder.RegisterAssemblyTypes(thisAssembly)
                .Where(t => t.Name.EndsWith("ViewModel"))
                .AsSelf()
                .InstancePerDependency();

            container = builder.Build();
        }

        private static IEnumerable<IPageExtractorService> GetPageExtractors()
        {
            yield return new PdfPageExtractorService();
            yield return new ImagePageExtractorService();
        }

        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}