using System;
using ProxyMgr.ProxyManager.Utilities;
using ProxyMgr.ProxyManager.ViewModel;

namespace ProxyMgr.ProxyManager
{
    /// <summary>
    /// Interaction logic for ProxyEntry.xaml
    /// </summary>
    public partial class ProxyEntry : Microsoft.VisualStudio.PlatformUI.DialogWindow
    {
        public event EventHandler<ProxyEventArgs> OnProxyEntered;

        public ProxyEntry()
        {
            InitializeComponent();
            this.DataContext = new ProxyEntryViewModel();
        }

        private void btnOK_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ProxyEntryViewModel context = this.DataContext as ProxyEntryViewModel;

            // Get handle of the proxy event 
            EventHandler<ProxyEventArgs> handler = OnProxyEntered;

            // If anyone attached to event then fire it
            if (handler != null)
            {
                handler(this, new ProxyEventArgs()
                {
                    Name = context.ServiceName,
                    Url = context.ServiceAddress,
                    GenerationType = context.GenerationType.DescriptionAttr()
                });

                // Close the form!!!
                this.Close();
            }
        }

        private void btnCancel_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.Close();
        }
    }

    public class ProxyEventArgs : EventArgs
    {
        private string name;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        private string url;

        public string Url
        {
            get { return url; }
            set { url = value; }
        }

        private string generationType;

        public string GenerationType
        {
            get { return generationType; }
            set { generationType = value; }
        }
    }
}
