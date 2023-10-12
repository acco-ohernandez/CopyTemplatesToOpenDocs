#region Namespaces
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

#endregion

namespace CopyTemplatesToOpenDocs
{
    [Transaction(TransactionMode.Manual)]
    public class Cmd_CopyTemplates : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // This is a variable for the Revit application
            UIApplication uiapp = commandData.Application;

            // This is a variable for the current Revit model
            Document doc = uiapp.ActiveUIDocument.Document;

            // Referenced similar code: https://boostyourbim.wordpress.com/2013/07/26/transferring-just-one-view-template-from-project-to-project/

            // Get the path of the current document
            string currentDocPath = doc.PathName;

            // Get all other open documents, excluding the current document based on the path
            var openDocs = uiapp.Application.Documents.Cast<Document>()
                .Where(d => d.PathName != currentDocPath)
                .ToList(); // ToList() ensures we have a list we can iterate through

            if (openDocs.Count == 0)
            {
                TaskDialog.Show("Info", "No other opened documents.");
                return Result.Cancelled;
            }

            // Get all the ViewTemplate IDs
            var allViewTemplates = GetAllViewTemplates(doc);
            if (allViewTemplates.Count() == 0)
            {
                TaskDialog.Show("Info", "The Current document has not ViewTemplates.");
                return Result.Cancelled;
            }

            //// Get the id of the view template assigned to the active view
            //ElementId templateId = doc.ActiveView.ViewTemplateId;
            //if (templateId == ElementId.InvalidElementId)
            //{
            //    TaskDialog.Show("Error", "Active view must have a view template assigned.");
            //    return Result.Failed;
            //}

            //// Add the template id to a collection
            //ICollection<ElementId> copyIds = new Collection<ElementId>();
            //copyIds.Add(templateId);

            // Create a default CopyPasteOptions
            CopyPasteOptions cpOpts = new CopyPasteOptions();

            // Create a new transaction in each of the other documents and copy the template
            foreach (Document otherDoc in openDocs)
            {
                using (Transaction t = new Transaction(otherDoc, "Copy View Template"))
                {
                    t.Start();

                    // Perform the copy into the other document using ElementTransformUtils
                    //ElementTransformUtils.CopyElements(doc, copyIds, otherDoc, Transform.Identity, cpOpts);
                    ElementTransformUtils.CopyElements(doc, allViewTemplates, otherDoc, Transform.Identity, cpOpts);

                    t.Commit();
                }
            }

            return Result.Succeeded;
        }

        private List<ElementId> GetAllViewTemplates(Document doc)
        {

            // Get all View Templates in the current document
            FilteredElementCollector collector = new FilteredElementCollector(doc).OfClass(typeof(View));

            // Filter for View Templates (they are of type View)
            var viewTemplates = collector.OfType<View>().Where(view => view.IsTemplate).ToList();

            // Check if there are any View Templates
            if (viewTemplates.Count == 0)
            {
                TaskDialog.Show("Error", "There are no View Templates in the current document.");
                return null;
            }

            // Now viewTemplates is a collection of View Templates in the current document
            // You can loop through these templates and copy them to other documents as needed
            List<ElementId> viewTemplateIds = new List<ElementId>();
            foreach (var template in viewTemplates)
            {
                //ElementId templateId = template.Id;
                viewTemplateIds.Add(template.Id);

                // Rest of your code to copy this template to other documents
            }
            return viewTemplateIds;
        }

        internal static PushButtonData GetButtonData()
        {
            // use this method to define the properties for this command in the Revit ribbon
            string buttonInternalName = "btnCopyTemplates";
            string buttonTitle = "Copy Templates";

            ButtonDataClass myButtonData1 = new ButtonDataClass(
                buttonInternalName,
                buttonTitle,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName,
                Properties.Resources.Blue_32,
                Properties.Resources.Blue_16,
                "This will copy templates from the current document to selected open documents");

            return myButtonData1.Data;
        }
    }
}
