using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MyDocs.Common.Contract.Service;
using MyDocs.Common.Model;
using Serializable = MyDocs.Common.Model.Serializable;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization;
using System.IO;
using System.Threading.Tasks;

namespace MyDocs.Common.ViewModel
{
    public class SettingsViewModel : ViewModelBase
    {
        private ISettingsService settingsService;

        public bool SyncEnabled
        {
            get { return settingsService.IsSyncEnabled; }
            set
            {
                if (settingsService.IsSyncEnabled != value) {
                    settingsService.IsSyncEnabled = value;
                    RaisePropertyChanged();
                }
            }
        }
        public SettingsViewModel(ISettingsService settingsService)
        {
            this.settingsService = settingsService;
        }
    }
}
