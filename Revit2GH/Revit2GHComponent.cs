using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using RhinoInside.Revit;

// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace Revit2GH
{
    public class Revit2GHComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public Revit2GHComponent()
          : base("Revit2GH", "Nickname",
              "Description",
              "Category", "Subcategory")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Load", "_Load", "Set True or False", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Geometry", "Geometry", "Geometry from Revit", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var ifLoad = true;
            DA.GetData(0, ref ifLoad);
            //var outputGeos = new List<Brep>();


            if (!ifLoad)
            {
                DA.SetDataList(0, ghbreps);
            }
                
            else
            {
                Revit.EnqueueAction((uido) => GetSelectedRevitElements(uido));
                
            }
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("0d85cd72-3bd8-40b8-81ab-eb05428ef505"); }
        }

        List<GH_Brep> ghbreps = new List<GH_Brep>();
        List<string> outVals = new List<string>();

        void GetSelectedRevitElements(UIDocument uidoc)
        {
            Selection selection = uidoc.Selection;
            ICollection<ElementId> selectedIds = uidoc.Selection.GetElementIds();
            //outVals.Add("Num Selection: " + selectedIds.Count.ToString());
            ghbreps = new List<GH_Brep>();
            foreach (ElementId id in selectedIds)
            {
                Autodesk.Revit.DB.Options opt = new Options();
                Autodesk.Revit.DB.GeometryElement geomElem = uidoc.Document.GetElement(id).get_Geometry(opt);

                //outVals.Add("Num Geo: " + geomElem.Count().ToString());
                foreach (GeometryObject geomObj in geomElem)
                {
                    var facePtList = new List<List<Point3d>>();
                    
                    Solid geomSolid = geomObj as Solid;
                    var breps = geomSolid.ToRhino();
                    foreach (var item in breps)
                    {
                        var ghB = new GH_Brep(item);

                        ghbreps.Add(ghB);

                    }


                    //outputs = facePtList;

                }


                //TaskDialog.Show("Revit", faceInfo);
                //ids.Add(id.IntegerValue);
            }



        }


    }
}
