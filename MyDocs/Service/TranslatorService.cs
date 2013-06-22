﻿using MyDocs.Common.Contract.Service;
using Windows.ApplicationModel.Resources;

namespace MyDocs.WindowsStoreFrontend.Service
{
	public class TranslatorService : ITranslatorService
	{
		private ResourceLoader rl = new ResourceLoader();

		public string Translate(string key)
		{
			return rl.GetString(key);
		}
	}
}
