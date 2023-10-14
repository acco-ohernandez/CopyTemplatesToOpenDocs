#region Namespaces
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows.Shapes;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

#endregion

namespace CopyTemplatesToOpenDocs
{
    [Transaction(TransactionMode.Manual)]
    public class Cmd_ReloadAddIn : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // this is a variable for the Revit application
            UIApplication uiapp = commandData.Application;

            // this is a variable for the current Revit model
            Document doc = uiapp.ActiveUIDocument.Document;

            // Get the path of the executing assembly (DLL)
            string assembly_Dll_File = Assembly.GetExecutingAssembly().Location;
            //var assembly_NameWithExtensionName = Assembly.GetExecutingAssembly().ManifestModule.Name;
            var assembly_NameWithNoExtensionName = Assembly.GetExecutingAssembly().GetName().Name;


            // Now, assembly_Name will contain the file name of the DLL.
            //if (false) return Result.Cancelled;

            // Assuming your assembly name is "MyRevitAddin.dll"
            System.AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.GetName().Name == assembly_NameWithNoExtensionName)
                .ToList()
                .ForEach(a => System.AppDomain.CurrentDomain.GetAssemblies().Where(b => b == a).ToList().ForEach(b => System.AppDomain.CurrentDomain.GetAssemblies().ToList().Remove(b)));

            // Assuming the path to your DLL is "C:\Path\To\MyRevitAddin.dll"
            System.Reflection.Assembly assembly = System.Reflection.Assembly.LoadFile(Assembly.GetExecutingAssembly().Location);

            TaskDialog.Show("info", "Hi");

            return Result.Succeeded;
        }
        internal static PushButtonData GetButtonData()
        {
            // use this method to define the properties for this command in the Revit ribbon
            string buttonInternalName = "btnReloadAddin";
            string buttonTitle = "Reload Ribbon";

            ButtonDataClass myButtonData1 = new ButtonDataClass(
                buttonInternalName,
                buttonTitle,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName,
                Properties.Resources.Blue_32,
                Properties.Resources.Blue_16,
                "Currently not working, it's in the testing phase. This will reload the this addin");

            return myButtonData1.Data;
        }
    }
}
