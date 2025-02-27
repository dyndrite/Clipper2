﻿/*******************************************************************************
*                                                                              *
* Author    :  Angus Johnson                                                   *
* Date      :  16 May 2022                                                     *
* Website   :  http://www.angusj.com                                           *
* Copyright :  Angus Johnson 2010-2022                                         *
* License   :  http://www.boost.org/LICENSE_1_0.txt                            *
*******************************************************************************/

using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Clipper2Lib;

namespace ClipperDemo1
{

  using Paths64 = List<List<Point64>>;
  using PathD = List<PointD>;
  using PathsD = List<List<PointD>>;

  public class Application
  {

    public static void Main()
    {
      DoSimpleShapes();
      DoRabbit();
    }

    public static void DoSimpleShapes()
    {
      //triangle offset - with large miter
      Paths64 p = new Paths64();
      p.Add(Clipper.MakePath(new int[] { 30, 150, 60, 350, 0, 350 }));
      Paths64 pp = new Paths64();
      pp.AddRange(p);

      for (int i = 0; i < 5; ++i)
      {
        //nb: the following '10' parameter greatly increases miter limit
        p = Clipper.InflatePaths(p, 5, JoinType.Miter, EndType.Polygon, 10);
        pp.AddRange(p);
      }

      //rectangle offset - both squared and rounded
      p.Clear();
      p.Add(Clipper.MakePath(new int[] { 100, 0, 340, 0, 340, 200, 100, 200 }));
      pp.AddRange(p);
      //nb: using the ClipperOffest class directly here to control 
      //different join types within the same offset operation
      ClipperOffset co = new ClipperOffset();
      co.AddPaths(p, JoinType.Miter, EndType.Joined);
      p = Clipper.TranslatePaths(p, 120, 100);
      pp.AddRange(p);
      co.AddPaths(p, JoinType.Round, EndType.Joined);
      p = co.Execute(20);
      pp.AddRange(p);

      SimpleSvgWriter svg = new SimpleSvgWriter();
      SvgUtils.AddSolution(svg, pp, false);
      SvgUtils.SaveToFile(svg, "../../../inflate.svg", FillRule.EvenOdd, 800, 600, 20);
      ClipperFileIO.OpenFileWithDefaultApp("../../../inflate.svg");
    }

    public static void DoRabbit()
    {
      PathsD pd = LoadPathsFromResource("InflateDemo.rabbit.bin");

      PathsD solution = new PathsD(pd);
      while (pd.Count > 0)
      {
        //don't forget to scale the delta offset
        pd = Clipper.InflatePaths(pd, -2.5, JoinType.Round, EndType.Polygon);
        //RamerDouglasPeucker - not essential but not only 
        //speeds up the loop but also tidies the result
        pd = Clipper.RamerDouglasPeucker(pd, 0.025);
        solution.AddRange(pd);
      }

      SimpleSvgWriter svg = new SimpleSvgWriter();
      SvgUtils.AddSolution(svg, solution, false);
      SvgUtils.SaveToFile(svg, "../../../rabbit2.svg", FillRule.EvenOdd, 450, 720, 10);
      ClipperFileIO.OpenFileWithDefaultApp("../../../rabbit2.svg");
    }

    public static PathsD LoadPathsFromResource(string resourceName)
    {
      using Stream stream = Assembly.GetExecutingAssembly().
        GetManifestResourceStream(resourceName);
      if (stream == null) return new PathsD();
      using BinaryReader reader = new BinaryReader(stream);
      int len = reader.ReadInt32();
      PathsD result = new PathsD(len);
      for (int i = 0; i < len; i++)
      {
        int len2 = reader.ReadInt32();
        PathD p = new PathD(len2);
        for (int j = 0; j < len2; j++)
        {
          long X = reader.ReadInt64();
          long Y = reader.ReadInt64();
          p.Add(new PointD(X, Y));
        }
        result.Add(p);
      }
      return result;
    }
  
  } //end Application

} //namespace
