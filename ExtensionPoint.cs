using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using VMware.VIClient.Plugins2;
using System.Xml;

using MOR = VMware.VIClient.Plugins2.ManagedObjectReference;

namespace sshAutoConnect
{
    public partial class ExtensionPoint : ToolStripMenuItem
    {
        const string BASE_NAME = "ssh AutoConnect";
        public static VIApp VI_APP = null;
        Extension _associatedExtension;

        public ExtensionPoint()
        {
            InitializeComponent();
        }

        public ExtensionPoint(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }

        public ExtensionPoint(Extension extension)
        {
            InitializeComponent();
            Init(extension);
            this.Name = BASE_NAME;
            this.Image = Properties.Resources.logo_putty;
            this.Text = this.Name;
        }

        private void Init(Extension extension)
        {
            if (extension != null)
            {
                _associatedExtension = extension;
            }
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            string displayString = GetESXhostname();

            if (displayString == "-1")
            {
                MessageBox.Show("Unable to get hostname moref");
            }
            else
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                execHandle exeHdl = new execHandle();
                exeHdl.recreatePuttyResource();
                
                startInfo.FileName = System.IO.Path.GetTempPath() + "sshAutoConnect.putty.exe";
                string argText = "-ssh " + displayString;
                string login = "";
                string password = "";

                Hashtable credentials = GetCredentialFromHostname(displayString);

                if (credentials.ContainsKey("login"))
                {
                    login = credentials["login"].ToString();

                    if (credentials.ContainsKey("password"))
                    {
                        password = credentials["password"].ToString();
                    }
                }

                if (login.Length > 0)
                {
                    argText = argText + " -l " + login;

                    if (password.Length > 0)
                    {
                        argText = argText + " -pw " + password;
                    }
                }

                startInfo.Arguments = argText;
                Process.Start(startInfo);
            }
        }

        private string GetESXhostname()
        {
            if (_associatedExtension == null || _associatedExtension.Context == null || _associatedExtension.Context.Object == null)
            {
                return "-1";
            }

            string objHostName = string.Empty;
            try
            {
                if (_associatedExtension.Context.Object is IList)
                {
                    IList list = (IList)_associatedExtension.Context.Object;
                    if (list[0] is MOR)
                    {
                        objHostName = VI_APP.Inventory.GetName((MOR)list[0]);
                    }
                    else
                    {
                        return "-1";
                    }
                }
                return objHostName;
            }
            catch (Exception) { return "-1"; }
        }

        private Hashtable GetCredentialFromHostname(string hostname)
        {
            Hashtable credentials = new Hashtable();

            try
            {
                string absPathClass = Assembly.GetExecutingAssembly().CodeBase;
                string absPathPlugin = absPathClass.Substring(0, absPathClass.LastIndexOf("/")) + "/";
                Boolean credsOK = false;
                
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(System.IO.Path.Combine(absPathPlugin, "sshAutoConnect.xml"));

                XmlNodeList nodelist = xmlDoc.GetElementsByTagName("server");

                foreach (XmlNode node in nodelist)
                {
                    XmlElement serverElement = (XmlElement)node;

                    if (serverElement.HasAttributes && (serverElement.Attributes["name"].InnerText == hostname))
                    {
                        credentials.Add("login", serverElement.GetElementsByTagName("login")[0].InnerText);
                        credentials.Add("password", DecodeFrom64(serverElement.GetElementsByTagName("password")[0].InnerText));
                        credsOK = true;
                        break;
                    }
                }

                if (!credsOK)
                {
                    credentials.Add("login", xmlDoc["credentials"]["default"]["login"].InnerText);
                    credentials.Add("password", DecodeFrom64(xmlDoc["credentials"]["default"]["password"].InnerText));
                }

                return credentials;
            }
            catch (Exception) { return credentials; }
        }
        
        static public string EncodeTo64(string toEncode)
        {
            byte[] toEncodeAsBytes = System.Text.ASCIIEncoding.ASCII.GetBytes(toEncode);
            string returnValue = System.Convert.ToBase64String(toEncodeAsBytes);
            return returnValue;
        }

        static public string DecodeFrom64(string encodedData)
        {
            byte[] encodedDataAsBytes = System.Convert.FromBase64String(encodedData);
            string returnValue = System.Text.ASCIIEncoding.ASCII.GetString(encodedDataAsBytes);
            return returnValue;
        }
    }
}
