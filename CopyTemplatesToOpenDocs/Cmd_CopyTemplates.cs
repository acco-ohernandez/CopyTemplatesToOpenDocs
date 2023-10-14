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

using CopyTemplatesToOpenDocs.Forms;

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
            if (allViewTemplates == null)
                return Result.Cancelled;

            // Open schedulesImport_Form1
            ListForm1 ViewTemplateListForm = new ListForm1()
            {
                Width = 500,
                Height = 600,
                WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen,
                Topmost = true,
            };

            // Gui for list of templates
            // CODE HERE<============================
            // Get the selected templates list
            // Pass the list schedules to the form data grid
            ViewTemplateListForm.dataGrid.ItemsSource = allViewTemplates.Select(vt => vt.Name).ToList();
            ViewTemplateListForm.ShowDialog();
            //var allViewTemplateIDs = allViewTemplates.Select(vt => vt.Id).ToList();
            int count = 0;
            List<ElementId> viewTemplateIDsList = ViewTemplateListForm.DialogResult == true
                                        ? ViewTemplateListForm.dataGrid.SelectedItems.Cast<string>()
                                            .Select(curViewTemplateName =>
                                            {
                                                var viewTemplate = allViewTemplates.FirstOrDefault(vt => vt.Name == curViewTemplateName);
                                                return viewTemplate.Id;
                                            })
                                            .Where(id => id != null)
                                            .ToList()
                                        : new List<ElementId>(); // Store as a list of integers, not ElementIds

            //M_MyTaskDialog("Info", $"{count} Schedule(s) Imported Successfully");



            // Gui for list of documents
            // CODE HERE <============================
            // Get the selected documents list


            // Create a default CopyPasteOptions
            CopyPasteOptions cpOpts = new CopyPasteOptions();

            List<string> titlesOfDoc = new List<string>();

            // Create a new transaction in each of the other documents and copy the template
            foreach (Document otherDoc in openDocs)
            {
                // Get the name of the current doc
                titlesOfDoc.Add(otherDoc.Title);

                using (Transaction t = new Transaction(otherDoc, "Copy View Template"))
                {
                    t.Start();

                    // Perform the copy into the other document using ElementTransformUtils
                    //ElementTransformUtils.CopyElements(doc, allViewTemplates, otherDoc, Transform.Identity, cpOpts);
                    ElementTransformUtils.CopyElements(doc, viewTemplateIDsList, otherDoc, Transform.Identity, cpOpts);

                    t.Commit();
                }
            }

            Debug.Print($"Revit model(s) updated: {titlesOfDoc.Count()}");
            Debug.Print($"View Templates copied: {allViewTemplates.Count()}");



            return Result.Succeeded;
        }
        private List<View> GetAllViewTemplates(Document doc)
        {
            // Get all View Templates in the current document
            FilteredElementCollector collector = new FilteredElementCollector(doc).OfClass(typeof(View));

            // Filter for View Templates (they are of type View)
            var viewTemplates = collector.OfType<View>().Where(view => view.IsTemplate).OrderBy(v => v.Name).ToList();

            // Check if there are any View Templates
            if (viewTemplates.Count == 0)
            {
                TaskDialog.Show("Error", "There are no View Templates in the current document.");
                return null;
            }

            // Now viewTemplates is a collection of View Templates in the current document
            return viewTemplates;
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
