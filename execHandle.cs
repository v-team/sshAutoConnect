using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace sshAutoConnect
{
    class execHandle
    {
        public void recreatePuttyResource()
        {
            Assembly currentAssembly = Assembly.GetExecutingAssembly();
            string[] arrResources = currentAssembly.GetManifestResourceNames();

            foreach (string resourceName in arrResources)
            {                
                if (resourceName == "sshAutoConnect.putty.exe")
                {
                    string saveAsName = resourceName;
                    FileInfo fileInfoOutputFile = new FileInfo(System.IO.Path.GetTempPath() + saveAsName);
                    if (!fileInfoOutputFile.Exists)
                    {
                        FileStream streamToOutputFile = fileInfoOutputFile.OpenWrite();
                        Stream streamToResourceFile = currentAssembly.GetManifestResourceStream(resourceName);

                        const int size = 4096;
                        byte[] bytes = new byte[4096];
                        int numBytes;
                        while ((numBytes = streamToResourceFile.Read(bytes, 0, size)) > 0)
                        {
                            streamToOutputFile.Write(bytes, 0, numBytes);
                        }

                        streamToOutputFile.Close();
                        streamToResourceFile.Close();
                    }
                }
            }
        }
    }
}
