#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using static System.Net.Mime.MediaTypeNames;

#endregion

namespace Intermediate_Module_01
{
    [Transaction(TransactionMode.Manual)]
    public class Command1 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // this is a variable for the Revit application
            UIApplication uiapp = commandData.Application;

            // this is a variable for the current Revit model
            Document doc = uiapp.ActiveUIDocument.Document;

            List<string> departments = new List<string>();

            FilteredElementCollector roomCollector = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Rooms).WhereElementIsNotElementType();
            Element roomInstance = roomCollector.FirstElement();
            // Extracting parameters from collected room above
            Parameter roomNumberParameter = roomInstance.get_Parameter(BuiltInParameter.ROOM_NUMBER);
            //Parameter roomNumberParameter = roomInstance.LookupParameter("Number");
            Parameter roomNameParameter = roomInstance.get_Parameter(BuiltInParameter.ROOM_NAME);
            Parameter roomDepartmentParameter = roomInstance.get_Parameter(BuiltInParameter.ROOM_DEPARTMENT);
            Parameter roomCommentsParameter = roomInstance.LookupParameter("Comments");
            Parameter roomAreaParameter = roomInstance.get_Parameter(BuiltInParameter.ROOM_AREA);
            Parameter roomLevelParameter = roomInstance.LookupParameter("Level");
            //Parameter roomAreaParameter = roomInstance.LookupParameter("Area");


            foreach (Element currentRoom in  roomCollector)

            {
            //  Element roomInstance = roomCollector.FirstElement();
                Parameter roomDepartmentParameters = currentRoom.get_Parameter(BuiltInParameter.ROOM_DEPARTMENT);
                departments.Add(roomDepartmentParameters.AsValueString());
            }
            List<string> uniqueDepartments = departments.Distinct().ToList();
            uniqueDepartments.Sort();

            using (Transaction t = new Transaction(doc))
            {
                t.Start("Create Schedule");

                ElementId catId = new ElementId(BuiltInCategory.OST_Rooms);

                foreach (string currentUniqueDepartment in uniqueDepartments)
                {

                    ViewSchedule newSchedule = ViewSchedule.CreateSchedule(doc, catId);
                    newSchedule.Name = "Dept - "+ currentUniqueDepartment;

                    ScheduleField roomNumField = newSchedule.Definition.AddField(ScheduleFieldType.Instance, roomNumberParameter.Id);
                    ScheduleField roomNameField = newSchedule.Definition.AddField(ScheduleFieldType.Instance, roomNameParameter.Id);
                    ScheduleField roomDeptField = newSchedule.Definition.AddField(ScheduleFieldType.Instance, roomDepartmentParameter.Id);
                    ScheduleField roomCommentsField = newSchedule.Definition.AddField(ScheduleFieldType.Instance, roomCommentsParameter.Id);
                    ScheduleField roomAreaField = newSchedule.Definition.AddField(ScheduleFieldType.ViewBased, roomAreaParameter.Id);
                    ScheduleField roomLevelField = newSchedule.Definition.AddField(ScheduleFieldType.Instance, roomLevelParameter.Id);

                    ScheduleFilter deptFilter = new ScheduleFilter(roomDeptField.FieldId, ScheduleFilterType.Equal, currentUniqueDepartment);
                    newSchedule.Definition.AddFilter(deptFilter);

                    roomLevelField.IsHidden = true;
                    roomAreaField.DisplayType = ScheduleFieldDisplayType.Totals;
                    //roomAreaField.FieldIndex


                    ScheduleSortGroupField LevelSort = new ScheduleSortGroupField(roomLevelField.FieldId);
                    LevelSort.ShowHeader = true;
                    LevelSort.ShowFooter = true;
                    LevelSort.ShowBlankLine = true;
                    newSchedule.Definition.AddSortGroupField(LevelSort);

                    newSchedule.Definition.IsItemized = true;
                    newSchedule.Definition.ShowGrandTotal = true;

                }

                ViewSchedule TotalSchedule = ViewSchedule.CreateSchedule(doc, catId);
                TotalSchedule.Name = "All Departments";

                ScheduleField roomDeptField2 = TotalSchedule.Definition.AddField(ScheduleFieldType.Instance, roomDepartmentParameter.Id);
                ScheduleField roomAreaField2 = TotalSchedule.Definition.AddField(ScheduleFieldType.ViewBased, roomAreaParameter.Id);

                roomAreaField2.DisplayType = ScheduleFieldDisplayType.Totals;

                ScheduleSortGroupField DeptSort = new ScheduleSortGroupField(roomDeptField2.FieldId);
                DeptSort.ShowHeader = true;
                DeptSort.ShowFooter = true;
                DeptSort.ShowBlankLine = false;
                TotalSchedule.Definition.AddSortGroupField(DeptSort);

                TotalSchedule.Definition.IsItemized = false;
                TotalSchedule.Definition.ShowGrandTotal = true;


                t.Commit();
            }

            return Result.Succeeded;
        }
        internal static PushButtonData GetButtonData()
        {
            // use this method to define the properties for this command in the Revit ribbon
            string buttonInternalName = "btnCommand1";
            string buttonTitle = "Push";

            ButtonDataClass myButtonData1 = new ButtonDataClass(
                buttonInternalName,
                buttonTitle,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName,
                Properties.Resources.PushHere_32,
                Properties.Resources.PushHere_16,
                "This is a tooltip for Button 1");

            return myButtonData1.Data;
        }
    }
}
