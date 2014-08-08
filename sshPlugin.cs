using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using VMware.VIClient.Plugins2;
using System.Windows.Forms;

using MOR = VMware.VIClient.Plugins2.ManagedObjectReference;

namespace sshAutoConnect
{
    public class sshPlugin : Plugin
    {
        public VIApp _viApp;

        private SynchronizationContext _loaderSyncContext;
        private bool _unloaded = false;

        #region Plugin Members

        public void Load(VIApp app)
        {
            if (app == null)
            {
                throw new Exception("Plugin.Load() viApp is null. Failing load!");
            }
            _viApp = app;
            ExtensionPoint.VI_APP = _viApp;
            _loaderSyncContext = System.Threading.SynchronizationContext.Current;
            
            AddSeparatorMenu();
            AddConstrainedInventoryMenu();
                        
            _unloaded = false;
        }

        public void Unload()
        {
            _viApp = null;
            _loaderSyncContext = null;
            _unloaded = true;
            File.Delete(System.IO.Path.GetTempPath() + "sshAutoConnect.putty.exe");
        }

        void OnConstrainedMenuActivated(Extension sender)
        {
            OnAnyCallback();
            if (!(sender is ToolStripMenuItem))
            {
                sender.Content = new ExtensionPoint(sender);
                sender.Activated -= OnConstrainedMenuActivated;
            }
        }

        void OnConstrainedSeparator(Extension sender)
        {
            OnAnyCallback();
            if (!(sender is ToolStripMenuItem))
            {
                sender.Content = new ExtensionSeparator(sender);
            }
        }

        private void AddConstrainedInventoryMenu()
        {
            Extension ext = _viApp.NewExtension();
            InventoryDisplayConstraint invConstraint = ext.AddNewDisplayConstraint<InventoryDisplayConstraint>();
            invConstraint.DisplayStyles.Add(InventoryStyles.Physical);
            ext.Activated += new ExtensionEvent(OnConstrainedMenuActivated);
            
            ext.Name = "Constrained Menu";
            _viApp.AddExtension(ExtensionPoints.Menus.Inventory.Host, ext);
        }

        private void AddSeparatorMenu()
        {
            Extension ext = _viApp.NewExtension();
            InventoryDisplayConstraint invConstraint = ext.AddNewDisplayConstraint<InventoryDisplayConstraint>();
            invConstraint.DisplayStyles.Add(InventoryStyles.Physical);
            ext.Activated += new ExtensionEvent(OnConstrainedSeparator);
            ext.Name = "Separator";
            _viApp.AddExtension(ExtensionPoints.Menus.Inventory.Host, ext);
        }
        
        #endregion
        
        void OnAnyCallback()
        {
            if (_unloaded)
            {
                MessageBox.Show("Callback(Check client logs) even though the plug-in was unloaded!!");
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(1, true);
                StringBuilder sb = new StringBuilder("Callback after unload identified");
                sb.AppendLine("");
                sb.Append(st.ToString());
                System.Diagnostics.Debug.WriteLine(sb.ToString());
            }
        }
    }
}
