using MyDocs.Common.Contract.Service;
using System;
using Windows.Storage;

namespace Common.Test.Mocks
{
    class SettingsServiceMock : ISettingsService
    {
        public bool IsSyncEnabled
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public IStorageFolder PhotoFolder
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public ApplicationDataContainer SettingsContainer
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IStorageFolder TempFolder
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}
