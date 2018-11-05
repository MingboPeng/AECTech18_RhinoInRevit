using System;
using System.Collections;
using System.Collections.Generic;

using Rhino;
using Rhino.Geometry;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.UI;
using RhinoInside.Revit;
using System.Linq;


/// <summary>
/// This class will be instantiated on demand by the Script component.
/// </summary>
public class Script_Instance : GH_ScriptInstance
{
#region Utility functions
  /// <summary>Print a String to the [Out] Parameter of the Script component.</summary>
  /// <param name="text">String to print.</param>
  private void Print(string text) { __out.Add(text); }
  /// <summary>Print a formatted String to the [Out] Parameter of the Script component.</summary>
  /// <param name="format">String format.</param>
  /// <param name="args">Formatting parameters.</param>
  private void Print(string format, params object[] args) { __out.Add(string.Format(format, args)); }
  /// <summary>Print useful information about an object instance to the [Out] Parameter of the Script component. </summary>
  /// <param name="obj">Object instance to parse.</param>
  private void Reflect(object obj) { __out.Add(GH_ScriptComponentUtilities.ReflectType_CS(obj)); }
  /// <summary>Print the signatures of all the overloads of a specific method to the [Out] Parameter of the Script component. </summary>
  /// <param name="obj">Object instance to parse.</param>
  private void Reflect(object obj, string method_name) { __out.Add(GH_ScriptComponentUtilities.ReflectType_CS(obj, method_name)); }
#endregion

#region Members
  /// <summary>Gets the current Rhino document.</summary>
  private RhinoDoc RhinoDocument;
  /// <summary>Gets the Grasshopper document that owns this script.</summary>
  private GH_Document GrasshopperDocument;
  /// <summary>Gets the Grasshopper script component that owns this script.</summary>
  private IGH_Component Component; 
  /// <summary>
  /// Gets the current iteration count. The first call to RunScript() is associated with Iteration==0.
  /// Any subsequent call within the same solution will increment the Iteration count.
  /// </summary>
  private int Iteration;
#endregion

  /// <summary>
  /// This procedure contains the user code. Input parameters are provided as regular arguments, 
  /// Output parameters as ref arguments. You don't have to assign output parameters, 
  /// they will have a default value.
  /// </summary>
  private void RunScript(bool Update, ref object A)
  {
        if(!Update)
      A = ghbreps;
    else
    {
      Revit.EnqueueAction((uido) => UpdateTopographySurface(uido));
      A = ghbreps;

      foreach(string s in outVals){
        Print(s);
      }
    }

  }

  // <Custom additional code> 
    ElementId topographySurfaceId = ElementId.InvalidElementId;
  //List<List<Point3d>> outputs = new List<List<Point3d>>();
  List<GH_Brep> ghbreps = new List<GH_Brep>();
  List<string> outVals = new List<string>();

  void UpdateTopographySurface(UIDocument uidoc)
  {
    Selection selection = uidoc.Selection;
    ICollection<ElementId> selectedIds = uidoc.Selection.GetElementIds();
    outVals.Add("Num Selection: " + selectedIds.Count.ToString());
    ghbreps = new List<GH_Brep>();
    foreach (ElementId id in selectedIds)
    {
      //var ele = uidoc.Document.GetElement(id).Geometry();
      String faceInfo = "";
      Autodesk.Revit.DB.Options opt = new Options();
      Autodesk.Revit.DB.GeometryElement geomElem = uidoc.Document.GetElement(id).get_Geometry(opt);

      outVals.Add("Num Geo: " + geomElem.Count().ToString());
      foreach (GeometryObject geomObj in geomElem)
      {
        var facePtList = new List<List<Point3d>>();

        Solid geomSolid = geomObj as Solid;
        if (null != geomSolid)
        {

          var brepSrfs = new List<Brep>();
          foreach (Face geomFace in geomSolid.Faces)              // get the faces from the solid
          {
            // outVals.Add("Face: " + faces.ToString());
            var facePts = new List<Point3d>();

            outVals.Add("Num Edge Loops: " + geomFace.EdgeLoops.Size.ToString());

            var edgeLp = geomFace.EdgeLoops.get_Item(0);
            // foreach (EdgeArray geomEdges in geomFace.EdgeLoops)   // get the edges from each face
            // {

            //outVals.Add("Edge Count: " + edgeLp.Size.ToString());
            int c = 0;
            foreach (Edge edge in edgeLp)                    // get edges from edges
              //foreach (Edge edge in geomEdges)                    // get edges from edges
            {
              c++;
              var crv = edge.AsCurve();
              XYZ p1 = edge.Evaluate(0);                        // extract start point


              var point = new Point3d(p1.X, p1.Y, p1.Z);        // convert to rhino poing
              //   Print(point.ToString());
              facePts.Add(point);                               // save pt to list
              //if (c == geomEdges.Size)
              if (c == edgeLp.Size);
              {
                XYZ p2 = edge.Evaluate(1);                      // get the last point in the edge loop
                var endPt = new Point3d(p2.X, p2.Y, p2.Z);
                facePts.Add(endPt);
              }

            }
            // get wall's geometry edges
            //}

            Rhino.Geometry.Curve tempCurve = Rhino.Geometry.Curve.CreateInterpolatedCurve(facePts, 1);
         
            List<Brep> breps = new List<Brep>();
            Brep[] planarBreps = Brep.CreatePlanarBreps(tempCurve, 0.01);
            outVals.Add(planarBreps.Count().ToString());
            if (planarBreps != null){
              brepSrfs.Add(planarBreps[0]);
            }

          }
          var joinedBrep = Rhino.Geometry.Brep.JoinBreps(brepSrfs, 0.01)[0];
          var ghB = new GH_Brep(joinedBrep);
          ghbreps.Add(ghB);

        }
        

        //outputs = facePtList;

      }


      //TaskDialog.Show("Revit", faceInfo);
      //ids.Add(id.IntegerValue);
    }



  }


  // </Custom additional code> 

  private List<string> __err = new List<string>(); //Do not modify this list directly.
  private List<string> __out = new List<string>(); //Do not modify this list directly.
  private RhinoDoc doc = RhinoDoc.ActiveDoc;       //Legacy field.
  private IGH_ActiveObject owner;                  //Legacy field.
  private int runCount;                            //Legacy field.
  
  public override void InvokeRunScript(IGH_Component owner, object rhinoDocument, int iteration, List<object> inputs, IGH_DataAccess DA)
  {
    //Prepare for a new run...
    //1. Reset lists
    this.__out.Clear();
    this.__err.Clear();

    this.Component = owner;
    this.Iteration = iteration;
    this.GrasshopperDocument = owner.OnPingDocument();
    this.RhinoDocument = rhinoDocument as Rhino.RhinoDoc;

    this.owner = this.Component;
    this.runCount = this.Iteration;
    this. doc = this.RhinoDocument;

    //2. Assign input parameters
        bool Update = default(bool);
    if (inputs[0] != null)
    {
      Update = (bool)(inputs[0]);
    }



    //3. Declare output parameters
      object A = null;


    //4. Invoke RunScript
    RunScript(Update, ref A);
      
    try
    {
      //5. Assign output parameters to component...
            if (A != null)
      {
        if (GH_Format.TreatAsCollection(A))
        {
          IEnumerable __enum_A = (IEnumerable)(A);
          DA.SetDataList(1, __enum_A);
        }
        else
        {
          if (A is Grasshopper.Kernel.Data.IGH_DataTree)
          {
            //merge tree
            DA.SetDataTree(1, (Grasshopper.Kernel.Data.IGH_DataTree)(A));
          }
          else
          {
            //assign direct
            DA.SetData(1, A);
          }
        }
      }
      else
      {
        DA.SetData(1, null);
      }

    }
    catch (Exception ex)
    {
      this.__err.Add(string.Format("Script exception: {0}", ex.Message));
    }
    finally
    {
      //Add errors and messages... 
      if (owner.Params.Output.Count > 0)
      {
        if (owner.Params.Output[0] is Grasshopper.Kernel.Parameters.Param_String)
        {
          List<string> __errors_plus_messages = new List<string>();
          if (this.__err != null) { __errors_plus_messages.AddRange(this.__err); }
          if (this.__out != null) { __errors_plus_messages.AddRange(this.__out); }
          if (__errors_plus_messages.Count > 0) 
            DA.SetDataList(0, __errors_plus_messages);
        }
      }
    }
  }
}