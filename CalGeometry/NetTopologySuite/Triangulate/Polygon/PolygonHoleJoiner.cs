﻿using NetTopologySuite.Geometries;
using NetTopologySuite.Noding;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetTopologySuite.Triangulate.Polygon
{
    /// <summary>
    /// Transforms a polygon with holes into a single self-touching (invalid) ring
    /// by joining holes to the exterior shell or to another hole.
    /// The holes are added from the lowest upwards.
    /// As the resulting shell develops, a hole might be added to what was
    /// originally another hole.
    /// <para/>
    /// There is no attempt to optimize the quality of the join lines.
    /// In particular, a hole which already touches at a vertex may be
    /// joined at a different vertex.
    /// </summary>
    public class PolygonHoleJoiner
    {
        /// <summary>
        /// The comparer to use when sorting <see cref="_shellCoordsSortedArray"/>
        /// </summary>
        private static readonly IComparer<Coordinate> _comparer =
            Comparer<Coordinate>.Create((u, v) => u.CompareTo(v));

        public static Geometries.Polygon JoinAsPolygon(Geometries.Polygon inputPolygon)
        {
            return inputPolygon.Factory.CreatePolygon(Join(inputPolygon));
        }

        public static Coordinate[] Join(Geometries.Polygon inputPolygon)
        {
            var joiner = new PolygonHoleJoiner(inputPolygon);
            return joiner.Compute();
        }

        private const double EPS = 1.0E-4;

        private List<Coordinate> _shellCoords;

        // Note: _shellCoordsSorted is a TreeSet in JTS which has functionality not
        //       provided by dotnet's SortedSet. Thus _orderedCoords is split into
        //       HashSet _orderedCoords and _orderedCoordsArray for which
        //       Above, Below and Min are added.

        // a copy of unique shellCoords
        private HashSet<Coordinate> _shellCoordsSorted;
        // _shellCoordsSortedArray is a sorted array of the coordinates stored in _shellCoordsSorted
        private Coordinate[] _shellCoordsSortedArray;

        // Key: starting end of the cut; Value: list of the other end of the cut
        private Dictionary<Coordinate, List<Coordinate>> _cutMap;
        private readonly ISegmentSetMutualIntersector _polygonIntersector;

        private readonly Geometries.Polygon _inputPolygon;

        public PolygonHoleJoiner(Geometries.Polygon inputPolygon)
        {
            _inputPolygon = inputPolygon;
            _polygonIntersector = CreatePolygonIntersector(inputPolygon);
        }

        /// <summary>
        /// Computes the joined ring.
        /// </summary>
        /// <returns>The points in the joined ring</returns>
        public Coordinate[] Compute()
        {
            //--- copy the input polygon shell coords
            _shellCoords = RingCoordinates(_inputPolygon.ExteriorRing);
            if (_inputPolygon.NumInteriorRings != 0)
            {
                JoinHoles();
            }
            return _shellCoords.ToArray();
        }

        private static List<Coordinate> RingCoordinates(LineString ring)
        {
            var coords = ring.Coordinates;
            var coordList = new List<Coordinate>();
            foreach (var p in coords)
            {
                coordList.Add(p);
            }
            return coordList;
        }

        private void JoinHoles()
        {
            _shellCoordsSorted = new HashSet<Coordinate>();
            foreach (var coord in _shellCoords)
                AddOrderedCoord(coord);

            _cutMap = new Dictionary<Coordinate, List<Coordinate>>();
            var orderedHoles = SortHoles(_inputPolygon);
            for (int i = 0; i < orderedHoles.Count; i++)
            {
                JoinHole(orderedHoles[i]);
            }
        }

        /// <summary>
        /// Adds a coordinate to the <see cref="_shellCoordsSorted"/> set and
        /// clears the <see cref="_shellCoordsSortedArray"/> array.
        /// </summary>
        /// <param name="coord">A coordinate</param>
        private void AddOrderedCoord(Coordinate coord)
        {
            if (_shellCoordsSorted.Add(coord))
                _shellCoordsSortedArray = null;

        }

        /// <summary>
        /// Joins a single hole to the current shellRing.
        /// </summary>
        /// <param name="hole">The hole to join</param>
        private void JoinHole(LinearRing hole)
        {
            /*
             * 1) Get a list of HoleVertex Index. 
             * 2) Get a list of ShellVertex. 
             * 3) Get the pair that has the shortest distance between them. 
             * This pair is the endpoints of the cut 
             * 4) The selected ShellVertex may occurs multiple times in
             * shellCoords[], so find the proper one and add the hole after it.
             */
            var holeCoords = hole.Coordinates;
            var holeLeftVerticesIndex = FindLeftVertices(hole);
            var holeCoord = holeCoords[holeLeftVerticesIndex[0]];
            var shellCoordsList = FindLeftShellVertices(holeCoord);
            var shellCoord = shellCoordsList[0];
            int shortestHoleVertexIndex = 0;
            //--- pick the shell-hole vertex pair that gives the shortest distance
            if (Math.Abs(shellCoord.X - holeCoord.X) < EPS)
            {
                double shortest = double.MaxValue;
                for (int i = 0; i < holeLeftVerticesIndex.Count; i++)
                {
                    for (int j = 0; j < shellCoordsList.Count; j++)
                    {
                        double currLength = Math.Abs(shellCoordsList[j].Y - holeCoords[holeLeftVerticesIndex[i]].Y);
                        if (currLength < shortest)
                        {
                            shortest = currLength;
                            shortestHoleVertexIndex = i;
                            shellCoord = shellCoordsList[j];
                        }
                    }
                }
            }
            int shellVertexIndex = GetShellCoordIndex(shellCoord,
                holeCoords[holeLeftVerticesIndex[shortestHoleVertexIndex]]);
            AddHoleToShell(shellVertexIndex, holeCoords, holeLeftVerticesIndex[shortestHoleVertexIndex]);
        }

        /// <summary>
        /// Get the i'th <paramref name="shellVertex"/> in <see cref="_shellCoords"/> that the current should add after
        /// </summary>
        /// <param name="shellVertex">The coordinate of the shell vertex</param>
        /// <param name="holeVertex">The coordinate of the hole vertex</param>
        /// <returns>The i'th shellvertex</returns>
        private int GetShellCoordIndex(Coordinate shellVertex, Coordinate holeVertex)
        {
            int numSkip = 0;
            var newValueList = new List<Coordinate>();
            newValueList.Add(holeVertex);
            if (_cutMap.ContainsKey(shellVertex))
            {
                foreach (var coord in _cutMap[shellVertex])
                {
                    if (coord.Y < holeVertex.Y)
                    {
                        numSkip++;
                    }
                }
                _cutMap[shellVertex].Add(holeVertex);
            }
            else
            {
                _cutMap[shellVertex] = newValueList;
            }
            if (!_cutMap.ContainsKey(holeVertex))
            {
                _cutMap.Add(holeVertex, new List<Coordinate>(newValueList));
            }
            return GetShellCoordIndexSkip(shellVertex, numSkip);
        }

        /// <summary>
        /// Find the index of the coordinate in ShellCoords ArrayList,
        /// skipping over some number of matches
        /// </summary>
        /// <param name="coord"></param>
        /// <param name="numSkip"></param>
        /// <returns></returns>
        private int GetShellCoordIndexSkip(Coordinate coord, int numSkip)
        {
            for (int i = 0; i < _shellCoords.Count; i++)
            {
                if (_shellCoords[i].Equals2D(coord, EPS))
                {
                    if (numSkip == 0)
                        return i;
                    numSkip--;
                }
            }
            throw new ArgumentException("Vertex is not in shellcoords", nameof(coord));
        }

        /// <summary>
        /// Gets a list of shell vertices that could be used to join with the hole.
        /// This list contains only one item if the chosen vertex does not share the same
        /// x value with <paramref name="holeCoord"/>
        /// </summary>
        /// <param name="holeCoord">The hole coordinate</param>
        /// <returns>A list of candidate join vertices</returns>
        private List<Coordinate> FindLeftShellVertices(Coordinate holeCoord)
        {
            if (_shellCoordsSortedArray == null)
                _shellCoordsSortedArray = _shellCoordsSorted.OrderBy(x => x, _comparer) .ToArray();

            var list = new List<Coordinate>();
            var closest = Above(holeCoord);
            while (closest.X == holeCoord.X) {
                closest = Above(closest);
            }
            do {
                closest = Below(closest);
            } while (!IsJoinable(holeCoord, closest) && !closest.Equals(Min));
            list.Add(closest);
            if (closest.X != holeCoord.X)
                return list;
            double chosenX = closest.X;
            list.Clear();
            while (chosenX == closest.X)
            {
                list.Add(closest);
                closest = Below(closest);
                if (closest == null)
                    return list;
            }
            return list;
        }

        /// <summary>
        /// Determine if a line segment between a hole vertex
        /// and a shell vertex lies inside the input polygon.
        /// </summary>
        /// <param name="holeCoord">A hole coordinate</param>
        /// <param name="shellCoord">A shell coordinate</param>
        /// <returns><c>true</c> if the line lies inside the polygon</returns>
        private bool IsJoinable(Coordinate holeCoord, Coordinate shellCoord)
        {
            /*
             * Since the line runs between a hole and the shell,
             * it is inside the polygon if it does not cross the polygon boundary.
             */
            bool isJoinable = !CrossesPolygon(holeCoord, shellCoord);
            /*
            //--- slow code for testing only
            LineString join = geomFact.createLineString(new Coordinate[] { holeCoord, shellCoord });
            boolean isJoinableSlow = inputPolygon.covers(join)
            if (isJoinableSlow != isJoinable) {
              System.out.println(WKTWriter.toLineString(holeCoord, shellCoord));
            }
            //Assert.isTrue(isJoinableSlow == isJoinable);
            */
            return isJoinable;
        }

        /// <summary>
        /// Tests whether a line segment crosses the polygon boundary.
        /// </summary>
        /// <param name="p0">A vertex</param>
        /// <param name="p1">A vertex</param>
        /// <returns><c>true</c> if the line segment crosses the polygon boundary</returns>
        private bool CrossesPolygon(Coordinate p0, Coordinate p1)
        {
            var segString = new BasicSegmentString(
                new Coordinate[] { p0, p1 }, null);
            var segStrings = new List<ISegmentString>();
            segStrings.Add(segString);

            var segInt = new SegmentIntersectionDetector();
            segInt.FindProper = true;
            _polygonIntersector.Process(segStrings, segInt);

            return segInt.HasProperIntersection;
        }

        /// <summary>
        /// Add hole vertices at proper position in shell vertex list.
        /// For a touching/zero-length join line, avoids adding the join vertices twice.
        /// Also adds hole points to ordered coordinates.
        /// </summary>
        /// <param name="shellJoinIndex">The index of join vertex in shell</param>
        /// <param name="holeCoords">The vertices of the hole to be inserted</param>
        /// <param name="holeJoinIndex">The index of join vertex in hole</param>
        private void AddHoleToShell(int shellJoinIndex, Coordinate[] holeCoords, int holeJoinIndex)
        {
            var shellJoinPt = _shellCoords[shellJoinIndex];
            var holeJoinPt = holeCoords[holeJoinIndex];
            //-- check for touching (zero-length) join to avoid inserting duplicate vertices
            bool isJoinTouching = shellJoinPt.Equals2D(holeJoinPt);

            //-- create new section of vertices to insert in shell
            var newSection = new List<Coordinate>();
            if (!isJoinTouching)
            {
                newSection.Add(shellJoinPt.Copy());
            }

            int nPts = holeCoords.Length - 1;
            int i = holeJoinIndex;
            do
            {
                newSection.Add(holeCoords[i].Copy());
                i = (i + 1) % nPts;
            } while (i != holeJoinIndex);
            if (!isJoinTouching)
                newSection.Add(holeCoords[holeJoinIndex].Copy());

            _shellCoords.InsertRange(shellJoinIndex, newSection);
            foreach (var coord in newSection)
                AddOrderedCoord(coord);
        }

        /// <summary>
        /// Sort the hole rings by minimum X, minimum Y.
        /// </summary>
        /// <param name="poly">Polygon that contains the holes</param>
        /// <returns>A list of sorted hole rings</returns>
        private static List<LinearRing> SortHoles(Geometries.Polygon poly)
        {
            var holes = new List<LinearRing>();
            for (int i = 0; i < poly.NumInteriorRings; i++)
            {
                holes.Add((LinearRing)poly.GetInteriorRingN(i));
            }
            holes.Sort(EnvelopeComparer.Instance);
            return holes;
        }

        /// <summary>
        /// Gets a list of indices of the leftmost vertices in a ring.
        /// </summary>
        /// <param name="ring">The hole ring</param>
        /// <returns>Index of the left most vertex</returns>
        private static List<int> FindLeftVertices(LinearRing ring)
        {
            var coords = ring.Coordinates;
            var leftmostIndex = new List<int>();
            double leftX = ring.EnvelopeInternal.MinX;
            for (int i = 0; i < coords.Length - 1; i++)
            {
                //TODO: can this be strict equality?
                if (Math.Abs(coords[i].X - leftX) < EPS)
                {
                    leftmostIndex.Add(i);
                }
            }
            return leftmostIndex;
        }

        private static ISegmentSetMutualIntersector CreatePolygonIntersector(Geometries.Polygon polygon)
        {
            var polySegStrings = SegmentStringUtil.ExtractSegmentStrings(polygon);
            return new MCIndexSegmentSetMutualIntersector(polySegStrings);
        }

        private class EnvelopeComparer : IComparer<Geometry>
        {
            public static EnvelopeComparer Instance = new EnvelopeComparer();

            private EnvelopeComparer() { }
            public int Compare(Geometry o1, Geometry o2)
            {
                var e1 = o1.EnvelopeInternal;
                var e2 = o2.EnvelopeInternal;
                return e1.CompareTo(e2);
            }
        }

        #region Functionality from TreeSet

        private Coordinate Above(Coordinate coordinate)
        {
            if (_shellCoordsSortedArray == null)
                throw new InvalidOperationException("_orderedCoordsArray not initialized");

            int index = Array.BinarySearch(_shellCoordsSortedArray, coordinate);
            if (index < 0)
            {
                // Convert to index of item just higher than coordinate
                index = ~index;
            }
            else
            {
                // We have a match, need to increase index to get next higher value
                index++;
            }

            if (index < _shellCoordsSortedArray.Length)
                return _shellCoordsSortedArray[index];
            return null;
        }

        private Coordinate Below(Coordinate coordinate)
        {
            if (_shellCoordsSortedArray == null)
                throw new InvalidOperationException("_orderedCoordsArray not initialized");

            int index = Array.BinarySearch(_shellCoordsSortedArray, coordinate);
            if (index < 0)
                index = ~index;

            // We want the index of the item below
            index--;
            if (index >= 0)
                return _shellCoordsSortedArray[index];
            return null;
        }

        private Coordinate Min
        {
            get
            {
                if (_shellCoordsSortedArray == null)
                    throw new InvalidOperationException("_orderedCoordsArray not initialized");

                return _shellCoordsSortedArray[0];
            }
        }

        #endregion
    }
}
