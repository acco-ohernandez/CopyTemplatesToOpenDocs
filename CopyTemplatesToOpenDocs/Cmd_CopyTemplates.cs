#region Namespaces
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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


            // Gui for list of templates to get the selected templates list
            var ViewTemplatesList = new ObservableCollection<ViewTemplateData>(
                allViewTemplates.Select(vt => new ViewTemplateData(vt.Name, false)));

            // Open schedulesImport_Form1
            ListForm1 ViewTemplateListForm = new ListForm1(ViewTemplatesList)
            {
                Width = 600,
                Height = 650,
                WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen,
                Topmost = true,
            };

            ViewTemplateListForm.ShowDialog();

            List<ElementId> viewTemplateIDsList = new List<ElementId>();
            // Check if the user confirmed the selection
            if (ViewTemplateListForm.DialogResult == true)
            {

                // Update the selectedViewTemplates collection with user selections
                var selectedViewTemplates = new ObservableCollection<ViewTemplateData>(ViewTemplateListForm.selectedViewTemplates);

                // Collect the IDs of the selected templates
                foreach (ViewTemplateData vtData in selectedViewTemplates)
                {
                    Debug.Print($"TemplateName: {vtData.TemplateName}, IsSelected: {vtData.IsSelected} ======================");
                    if (vtData.IsSelected)
                    {
                        var viewTemplate = allViewTemplates.FirstOrDefault(vt => vt.Name == vtData.TemplateName);
                        if (viewTemplate != null)
                        {
                            viewTemplateIDsList.Add(viewTemplate.Id);
                        }
                    }
                }

                if (viewTemplateIDsList.Count == 0)
                {
                    TaskDialog.Show("Info", "No View Templates Selected \nNothing copied");
                    return Result.Cancelled;
                }

                // Rest of your code for copying templates...
            }
            else
            {
                return Result.Cancelled;
            }

            // Gui for list of documents
            // CODE HERE <============================
            // Get the selected documents list

            // Create a default CopyPasteOptions
            CopyPasteOptions cpOpts = new CopyPasteOptions();
            cpOpts.SetDuplicateTypeNamesHandler(new MyCopyHandler());

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
            Debug.Print($"View Templates Tatal: {allViewTemplates.Count()}");
            Debug.Print($"View Templates copied: {viewTemplateIDsList.Count()}");

            string result = string.Format("Revit model(s) updated: {0}\nView Templates Total: {1}\nView Templates copied: {2}",
                               titlesOfDoc.Count(),
                               allViewTemplates.Count(),
                               viewTemplateIDsList.Count());

            TaskDialog.Show("Copy Templates Result", result);


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
    public class MyCopyHandler : IDuplicateTypeNamesHandler
    {
        public DuplicateTypeAction OnDuplicateTypeNamesFound(DuplicateTypeNamesHandlerArgs args)
        {
            // You can decide how to handle duplicate types here.
            // For example, to skip duplicates and use the existing type, you can do:
            return DuplicateTypeAction.UseDestinationTypes;
        }
    }
    public class ViewTemplateData : INotifyPropertyChanged
    {
        private bool isSelected;
        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                if (isSelected != value)
                {
                    isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }

        private string templateName;
        public string TemplateName
        {
            get { return templateName; }
            set
            {
                if (templateName != value)
                {
                    templateName = value;
                    OnPropertyChanged(nameof(TemplateName));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ViewTemplateData(string templateName, bool isSelected)
        {
            TemplateName = templateName;
            IsSelected = isSelected;
        }
    }
}
