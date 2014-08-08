using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using VMware.VIClient.Plugins2;

namespace sshAutoConnect
{
    public partial class ExtensionSeparator : ToolStripSeparator
    {
        Extension _associatedExtension;

        public ExtensionSeparator()
        {
            InitializeComponent();
        }

        public ExtensionSeparator(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }

        public ExtensionSeparator(Extension extension)
        {
            InitializeComponent();
            Init(extension);
        }

        private void Init(Extension extension)
        {
            if (extension != null)
            {
                _associatedExtension = extension;
            }
        }
    }
}
