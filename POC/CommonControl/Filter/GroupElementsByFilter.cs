using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POC
{
    public class GroupElementsByFilter
    {
        public static Dictionary<int, List<ConduitGrid>> GroupByElements(List<Element> ElementCollection, string offsetVariable, double maximumSpacing = 0.5)
        {


            List<ConduitGrid> CongridReferences = new List<ConduitGrid>();
            List<Element> RefElements = new List<Element>();
            foreach (Element elementOne in ElementCollection)
            {
                Line lineOne = (elementOne.Location as LocationCurve).Curve as Line;
                XYZ DirectionOne = lineOne.Direction;
                if (!RefElements.Any(r => Utility.IsSameDirection(((r.Location as LocationCurve).Curve as Line).Direction, DirectionOne)))
                {
                    RefElements.Add(elementOne);
                }
            }
            foreach (Element elementOne in RefElements)
            {
                Line lineOne = (elementOne.Location as LocationCurve).Curve as Line;
                XYZ DirectionOne = lineOne.Direction;
                foreach (Element elementTwo in ElementCollection)
                {
                    Line lineTwo = (elementTwo.Location as LocationCurve).Curve as Line;
                    XYZ DirectionTwo = lineTwo.Direction;
                    if (Utility.IsSameDirection(DirectionOne, DirectionTwo))
                    {
                        XYZ stpt = lineOne.GetEndPoint(0);
                        XYZ edpt = lineOne.GetEndPoint(1);
                        if (!new XYZ(stpt.X, stpt.Y, 0).IsAlmostEqualTo(new XYZ(edpt.X, edpt.Y, 0)))
                        {
                            Line verticalLine = null;
                            XYZ midPoint = (stpt + edpt) / 2;
                            XYZ cross = lineOne.Direction.CrossProduct(XYZ.BasisZ);
                            XYZ newStart = midPoint + cross.Multiply(1);
                            XYZ newEnd = midPoint - cross.Multiply(1);
                            try
                            {
                                verticalLine = Line.CreateBound(newStart, newEnd);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                            if (verticalLine != null)
                            {
                                XYZ stpt2 = lineTwo.GetEndPoint(0);
                                XYZ edpt2 = lineTwo.GetEndPoint(1);
                                if (!new XYZ(stpt2.X, stpt2.Y, 0).IsAlmostEqualTo(new XYZ(edpt2.X, edpt2.Y, 0)))
                                {
                                    try
                                    {
                                        XYZ intersectionPoint = Utility.FindIntersection(elementTwo, verticalLine);
                                        if (intersectionPoint != null)
                                        {
                                            ConduitGrid CongridRef = new ConduitGrid(elementTwo, elementOne, intersectionPoint.DistanceTo(new XYZ(midPoint.X, midPoint.Y, 0)));
                                            CongridReferences.Add(CongridRef);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine(ex.Message);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            Dictionary<int, List<ConduitGrid>> CongridDictionary = new Dictionary<int, List<ConduitGrid>>();
            List<Element> processedConduits = new List<Element>();
            int index = 0;
            //Groups grid and Elements based on shortest distance between them
            foreach (var group in CongridReferences.GroupBy(r => r.RefConduit.Id))
            {
                foreach (ConduitGrid congrid in group.OrderBy(x => x.Distance))
                {
                    if (!processedConduits.Any(r => r.Id == congrid.Conduit.Id))
                    {
                        List<ConduitGrid> ReferenceGroup = CongridReferences.Where(r => r.RefConduit.Id == congrid.RefConduit.Id && !CongridDictionary.Any(x => x.Value.Any(y => y.Conduit.Id == r.Conduit.Id))).ToList();
                        List<ConduitGrid> IntersectingConduits = ReferenceGroup.Where(r => Utility.GetIntersection(r.Conduit, congrid, congrid.StartPoint, maximumSpacing) != null).ToList();
                        IntersectingConduits.AddRange(ReferenceGroup.Where(r => !IntersectingConduits.Any(x => r.Conduit.Id == x.Conduit.Id) && Utility.GetIntersection(r.Conduit, congrid, congrid.EndPoint, maximumSpacing) != null).ToList());
                        IntersectingConduits.AddRange(ReferenceGroup.Where(r => !IntersectingConduits.Any(x => r.Conduit.Id == x.Conduit.Id) && Utility.GetIntersection(r.Conduit, congrid, congrid.MidPoint, maximumSpacing) != null).ToList());
                        if (IntersectingConduits != null && IntersectingConduits.Any())
                        {
                            if (!CongridDictionary.Any(r => r.Value.Any(x => IntersectingConduits.Any(y => y.Conduit.Id == x.Conduit.Id))))
                            {
                                foreach (ConduitGrid cg in IntersectingConduits)
                                {
                                    ReferenceGroup = CongridReferences.Where(r => r.RefConduit.Id == cg.RefConduit.Id).ToList();
                                    List<ConduitGrid> ISC = ReferenceGroup.Where(r => Utility.GetIntersection(r.Conduit, cg, cg.StartPoint, maximumSpacing) != null).ToList();
                                    ISC.AddRange(ReferenceGroup.Where(r => !ISC.Any(x => r.Conduit.Id == x.Conduit.Id) && Utility.GetIntersection(r.Conduit, cg, cg.EndPoint, maximumSpacing) != null).ToList());
                                    ISC.AddRange(ReferenceGroup.Where(r => !ISC.Any(x => r.Conduit.Id == x.Conduit.Id) && Utility.GetIntersection(r.Conduit, cg, cg.MidPoint, maximumSpacing) != null).ToList());
                                    if (ISC != null)
                                    {
                                        if (!CongridDictionary.Any(r => r.Value.Any(x => ISC.Any(y => y.Conduit.Id == x.Conduit.Id))))
                                        {
                                            ISC = ISC.Where(x => !CongridDictionary.Any(r => r.Value.Any(y => x.Conduit.Id == y.Conduit.Id))).ToList();
                                            if (!CongridDictionary.Any(r => r.Key == index))
                                                CongridDictionary.Add(index, ISC);
                                        }
                                        else
                                        {
                                            KeyValuePair<int, List<ConduitGrid>> existingKeyValuePair = CongridDictionary.FirstOrDefault(r => r.Value.Any(x => ISC.Any(y => y.Conduit.Id == x.Conduit.Id)));
                                            List<ConduitGrid> existingConGrids = existingKeyValuePair.Value;
                                            if (existingConGrids != null)
                                            {
                                                foreach (ConduitGrid conGrid in ISC)
                                                {
                                                    if (!existingConGrids.Any(r => r.Conduit.Id == conGrid.Conduit.Id && r.RefConduit.Id == conGrid.RefConduit.Id) && !CongridDictionary.Any(r => r.Value.Any(x => x.Conduit.Id == conGrid.Conduit.Id)))
                                                    {
                                                        existingConGrids.Add(conGrid);
                                                    }
                                                }
                                                CongridDictionary[existingKeyValuePair.Key] = existingConGrids;
                                            }
                                        }
                                    }
                                }
                                if (!CongridDictionary.Any(r => IntersectingConduits.Any(x => r.Value.Any(y => x.Conduit.Id == y.Conduit.Id))))
                                {
                                    IntersectingConduits = IntersectingConduits.Where(x => !CongridDictionary.Any(r => r.Value.Any(y => x.Conduit.Id == y.Conduit.Id))).ToList();
                                    if (!CongridDictionary.Any(r => r.Key == index))
                                    {
                                        if (!CongridDictionary.Any(r => IntersectingConduits.Any(x => r.Value.Any(y => x.Conduit.Id == y.RefConduit.Id || x.RefConduit.Id == y.Conduit.Id))))
                                        {
                                            CongridDictionary.Add(index, IntersectingConduits);
                                        }
                                        else
                                        {
                                            KeyValuePair<int, List<ConduitGrid>> existingKeyValuePair = CongridDictionary.FirstOrDefault(r => r.Value.Any(x => IntersectingConduits.Any(y => y.RefConduit.Id == x.Conduit.Id)));
                                            List<ConduitGrid> existingConGrids = existingKeyValuePair.Value;
                                            if (existingConGrids != null)
                                            {
                                                foreach (ConduitGrid conGrid in IntersectingConduits)
                                                {
                                                    if (!existingConGrids.Any(r => r.Conduit.Id == conGrid.Conduit.Id && r.RefConduit.Id == conGrid.RefConduit.Id) && !CongridDictionary.Any(r => r.Value.Any(x => x.Conduit.Id == conGrid.Conduit.Id)))
                                                    {
                                                        existingConGrids.Add(conGrid);
                                                    }
                                                }
                                                CongridDictionary[existingKeyValuePair.Key] = existingConGrids;
                                            }
                                        }
                                    }
                                }
                                index++;
                            }
                            else
                            {
                                KeyValuePair<int, List<ConduitGrid>> existingKeyValuePair = CongridDictionary.FirstOrDefault(r => r.Value.Any(x => IntersectingConduits.Any(y => y.Conduit.Id == x.Conduit.Id)));
                                List<ConduitGrid> existingConGrids = existingKeyValuePair.Value;
                                if (existingConGrids != null)
                                {
                                    foreach (ConduitGrid conGrid in IntersectingConduits)
                                    {
                                        if (!existingConGrids.Any(r => r.Conduit.Id == conGrid.Conduit.Id && r.RefConduit.Id == conGrid.RefConduit.Id) && !CongridDictionary.Any(r => r.Value.Any(x => x.Conduit.Id == conGrid.Conduit.Id)))
                                        {
                                            existingConGrids.Add(conGrid);
                                        }
                                    }
                                    CongridDictionary[existingKeyValuePair.Key] = existingConGrids;
                                }
                            }
                            processedConduits.Add(congrid.Conduit);
                        }
                    }
                }
            }
            return CongridDictionary;

        }



        /// <summary>
        ///  this for reference code only  , how to group the logic as elevation 
        ///  dont call this function because you can another new loop .. 
        /// </summary>
        /// <param name="ElementCollection"></param>
        /// <param name="offsetVariable"></param>
        /// <returns></returns>
        public static Dictionary<double, List<Element>> GroupByElementsWithElevation(List<Element> ElementCollection, string offsetVariable)
        {
            Dictionary<int, List<ConduitGrid>> CongridDictionary = GroupByElements(ElementCollection, offsetVariable);
            Dictionary<double, List<Element>> groupedPrimaryElements = new Dictionary<double, List<Element>>();
            foreach (KeyValuePair<int, List<ConduitGrid>> kvp in CongridDictionary)
            {
                if (kvp.Value.Any())
                {
                    List<Element> groupedConduits = kvp.Value.Select(r => r.Conduit).ToList();
                    Utility.GroupByElevation(groupedConduits, offsetVariable, ref groupedPrimaryElements);

                }

            }
            return groupedPrimaryElements;
        }
    }
}
