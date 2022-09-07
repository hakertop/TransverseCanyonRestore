﻿using System;
using System.Collections.Generic;
using System.Text;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Clip
{
    /// <summary>
    /// Clips polygonal geometry to a rectangle.
    /// This implementation is faster, more robust,
    /// and less sensitive to invalid input than
    /// <see cref="Geometry.Intersection(Geometry)"/>.
    /// <para/>
    /// It can also enforce a supplied precision model on the computed result.
    /// The inputs do not have to meet the precision model.
    /// This allows clipping using integer coordinates
    /// in the output, for example.
    /// </summary>
    /// <author>mdavis</author>
    public class RectangleClipPolygon
    {

        private const int ENV_LEFT = 3;
        private const int ENV_TOP = 2;
        private const int ENV_RIGHT = 1;
        private const int ENV_BOTTOM = 0;

        public static Geometry Clip(Geometry geom, Geometry rectangle)
        {
            var ctt = new RectangleClipPolygon(rectangle);
            var result = ctt.Clip(geom);
            return result;
        }

        public static Geometry Clip(Geometry geom, Geometry rectangle, PrecisionModel pm)
        {
            var ctt = new RectangleClipPolygon(rectangle, pm);
            var result = ctt.Clip(geom);
            return result;
        }

        private readonly Envelope _clipEnv;
        private readonly double _clipEnvMinY;
        private readonly double _clipEnvMaxY;
        private readonly double _clipEnvMinX;
        private readonly double _clipEnvMaxX;
        private readonly PrecisionModel _precModel;

        public RectangleClipPolygon(Envelope clipEnv)
            : this(clipEnv, new PrecisionModel(PrecisionModels.Floating))
        { }

        public RectangleClipPolygon(Geometry clipRectangle)
            : this(clipRectangle, new PrecisionModel(PrecisionModels.Floating))
        { }

        public RectangleClipPolygon(Geometry clipRectangle, PrecisionModel pm)
            : this(clipRectangle.EnvelopeInternal, pm)
        { }

        public RectangleClipPolygon(Envelope clipEnv, PrecisionModel pm)
        {
            _clipEnv = clipEnv;
            _clipEnvMinY = clipEnv.MinY;
            _clipEnvMaxY = clipEnv.MaxY;
            _clipEnvMinX = clipEnv.MinX;
            _clipEnvMaxX = clipEnv.MaxX;

            _precModel = pm;
        }

        public Geometry Clip(Geometry geom)
        {
            var geomsClip = ClipCollection(geom);
            if (geomsClip == null)
                return geom.Factory.CreatePolygon();

            return FixTopology(geomsClip);
        }

        /// <summary>
        /// The clipped geometry may be invalid
        /// (due to coincident at clip edges, 
        /// or due to precision reduction if performed).
        /// This method fixed the geometry topology to be valid.
        /// <para/>
        /// Currently uses the buffer(0) trick.
        /// This should work in most cases(but need to verify this).
        /// But it may produce unexpected results if the input polygon
        /// was invalid inside the clip area.
        /// </summary>
        /// <param name="geom">A geometry</param>
        /// <returns>A fixed geometry</returns>
        /// <seealso cref="Precision.GeometryPrecisionReducer"/>
        private Geometry FixTopology(Geometry geom)
        {
            // TODO: do this in a better way (could use GeometryPrecisionReducer?)
            // TODO: ensure this meets required precision model (see GeometryPrecisionReducer)
            return geom.Buffer(0d);
        }

        public Geometry ClipCollection(Geometry geom)
        {
            if (IsOutsideRectangle(geom)) return null;
            // TODO: need to precision reduce
            if (IsInsideRectangle(geom)) return geom.Copy();

            var geomsClip = new List<Geometry>();
            for (int i = 0; i < geom.NumGeometries; i++)
            {
                var item = geom.GetGeometryN(i);
                if (!(item is Polygon poly)) continue;
                var polyClip = ClipPolygon(poly);
                if (polyClip == null) continue;
                geomsClip.Add(polyClip);
            }
    
            if (geomsClip.Count == 0) {
                return null;
            }
            var geomClip = geom.Factory.BuildGeometry(geomsClip);
            return geomClip;
        }

        private Polygon ClipPolygon(Polygon poly)
        {
            if (IsOutsideRectangle(poly)) return null;
            // TODO: need to precision reduce
            if (IsInsideRectangle(poly)) return (Polygon)poly.Copy();

            var shell = (LinearRing)poly.ExteriorRing;
            var shellClip = ClipRing(shell);
            if (shellClip == null)
            {
                return null;
            }

            var holesClip = ClipHoles(poly);

            var polyClip = poly.Factory.CreatePolygon(shellClip, holesClip);
            return polyClip;
        }

        private LinearRing[] ClipHoles(Polygon poly)
        {
            var holesClip = new List<LinearRing>();
            for (int i = 0; i < poly.NumInteriorRings; i++)
            {
                var holeClip = ClipRing((LinearRing)poly.GetInteriorRingN(i));
                if (holeClip != null)
                {
                    holesClip.Add(holeClip);
                }
            }
            return holesClip.ToArray();
        }

        private LinearRing ClipRing(LinearRing ring)
        {
            if (IsOutsideRectangle(ring)) return null;
            // TODO: need to precision reduce
            if (IsInsideRectangle(ring)) return (LinearRing)ring.Copy();

            var pts = ClipRingToBox(ring.Coordinates);
            // check for a collapsed ring
            if (pts == null || pts.Length < 4) return null;
            return ring.Factory.CreateLinearRing(pts);
        }

        private bool IsInsideRectangle(Geometry geom)
        {
            return _clipEnv.Covers(geom.EnvelopeInternal);
        }

        private bool IsOutsideRectangle(Geometry geom)
        {
            return !_clipEnv.Intersects(geom.EnvelopeInternal);
        }


        /// <summary>
        /// Clips ring to rectangle box.
        /// This follows the Sutherland-Hodgson algorithm.
        /// </summary>
        /// <param name="ring"></param>
        /// <returns>the clipped points, or <c>null</c> if all were clipped</returns>
        private Coordinate[] ClipRingToBox(Coordinate[] ring)
        {
            var coords = ring;
            for (int edgeIndex = 0; edgeIndex < 4; edgeIndex++)
            {
                /*
                // this is a further optimization to clip entire line
                // but not clear it makes much difference
                if (edgeIndex >= 1 && isInsideEdge(currentCoordsEnv, edgeIndex))
                    // all pts inside - skip clipping against this edge
                    continue;
                */

                //currentCoordsEnv = new Envelope();
                coords = ClipRingToBoxEdge(coords, edgeIndex);
                // check if all points clipped off
                if (coords == null) return null;

                //if (isOutsideEdge(currentCoordsEnv, edgeIndex)) return null;
            }
            return coords;
        }

        /*
        private bool IsInsideEdge(Envelope env, int edgeIndex)
        {
            switch (edgeIndex)
            {
                case ENV_BOTTOM:
                    return env.MinY > _clipEnvMinY;
                case ENV_RIGHT:
                    return env.MaxX < _clipEnvMaxX;
                case ENV_TOP:
                    return env.MaxY < _clipEnvMaxY;
                case ENV_LEFT:
                default:
                    return env.MinX > _clipEnvMinX;
            }
        }

        private bool IsOutsideEdge(Envelope env, int edgeIndex)
        {
            switch (edgeIndex)
            {
                case ENV_BOTTOM:
                    return env.MaxY < _clipEnvMinY;
                case ENV_RIGHT:
                    return env.MinX > _clipEnvMaxX;
                case ENV_TOP:
                    return env.MinY > _clipEnvMaxY;
                case ENV_LEFT:
                default:
                    return env.MaxX < _clipEnvMinX;
            }
        }

        Envelope currentCoordsEnv;
        */

        /// <summary>
        /// Clips ring to a axis-parallel line defined by a single box edge.
        /// </summary>
        /// <param name="coords">An array of coordinates</param>
        /// <param name="edgeIndex">An edge index</param>
        /// <returns>The clipped points, or <c>null</c> if all were clipped</returns>
        private Coordinate[] ClipRingToBoxEdge(Coordinate[] coords, int edgeIndex)
        {
            var clipCoords = new CoordinateList();

            var p0 = coords[coords.Length - 1];
            for (int i = 0; i < coords.Length; i++)
            {
                var p1 = coords[i];
                if (IsInsideEdge(p1, edgeIndex))
                {
                    if (!IsInsideEdge(p0, edgeIndex))
                    {
                        var intPt = IntersectionPrecise(p0, p1, edgeIndex);
                        clipCoords.Add(intPt, false);
                        //currentCoordsEnv.expandToInclude(intPt);
                    }
                    // TODO: avoid copying so much?
                    var p1Precise = MakePrecise(p1.Copy());
                    clipCoords.Add(p1Precise, false);
                    //currentCoordsEnv.expandToInclude(p1Precise);
                }
                else if (IsInsideEdge(p0, edgeIndex))
                {
                    var intPt = IntersectionPrecise(p0, p1, edgeIndex);
                    clipCoords.Add(intPt, false);
                    //currentCoordsEnv.expandToInclude(intPt);
                }
                // move to next segment
                p0 = p1;
            }
            // check if all points clipped off
            if (clipCoords.Count <= 0) return null;

            clipCoords.CloseRing();
            return clipCoords.ToCoordinateArray();
        }

        private Coordinate MakePrecise(Coordinate coord)
        {
            if (_precModel == null)
                return coord;
            _precModel.MakePrecise(coord);
            return coord;
        }

        private Coordinate IntersectionPrecise(Coordinate a, Coordinate b, int edgeIndex)
        {
            return MakePrecise(Intersection(a, b, edgeIndex));
        }

        // TODO: test that intersection computatin is robust
        // e.g. how are nearly horizontal/vertical lines handled?
        /// <summary>
        /// <para/>
        /// Due to the nature of the S-H algorithm,
        /// it should never happen that the
        /// computation of intersection line slope is infinite
        /// (i.e.encounters division-by-zero).
        /// </summary>
        /// <param name="a">A coordinate</param>
        /// <param name="b">A coordinate</param>
        /// <param name="edgeIndex">An index of an edge</param>
        /// <returns>A coordinate</returns>
        private Coordinate Intersection(Coordinate a, Coordinate b, int edgeIndex)
        {
            switch (edgeIndex)
            {
                case ENV_BOTTOM:
                    return new Coordinate(IntersectionLineY(a, b, _clipEnvMinY), _clipEnvMinY);
                case ENV_RIGHT:
                    return new Coordinate(_clipEnvMaxX, IntersectionLineX(a, b, _clipEnvMaxX));
                case ENV_TOP:
                    return new Coordinate(IntersectionLineY(a, b, _clipEnvMaxY), _clipEnvMaxY);
                case ENV_LEFT:
                default:
                    return new Coordinate(_clipEnvMinX, IntersectionLineX(a, b, _clipEnvMinX));
            }
        }

        private double IntersectionLineY(Coordinate a, Coordinate b, double y)
        {
            // short-circuit segment parallel to Y axis
            if (b.X == a.X) return a.X;

            double m = (b.X - a.X) / (b.Y - a.Y);
            double intercept = (y - a.Y) * m;
            return a.X + intercept;
        }

        private double IntersectionLineX(Coordinate a, Coordinate b, double x)
        {
            // short-circuit segment parallel to X axis
            if (b.Y == a.Y) return a.Y;

            double m = (b.Y - a.Y) / (b.X - a.X);
            double intercept = (x - a.X) * m;
            return a.Y + intercept;
        }

        private bool IsInsideEdge(Coordinate p, int edgeIndex)
        {
            switch (edgeIndex)
            {
                case ENV_BOTTOM: // bottom
                    return p.Y > _clipEnvMinY;
                case ENV_RIGHT: // right
                    return p.X < _clipEnvMaxX;
                case ENV_TOP: // top
                    return p.Y < _clipEnvMaxY;
                case ENV_LEFT:
                default: // left
                    return p.X > _clipEnvMinX;
            }
        }

    }
}
