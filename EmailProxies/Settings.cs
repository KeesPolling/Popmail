using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Foundation.Collections;

namespace PopMail.EmailProxies
{
    internal class Settings
    {
        public const string projectContainerName = "EmailProxies";
        public const string ipContainerName = "Ip";
        internal void CreateEmailProxiesSettings(ApplicationDataContainer localRootContainer)
        {
            if (!localRootContainer.Containers.ContainsKey(projectContainerName))
            {
                localRootContainer.CreateContainer(projectContainerName, ApplicationDataCreateDisposition.Always);
            }
            var emailProxiesSettings = localRootContainer.Containers[projectContainerName];
            CreateIpSettings(emailProxiesSettings);
        }
        private void CreateIpSettings(ApplicationDataContainer emailProxiesContainer)
        {
            if (!emailProxiesContainer.Containers.ContainsKey(ipContainerName))
            {
                emailProxiesContainer.CreateContainer(ipContainerName, ApplicationDataCreateDisposition.Always);
            }
            var ipSettings = emailProxiesContainer.Containers[ipContainerName].Values;
            ipSettings["MinBufferSize"] = (uint)1024;
            ipSettings["MaxBufferSize"] = (uint)65536;
            ipSettings["LogResponse"] = (bool)true;  //TODO this mist be set to false!!!!
            ipSettings["Timeout"] = (uint)1000;
            }
        internal IPropertySet GetIpSettings()
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            if (!localSettings.Containers.ContainsKey(projectContainerName)) CreateEmailProxiesSettings(localSettings);
            if (!localSettings.Containers[projectContainerName].Containers.ContainsKey(ipContainerName))
                CreateIpSettings(localSettings.Containers[projectContainerName]);
            return localSettings.Containers[projectContainerName].Containers[ipContainerName].Values;
        }
    }
}
