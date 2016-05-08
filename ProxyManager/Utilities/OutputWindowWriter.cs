using System;
using System.IO;
using System.Text;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace ProxyMgr.ProxyManager.Utilities
{
    /// <summary>
    /// Manages writing to Output window pane
    /// </summary>
    internal class OutputWindowWriter : TextWriter
    {
        #region Constants
        /// <summary>
        /// Name of the custom output pane.
        /// </summary>
        private const string PaneName = "Proxy Generation";
        /// <summary>
        /// Guid for the custom output pane.
        /// </summary>
        private static readonly Guid PaneGuid = new Guid("AB9F45E4-2001-4197-BAF5-4B165222AF29");
        #endregion

        #region Fields
        /// <summary>
        /// Output window.
        /// </summary>
        private IVsOutputWindow outputWindow;
        /// <summary>
        /// Output window pane.
        /// </summary>
        private IVsOutputWindowPane outputPane;
        /// <summary>
        /// Parent package.
        /// </summary>
        private ProxyManagerPackage package;
        #endregion

        #region Properties
        /// <summary>
        /// Gets output window object.
        /// </summary>
        private IVsOutputWindow OutputWindow
        {
            get
            {
                if (outputWindow == null)
                {
                    DTE dte = (DTE)((package as IServiceProvider).GetService(typeof(DTE)));
                    IServiceProvider serviceProvider = new ServiceProvider(dte as Microsoft.VisualStudio.OLE.Interop.IServiceProvider);
                    outputWindow = serviceProvider.GetService(typeof(SVsOutputWindow)) as IVsOutputWindow;
                }

                return outputWindow;
            }
        }

        /// <summary>
        /// Returns output pane.
        /// </summary>
        private IVsOutputWindowPane OutputPane
        {
            get
            {
                if (outputPane == null)
                {
                    Guid generalPaneGuid = PaneGuid;
                    IVsOutputWindowPane pane;

                    OutputWindow.GetPane(ref generalPaneGuid, out pane);

                    if (pane == null)
                    {
                        OutputWindow.CreatePane(ref generalPaneGuid, PaneName, 1, 1);
                        OutputWindow.GetPane(ref generalPaneGuid, out pane);
                    }

                    outputPane = pane;
                }

                return outputPane;
            }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes new instance of the writer.
        /// </summary>
        /// <param name="package">Parent package.</param>
        public OutputWindowWriter(ProxyManagerPackage package)
        {
            if (package == null)
                throw new ArgumentNullException("package");

            this.package = package;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Writes a message into our output pane.
        /// </summary>
        /// <param name="message">Message to write.</param>
        public override void Write(string message)
        {
            OutputPane.OutputString(message);
        }

        /// <summary>
        /// Writes a character into our output pane.
        /// </summary>
        /// <param name="ch">Character to write.</param>
        public override void Write(char ch)
        {
            OutputPane.OutputString(ch.ToString());
        }

        /// <summary>
        /// Clears output pane.
        /// </summary>
        public void Clear()
        {
            OutputPane.Clear();
        }

        /// <summary>
        /// Gets the encoding
        /// </summary>
        public override Encoding Encoding
        {
            get
            {
                return Encoding.UTF8;
            }
        }
        #endregion
    }
}
