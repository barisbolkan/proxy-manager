using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Xml.Serialization;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace ProxyMgr.ProxyManager.Utilities
{
    /// <summary>
    /// Helper functions
    /// </summary>
    public static class Extensions
    {
        #region Serialization Extensions
        /// <summary>
        /// Serializes the given object to the filestream
        /// </summary>
        /// <param name="stream">Filestrem to serialize the object to</param>
        /// <param name="obj">Object to serialize</param>
        public static void Serialize(this FileStream stream, object obj)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(obj.GetType());
                serializer.Serialize(stream, obj);
            }
            catch (Exception ex)
            { }
        }

        /// <summary>
        /// Deserializes the object in the filestream 
        /// </summary>
        /// <typeparam name="T">Type of object to deserialize</typeparam>
        /// <param name="stream">Stream of the object</param>
        /// <returns>Deserialized object</returns>
        public static T Deserialize<T>(this FileStream stream)
        {
            T obj = default(T);

            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                obj = (T)serializer.Deserialize(stream);
            }
            catch (Exception ex)
            { }

            return obj;
        }
        #endregion

        #region DTE Extensions
        /// <summary>
        /// Gets the selected project item
        /// </summary>
        /// <param name="selectedHierarchy">Current hierarchy</param>
        /// <param name="itemId">item id</param>
        /// <returns>Selected project item</returns>
        public static ProjectItem GetSelectedProjectItem(this IVsHierarchy selectedHierarchy, uint itemId)
        {
            // Create handles
            object propertyValue = null;

            // Get selected project item
            selectedHierarchy.GetProperty(itemId, (int)__VSHPROPID.VSHPROPID_ExtObject, out propertyValue);

            // return...
            return propertyValue as ProjectItem;
        }

        /// <summary>
        /// Finds the interface element for given <see cref="CodeElements"/> instance
        /// </summary>
        /// <param name="elements"></param>
        /// <returns></returns>
        public static CodeInterface FindInterface(this CodeElements elements)
        {
            // Loop
            foreach (CodeElement element in elements)
            {
                // Get element as interface
                CodeInterface myInterface = element as CodeInterface;

                // if it is interface then return
                if (myInterface != null)
                    return myInterface;

                // Or recurse the clidren
                myInterface = FindInterface(element.Children);

                // if found return
                if (myInterface != null)
                    return myInterface;
            }

            // Oppss. We didnt find it
            return null;
        }

        /// <summary>
        /// Gets the <see cref="UIHierarchyItem"/> from dte with the given name
        /// </summary>
        /// <param name="hierarchyItems">Current hierarchy items to look for</param>
        /// <param name="name">Name of the item</param>
        /// <returns>null if not found otherwise returns the found item</returns>
        public static UIHierarchyItem GetHierarchyItem(this UIHierarchyItems hierarchyItems, string name)
        {
            // Get count
            int count = hierarchyItems.Count;

            // Loop 
            for (int i = 1; i <= count; i++)
            {
                // Get inner item
                UIHierarchyItem hierarchyItem = hierarchyItems.Item(i);

                if (hierarchyItem.Name.Equals(name))
                    return hierarchyItem;

                if (hierarchyItem.UIHierarchyItems.Count != 0)
                {
                    UIHierarchyItem childItem = hierarchyItem.UIHierarchyItems.GetHierarchyItem(name);

                    if (childItem != null)
                        return childItem;
                }
            }

            return null;
        }

        /// <summary>
        /// Retrieves the selected hierarchy item in the given solution
        /// </summary>
        /// <param name="solution">Current solution</param>
        /// <returns>Selected hierarchy</returns>
        public static IVsHierarchy GetSelectedHierarchy(this IVsSolution solution, out uint itemId)
        {
            // Set default item id
            itemId = VSConstants.VSITEMID_NIL;

            // Check for solution is null or not!
            if (solution == null)
                return null;

            // Create handles
            IVsMultiItemSelect multiItemSelect = null;
            IntPtr hierarchyPtr = IntPtr.Zero;
            IntPtr selectionContainerPtr = IntPtr.Zero;
            IVsHierarchy selectedHierarchy = null;
            Guid guidProjectID = Guid.Empty;

            // Get global service
            IVsMonitorSelection monitorSelection = Package.GetGlobalService(typeof(SVsShellMonitorSelection)) as IVsMonitorSelection;

            // Check monitor selection is null or not!
            if (monitorSelection == null)
                return null;

            try
            {
                // Retrieve selection
                int hr = monitorSelection.GetCurrentSelection(out hierarchyPtr, out itemId, out multiItemSelect, out selectionContainerPtr);

                // Check check check
                if (ErrorHandler.Failed(hr) || hierarchyPtr == IntPtr.Zero ||
                    itemId == VSConstants.VSITEMID_NIL || multiItemSelect != null ||
                    itemId == VSConstants.VSITEMID_ROOT)
                    return null;

                // Retrieve hierarchy
                if ((selectedHierarchy = Marshal.GetObjectForIUnknown(hierarchyPtr) as IVsHierarchy) == null)
                    return null;

                if (ErrorHandler.Failed(solution.GetGuidOfProject(selectedHierarchy, out guidProjectID)))
                    return null; // hierarchy is not a project inside the Solution if it does not have a ProjectID Guid

                // if we got this far then there is a single project item selected
                return selectedHierarchy;
            }
            finally
            {
                if (selectionContainerPtr != IntPtr.Zero)
                    Marshal.Release(selectionContainerPtr);

                if (hierarchyPtr != IntPtr.Zero)
                    Marshal.Release(hierarchyPtr);
            }
        }
        #endregion
    }
}