using MyDocs.Common.Contract.Service;

namespace Common.Test.Mocks
{
    class TranslatorServiceMock : ITranslatorService
    {
        public string Translate(string key)
        {
            return key;
        }
    }
}
