using System;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;

namespace ProxyMgr.ProxyManager.ViewModel
{
    /// <summary>
    /// Represents the view model class for <see cref="ProxyEntry.xaml.cs"/>
    /// </summary>
    [XmlRoot(Namespace = "urn:proxymgr:scvmap", ElementName = "ProxyInfo")]
    public class ProxyEntryInformation : INotifyPropertyChanged
    {
        #region Fields
        /// <summary>
        /// Backing field for <see cref="ButtonEnabled"/>
        /// </summary>
        private bool buttonEnabled;
        /// <summary>
        /// Backing field for <see cref="ServiceName"/>
        /// </summary>
        private string serviceName;
        /// <summary>
        /// Backing field for <see cref="ServiceAddress"/>
        /// </summary>
        private string serviceAddress;
        /// <summary>
        /// Backing field for <see cref="GenerateClient"/>
        /// </summary>
        private bool generateClient;
        /// <summary>
        /// Backing field for <see cref="IsAddContext"/>
        /// </summary>
        private bool isAddContext;
        /// <summary>
        /// Backing field for <see cref="ShowMissingMap"/>
        /// </summary>
        private bool showMissingMap;
        #endregion

        #region Properties
        /// <summary>
        /// Sets or gets the proxy generation method
        /// </summary>
        [XmlElement]
        public bool GenerateClient
        {
            get { return generateClient; }
            set
            {
                generateClient = value;
                OnNotifyPropertyChanged("GenerateClient");
            }
        }

        /// <summary>
        /// Sets or gets the continue button enablity
        /// </summary>
        [XmlIgnore]
        public bool ButtonEnabled
        {
            get
            {
                return buttonEnabled;
            }
            set
            {
                buttonEnabled = value;
                OnNotifyPropertyChanged("ButtonEnabled");
            }
        }

        /// <summary>
        /// Sets or gets the service namespace to add
        /// </summary>
        [XmlElement]
        public string ServiceName
        {
            get
            {
                return serviceName;
            }
            set
            {
                serviceName = value;
                OnNotifyPropertyChanged("ServiceName");
                Validate();
            }
        }

        /// <summary>
        /// Sets or gets the service wsdl address to generate the proxy classes
        /// </summary>
        [XmlElement]
        public string ServiceAddress
        {
            get
            {
                return serviceAddress;
            }
            set
            {
                serviceAddress = value;
                OnNotifyPropertyChanged("ServiceAddress");
                Validate();
            }
        }

        /// <summary>
        /// Sets or gets that the active window can be configure or add
        /// </summary>
        [XmlIgnore]
        public bool IsAddContext
        {
            get
            {
                return isAddContext;
            }
            set
            {
                isAddContext = value;
                OnNotifyPropertyChanged("IsAddContext");
            }
        }

        /// <summary>
        /// Gets or sets a value that shows message when the svcmap file not found
        /// </summary>
        [XmlIgnore]
        public bool ShowMissingMap
        {
            get
            {
                return showMissingMap;
            }
            set
            {
                showMissingMap = value;
                OnNotifyPropertyChanged("ShowMissingMap");
            }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Construction
        /// </summary>
        internal ProxyEntryInformation()
        {
            this.IsAddContext = true;
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Validates the user inputs
        /// </summary>
        private void Validate()
        {
            bool nameCheck = (!string.IsNullOrWhiteSpace(ServiceName) && !ServiceName.Any(Char.IsWhiteSpace) && ServiceName.Length > 3);
            Uri serviceUri;
            bool addressCheck = Uri.TryCreate(ServiceAddress, UriKind.Absolute, out serviceUri);

            ButtonEnabled = nameCheck && addressCheck;
        }

        /// <summary>
        /// Notifies that the property changed
        /// </summary>
        /// <param name="propertyName">Property name which changes occured</param>
        protected void OnNotifyPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handle = PropertyChanged;

            if (handle != null)
            {
                handle(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        #region INotifyPropertyChanged Members
        /// <summary>
        /// Event for notifying changes
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
    }
}
