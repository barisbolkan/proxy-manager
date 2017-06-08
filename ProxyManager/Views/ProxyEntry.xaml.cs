using Microsoft.Win32;
using ProxyMgr.ProxyManager.ViewModel;
using System;

namespace ProxyMgr.ProxyManager.Views
{
    /// <summary>
    /// Interaction logic for ProxyEntry.xaml
    /// </summary>
    public partial class ProxyEntry : Microsoft.VisualStudio.PlatformUI.DialogWindow
    {
        #region Fields
        /// <summary>
        /// Event handler for proxy entry
        /// </summary>
        public event EventHandler<ProxyEntryInformation> OnProxyEntered;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of <see cref="ProxyEntry"/> class
        /// </summary>
        internal ProxyEntry()
            : this(new ProxyEntryInformation())
        { }

        /// <summary>
        /// Initializes a new instance of <see cref="ProxyEntry"/> class with the given model
        /// </summary>
        /// <param name="vModel">View model to bind</param>
        internal ProxyEntry(ProxyEntryInformation vModel)
        {
            InitializeComponent();
            this.DataContext = vModel;
        }
        #endregion

        #region Events
        /// <summary>
        /// Triggered when OK button clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOK_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // Get handle of the proxy event 
            EventHandler<ProxyEntryInformation> handler = OnProxyEntered;

            // If anyone attached to event then fire it
            if (handler != null)
            {
                handler(this, (this.DataContext as ProxyEntryInformation));

                // Close the form!!!
                this.Close();
            }
        }

        private void btnFile_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ProxyEntryInformation proxyInformation = (this.DataContext as ProxyEntryInformation);
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Filter = "WSDL-XML Files|*.wsdl;*.xml",
                InitialDirectory = GetWSDLPath(proxyInformation.ServiceAddress)
            };
            if (openFileDialog.ShowDialog() == true)
            {
                (this.DataContext as ProxyEntryInformation).ServiceAddress = openFileDialog.FileName;
            }
        }

        private string GetWSDLPath(string inputPath)
        {
            string defaultPath = "C:\\";
            if (string.IsNullOrWhiteSpace(inputPath))
            {
                return defaultPath;
            }

            Uri serviceUri;
            bool addressCheck = Uri.TryCreate(inputPath, UriKind.Absolute, out serviceUri);
            if (addressCheck && serviceUri.IsFile)
            {
                return inputPath;
            }

            return defaultPath;
        }

        /// <summary>
        /// Triggered when CANCEL button clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancel_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.Close();
        }
        #endregion
    }
}
