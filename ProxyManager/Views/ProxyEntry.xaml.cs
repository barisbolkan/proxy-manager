using System;
using ProxyMgr.ProxyManager.ViewModel;

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
