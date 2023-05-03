#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB.Visual;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;

#endregion


namespace Addin_Bootcamp_Challenge2
{

    [Transaction(TransactionMode.Manual)]
    public class Module02Challenge : IExternalCommand
    {
        public List<CurveElement> modelCurves = new List<CurveElement>();
        public ElementId levelId = new ElementId(0);

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // this is a variable for the Revit application
            UIApplication uiapp = commandData.Application;

            // this is a variable for the current Revit model
            Document doc = uiapp.ActiveUIDocument.Document;

            // this is a variable to get the active view
            View activeView = doc.ActiveView;


            // Prompt the user to select elements
            UIDocument uiDoc = uiapp.ActiveUIDocument;
            IList<Element> elementsList = uiDoc.Selection.PickElementsByRectangle("Select elements by rectangle");


            // Filter selection for model curves
            foreach (Element element in elementsList)
            {
                if (element is CurveElement)
                {
                    CurveElement curveElement = element as CurveElement;
                    if (curveElement.CurveElementType == CurveElementType.ModelCurve)
                    {
                        modelCurves.Add(curveElement);
                    }
                }
            }

            using (Transaction t = new Transaction(doc))
            {
                t.Start("Create message");

                // Get level data
                Level curLevel = activeView.GenLevel;
                levelId = curLevel.Id;

                // Create a wall
                WallType wallType1 = GetWallTypeByName(doc, "Storefront");
                WallType wallType2 = GetWallTypeByName(doc, "Generic - 8\"");

                Wall wallToAdd1 = addWall(doc, "A-GLAZ", wallType1);
                Wall wallToAdd2 = addWall(doc, "A-WALL", wallType2);

                //Get the Duct system types
                MEPSystemType ductSystemType = systemType(doc, "Supply Air");

                //Add ducts
                DuctType ductToAdd = DuctType(doc, "M-DUCT", ductSystemType);

                //Get the Pipe system types
                MEPSystemType pipeSystemType = systemType(doc, "Domestic Hot Water");

                //Create pipe
                PipeType pipeToAdd = pipeType(doc, "P-PIPE", pipeSystemType);

                t.Commit();
            }


            return Result.Succeeded;
        }

        // Method to get the wall type
        internal WallType GetWallTypeByName(Document doc, string typeName)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(WallType));

            foreach (WallType curWallType in collector)
            {
                if (curWallType.Name == typeName)
                    return curWallType;
            }

            return null;
        }

        internal Wall addWall (Document doc, string lineName, WallType wt)
        {
            foreach (CurveElement curveElement in modelCurves)
            {
                Curve wallCurve1 = curveElement.GeometryCurve;
                GraphicsStyle wallCurve1GS = curveElement.LineStyle as GraphicsStyle;
                if (wallCurve1GS.Name == lineName)
                {
                    switch (wallCurve1GS.Name)
                    {
                        case "A-GLAZ":
                            Wall.Create(doc, wallCurve1, wt.Id, levelId, 20, 0, false, false);
                            break;
                        case "A-WALL":
                            Wall.Create(doc, wallCurve1, wt.Id, levelId, 20, 0, false, false);
                            break;
                        default:
                            break;
                    }
                }

            }

            return null;
        }

        internal MEPSystemType systemType(Document doc, string sysName)
        {
            FilteredElementCollector systemType = new FilteredElementCollector(doc);
            systemType.OfClass(typeof(MEPSystemType));

            foreach (MEPSystemType curType in systemType)
            {
                if (curType.Name == sysName)
                {
                    return curType;
                }
            }

            return null;
        }

        internal DuctType DuctType(Document doc, string lineName, MEPSystemType ductSysType)
        {
            FilteredElementCollector ductTypes = new FilteredElementCollector(doc);
            ductTypes.OfClass(typeof(DuctType));

            foreach (CurveElement curveElement in modelCurves)
            {
                Curve ductCurve1 = curveElement.GeometryCurve;
                GraphicsStyle ductCurve1GS = curveElement.LineStyle as GraphicsStyle;
                if (ductCurve1GS.Name == lineName)
                {
                    Duct newDuct = Duct.Create(doc, ductSysType.Id, ductTypes.FirstElementId(), levelId, ductCurve1.GetEndPoint(0), ductCurve1.GetEndPoint(1));
                }
            }

            return null;
        }

        internal PipeType pipeType(Document doc, string lineName, MEPSystemType pipeSysType)
        {
            FilteredElementCollector pipeType = new FilteredElementCollector(doc);
            pipeType.OfClass(typeof(PipeType));

            foreach (CurveElement curveElement in modelCurves)
            {
                Curve pipeCurve1 = curveElement.GeometryCurve;
                GraphicsStyle pipeCurve1GS = curveElement.LineStyle as GraphicsStyle;
                if (pipeCurve1GS.Name == lineName)
                {
                    Pipe newPipe = Pipe.Create(doc, pipeSysType.Id, pipeType.FirstElementId(), levelId, pipeCurve1.GetEndPoint(0), pipeCurve1.GetEndPoint(1));
                }
            }

            return null;
        }

        public static String GetMethod()
        {
            var method = MethodBase.GetCurrentMethod().DeclaringType?.FullName;
            return method;
        }
    }
}
