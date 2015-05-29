using MyDocs.Common.Contract.Service;
using System;

namespace MyDocs.Common.Test
{
    public class DummyTranslatorService : ITranslatorService
    {
        public string Translate(string key)
        {
            return key;
        }
    }
}
