using System.Xml.Serialization;

namespace ProxyMgr.ProxyManager.Utilities
{
    /// <summary>
    /// Event that triggered when proxy entered
    /// </summary>
    [XmlRoot(Namespace = "urn:proxymgr:scvmap", ElementName = "ProxyInfo")]
    public class ProxyEntryInfo
    {
        #region Fields
        /// <summary>
        /// Backing field for <see cref="Name"/>
        /// </summary>
        private string name;
        /// <summary>
        /// Backing field for <see cref="Url"/>
        /// </summary>
        private string url;
        /// <summary>
        /// Backing field for <see cref="GenerateClient"/>
        /// </summary>
        private bool generateClient;
        /// <summary>
        /// Backing field for <see cref="UseXmlSerializer"/>
        /// </summary>
        private bool useXmlSerializer;
        #endregion

        #region Properties
        /// <summary>
        /// Sets or gets the name entered for service proxy namespace
        /// </summary>
        [XmlElement]
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        /// <summary>
        /// Sets or gets the wsdl url which entered
        /// </summary>
        [XmlElement]
        public string Url
        {
            get { return url; }
            set { url = value; }
        }

        /// <summary>
        /// Sets or gets a flag for generating Client or not
        /// </summary>
        [XmlElement]
        public bool GenerateClient
        {
            get { return generateClient; }
            set { generateClient = value; }
        }

        /// <summary>
        /// Gets or sets a flag for generating code with xmlserializer or datacontractserializer
        /// </summary>
        [XmlElement]
        public bool UseXmlSerializer
        {
            get { return useXmlSerializer; }
            set { useXmlSerializer = value; }
        }
        #endregion
    }
}
