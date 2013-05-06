using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace MyDocs.Contract.Service
{
	public interface ISettingsService
	{
		StorageFolder PhotoFolder { get; }
		ApplicationDataContainer SettingsContainer { get; }
		bool IsSyncEnabled { get; set; }
	}
}
