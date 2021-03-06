﻿using Autofac;
using MyDocs.Common.Contract.Service;
using MyDocs.WindowsStore.ViewModel;
using System;
using Windows.UI.Xaml.Data;

namespace MyDocs.WindowsStore.Converters
{
    public class TranslationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var translator = ViewModelLocator.Container.Resolve<ITranslatorService>();
            string format = (string)parameter;
            return string.Format(translator.Translate(format), value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
