/* Copyright 2010-2014 MongoDB Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using MongoDB.Bson;

namespace MongoDB.Driver.GeoJsonObjectModel
{
    /// <summary>
    /// A static class containing helper methods to create GeoJson objects.
    /// </summary>
    public static class GeoJson
    {
        // public static methods
        /// <summary>
        /// Creates a GeoJson bounding box.
        /// </summary>
        /// <typeparam name="TCoordinates">The type of the coordinates.</typeparam>
        /// <param name="min">The min.</param>
        /// <param name="max">The max.</param>
        /// <returns>A GeoJson bounding box.</returns>
        public static GeoJsonBoundingBox<TCoordinates> BoundingBox<TCoordinates>(TCoordinates min, TCoordinates max) where TCoordinates : GeoJsonCoordinates
        {
            return new GeoJsonBoundingBox<TCoordinates>(min, max);
        }

        /// <summary>
        /// Creates a GeoJson Feature object.
        /// </summary>
        /// <typeparam name="TCoordinates">The type of the coordinates.</typeparam>
        /// <param name="geometry">The geometry.</param>
        /// <returns>A GeoJson Feature object.</returns>
        public static GeoJsonFeature<TCoordinates> Feature<TCoordinates>(GeoJsonGeometry<TCoordinates> geometry) where TCoordinates : GeoJsonCoordinates
        {
            return new GeoJsonFeature<TCoordinates>(geometry);
        }

        /// <summary>
        /// Creates a GeoJson Feature object.
        /// </summary>
        /// <typeparam name="TCoordinates">The type of the coordinates.</typeparam>
        /// <param name="args">The additional args.</param>
        /// <param name="geometry">The geometry.</param>
        /// <returns>A GeoJson Feature object.</returns>
        public static GeoJsonFeature<TCoordinates> Feature<TCoordinates>(GeoJsonFeatureArgs<TCoordinates> args, GeoJsonGeometry<TCoordinates> geometry) where TCoordinates : GeoJsonCoordinates
        {
            return new GeoJsonFeature<TCoordinates>(args, geometry);
        }

        /// <summary>
        /// Creates a GeoJson FeatureCollection object.
        /// </summary>
        /// <typeparam name="TCoordinates">The type of the coordinates.</typeparam>
        /// <param name="args">The additional args.</param>
        /// <param name="features">The features.</param>
        /// <returns>A GeoJson FeatureCollection object.</returns>
        public static GeoJsonFeatureCollection<TCoordinates> FeatureCollection<TCoordinates>(GeoJsonObjectArgs<TCoordinates> args, params GeoJsonFeature<TCoordinates>[] features) where TCoordinates : GeoJsonCoordinates
        {
            return new GeoJsonFeatureCollection<TCoordinates>(args, features);
        }

        /// <summary>
        /// Creates a GeoJson FeatureCollection object.
        /// </summary>
        /// <typeparam name="TCoordinates">The type of the coordinates.</typeparam>
        /// <param name="features">The features.</param>
        /// <returns>A GeoJson FeatureCollection object.</returns>
        public static GeoJsonFeatureCollection<TCoordinates> FeatureCollection<TCoordinates>(params GeoJsonFeature<TCoordinates>[] features) where TCoordinates : GeoJsonCoordinates
        {
            return new GeoJsonFeatureCollection<TCoordinates>(features);
        }

        /// <summary>
        /// Creates a GeoJson 2D geographic position (longitude, latitude).
        /// </summary>
        /// <param name="longitude">The longitude.</param>
        /// <param name="latitude">The latitude.</param>
        /// <returns>A GeoJson 2D geographic position.</returns>
        public static GeoJson2DGeographicCoordinates Geographic(double longitude, double latitude)
        {
            return new GeoJson2DGeographicCoordinates(longitude, latitude);
        }

        /// <summary>
        /// Creates a GeoJson 3D geographic position (longitude, latitude, altitude).
        /// </summary>
        /// <param name="longitude">The longitude.</param>
        /// <param name="latitude">The latitude.</param>
        /// <param name="altitude">The altitude.</param>
        /// <returns>A GeoJson 3D geographic position.</returns>
        public static GeoJson3DGeographicCoordinates Geographic(double longitude, double latitude, double altitude)
        {
            return new GeoJson3DGeographicCoordinates(longitude, latitude, altitude);
        }

        /// <summary>
        /// Creates a GeoJson GeometryCollection object.
        /// </summary>
        /// <typeparam name="TCoordinates">The type of the coordinates.</typeparam>
        /// <param name="args">The additional args.</param>
        /// <param name="geometries">The geometries.</param>
        /// <returns>A GeoJson GeometryCollection object.</returns>
        public static GeoJsonGeometryCollection<TCoordinates> GeometryCollection<TCoordinates>(GeoJsonObjectArgs<TCoordinates> args, params GeoJsonGeometry<TCoordinates>[] geometries) where TCoordinates : GeoJsonCoordinates
        {
            return new GeoJsonGeometryCollection<TCoordinates>(args, geometries);
        }

        /// <summary>
        /// Creates a GeoJson GeometryCollection object.
        /// </summary>
        /// <typeparam name="TCoordinates">The type of the coordinates.</typeparam>
        /// <param name="geometries">The geometries.</param>
        /// <returns>A GeoJson GeometryCollection object.</returns>
        public static GeoJsonGeometryCollection<TCoordinates> GeometryCollection<TCoordinates>(params GeoJsonGeometry<TCoordinates>[] geometries) where TCoordinates : GeoJsonCoordinates
        {
            return new GeoJsonGeometryCollection<TCoordinates>(geometries);
        }

        /// <summary>
        /// Creates the coordinates of a GeoJson linear ring.
        /// </summary>
        /// <typeparam name="TCoordinates">The type of the coordinates.</typeparam>
        /// <param name="positions">The positions.</param>
        /// <returns>The coordinates of a GeoJson linear ring.</returns>
        public static GeoJsonLinearRingCoordinates<TCoordinates> LinearRingCoordinates<TCoordinates>(params TCoordinates[] positions) where TCoordinates : GeoJsonCoordinates
        {
            return new GeoJsonLinearRingCoordinates<TCoordinates>(positions);
        }

        /// <summary>
        /// Creates a GeoJson LineString object.
        /// </summary>
        /// <typeparam name="TCoordinates">The type of the coordinates.</typeparam>
        /// <param name="args">The additional args.</param>
        /// <param name="positions">The positions.</param>
        /// <returns>A GeoJson LineString object.</returns>
        public static GeoJsonLineString<TCoordinates> LineString<TCoordinates>(GeoJsonObjectArgs<TCoordinates> args, params TCoordinates[] positions) where TCoordinates : GeoJsonCoordinates
        {
            var coordinates = new GeoJsonLineStringCoordinates<TCoordinates>(positions);
            return new GeoJsonLineString<TCoordinates>(args, coordinates);
        }

        /// <summary>
        /// Creates a GeoJson LineString object.
        /// </summary>
        /// <typeparam name="TCoordinates">The type of the coordinates.</typeparam>
        /// <param name="positions">The positions.</param>
        /// <returns>A GeoJson LineString object.</returns>
        public static GeoJsonLineString<TCoordinates> LineString<TCoordinates>(params TCoordinates[] positions) where TCoordinates : GeoJsonCoordinates
        {
            var coordinates = new GeoJsonLineStringCoordinates<TCoordinates>(positions);
            return new GeoJsonLineString<TCoordinates>(coordinates);
        }

        /// <summary>
        /// Creates the coordinates of a GeoJson LineString.
        /// </summary>
        /// <typeparam name="TCoordinates">The type of the coordinates.</typeparam>
        /// <param name="positions">The positions.</param>
        /// <returns>The coordinates of a GeoJson LineString.</returns>
        public static GeoJsonLineStringCoordinates<TCoordinates> LineStringCoordinates<TCoordinates>(params TCoordinates[] positions) where TCoordinates : GeoJsonCoordinates
        {
            return new GeoJsonLineStringCoordinates<TCoordinates>(positions);
        }

        /// <summary>
        /// Creates a GeoJson MultiLineString object.
        /// </summary>
        /// <typeparam name="TCoordinates">The type of the coordinates.</typeparam>
        /// <param name="args">The additional args.</param>
        /// <param name="lineStrings">The line strings.</param>
        /// <returns>A GeoJson MultiLineString object.</returns>
        public static GeoJsonMultiLineString<TCoordinates> MultiLineString<TCoordinates>(GeoJsonObjectArgs<TCoordinates> args, params GeoJsonLineStringCoordinates<TCoordinates>[] lineStrings) where TCoordinates : GeoJsonCoordinates
        {
            var coordinates = new GeoJsonMultiLineStringCoordinates<TCoordinates>(lineStrings);
            return new GeoJsonMultiLineString<TCoordinates>(args, coordinates);
        }

        /// <summary>
        /// Creates a GeoJson MultiLineString object.
        /// </summary>
        /// <typeparam name="TCoordinates">The type of the coordinates.</typeparam>
        /// <param name="lineStrings">The line strings.</param>
        /// <returns>A GeoJson MultiLineString object.</returns>
        public static GeoJsonMultiLineString<TCoordinates> MultiLineString<TCoordinates>(params GeoJsonLineStringCoordinates<TCoordinates>[] lineStrings) where TCoordinates : GeoJsonCoordinates
        {
            var coordinates = new GeoJsonMultiLineStringCoordinates<TCoordinates>(lineStrings);
            return new GeoJsonMultiLineString<TCoordinates>(coordinates);
        }

        /// <summary>
        /// Creates a GeoJson MultiPoint object.
        /// </summary>
        /// <typeparam name="TCoordinates">The type of the coordinates.</typeparam>
        /// <param name="args">The additional args.</param>
        /// <param name="positions">The positions.</param>
        /// <returns>A GeoJson MultiPoint object.</returns>
        public static GeoJsonMultiPoint<TCoordinates> MultiPoint<TCoordinates>(GeoJsonObjectArgs<TCoordinates> args, params TCoordinates[] positions) where TCoordinates : GeoJsonCoordinates
        {
            var coordinates = new GeoJsonMultiPointCoordinates<TCoordinates>(positions);
            return new GeoJsonMultiPoint<TCoordinates>(args, coordinates);
        }

        /// <summary>
        /// Creates a GeoJson MultiPoint object.
        /// </summary>
        /// <typeparam name="TCoordinates">The type of the coordinates.</typeparam>
        /// <param name="positions">The positions.</param>
        /// <returns>A GeoJson MultiPoint object.</returns>
        public static GeoJsonMultiPoint<TCoordinates> MultiPoint<TCoordinates>(params TCoordinates[] positions) where TCoordinates : GeoJsonCoordinates
        {
            var coordinates = new GeoJsonMultiPointCoordinates<TCoordinates>(positions);
            return new GeoJsonMultiPoint<TCoordinates>(coordinates);
        }

        /// <summary>
        /// Creates a GeoJson MultiPolygon object.
        /// </summary>
        /// <typeparam name="TCoordinates">The type of the coordinates.</typeparam>
        /// <param name="args">The additional args.</param>
        /// <param name="polygons">The polygons.</param>
        /// <returns>A GeoJson MultiPolygon object.</returns>
        public static GeoJsonMultiPolygon<TCoordinates> MultiPolygon<TCoordinates>(GeoJsonObjectArgs<TCoordinates> args, params GeoJsonPolygonCoordinates<TCoordinates>[] polygons) where TCoordinates : GeoJsonCoordinates
        {
            var coordinates = new GeoJsonMultiPolygonCoordinates<TCoordinates>(polygons);
            return new GeoJsonMultiPolygon<TCoordinates>(args, coordinates);
        }

        /// <summary>
        /// Creates a GeoJson MultiPolygon object.
        /// </summary>
        /// <typeparam name="TCoordinates">The type of the coordinates.</typeparam>
        /// <param name="polygons">The polygons.</param>
        /// <returns>A GeoJson MultiPolygon object.</returns>
        public static GeoJsonMultiPolygon<TCoordinates> MultiPolygon<TCoordinates>(params GeoJsonPolygonCoordinates<TCoordinates>[] polygons) where TCoordinates : GeoJsonCoordinates
        {
            var coordinates = new GeoJsonMultiPolygonCoordinates<TCoordinates>(polygons);
            return new GeoJsonMultiPolygon<TCoordinates>(coordinates);
        }

        /// <summary>
        /// Creates a GeoJson Point object.
        /// </summary>
        /// <typeparam name="TCoordinates">The type of the coordinates.</typeparam>
        /// <param name="args">The additional args.</param>
        /// <param name="coordinates">The coordinates.</param>
        /// <returns>A GeoJson Point object.</returns>
        public static GeoJsonPoint<TCoordinates> Point<TCoordinates>(GeoJsonObjectArgs<TCoordinates> args, TCoordinates coordinates) where TCoordinates : GeoJsonCoordinates
        {
            return new GeoJsonPoint<TCoordinates>(args, coordinates);
        }

        /// <summary>
        /// Creates a GeoJson Point object.
        /// </summary>
        /// <typeparam name="TCoordinates">The type of the coordinates.</typeparam>
        /// <param name="coordinates">The coordinates.</param>
        /// <returns>A GeoJson Point object.</returns>
        public static GeoJsonPoint<TCoordinates> Point<TCoordinates>(TCoordinates coordinates) where TCoordinates : GeoJsonCoordinates
        {
            return new GeoJsonPoint<TCoordinates>(coordinates);
        }

        /// <summary>
        /// Creates a GeoJson Polygon object.
        /// </summary>
        /// <typeparam name="TCoordinates">The type of the coordinates.</typeparam>
        /// <param name="args">The additional args.</param>
        /// <param name="positions">The positions.</param>
        /// <returns>A GeoJson Polygon object.</returns>
        public static GeoJsonPolygon<TCoordinates> Polygon<TCoordinates>(GeoJsonObjectArgs<TCoordinates> args, params TCoordinates[] positions) where TCoordinates : GeoJsonCoordinates
        {
            var exterior = new GeoJsonLinearRingCoordinates<TCoordinates>(positions);
            var coordinates = new GeoJsonPolygonCoordinates<TCoordinates>(exterior);
            return new GeoJsonPolygon<TCoordinates>(args, coordinates);
        }

        /// <summary>
        /// Creates a GeoJson Polygon object.
        /// </summary>
        /// <typeparam name="TCoordinates">The type of the coordinates.</typeparam>
        /// <param name="args">The additional args.</param>
        /// <param name="coordinates">The coordinates.</param>
        /// <returns>A GeoJson Polygon object.</returns>
        public static GeoJsonPolygon<TCoordinates> Polygon<TCoordinates>(GeoJsonObjectArgs<TCoordinates> args, GeoJsonPolygonCoordinates<TCoordinates> coordinates) where TCoordinates : GeoJsonCoordinates
        {
            return new GeoJsonPolygon<TCoordinates>(args, coordinates);
        }

        /// <summary>
        /// Creates a GeoJson Polygon object.
        /// </summary>
        /// <typeparam name="TCoordinates">The type of the coordinates.</typeparam>
        /// <param name="coordinates">The coordinates.</param>
        /// <returns>A GeoJson Polygon object.</returns>
        public static GeoJsonPolygon<TCoordinates> Polygon<TCoordinates>(GeoJsonPolygonCoordinates<TCoordinates> coordinates) where TCoordinates : GeoJsonCoordinates
        {
            return new GeoJsonPolygon<TCoordinates>(coordinates);
        }

        /// <summary>
        /// Creates a GeoJson Polygon object.
        /// </summary>
        /// <typeparam name="TCoordinates">The type of the coordinates.</typeparam>
        /// <param name="positions">The positions.</param>
        /// <returns>A GeoJson Polygon object.</returns>
        public static GeoJsonPolygon<TCoordinates> Polygon<TCoordinates>(params TCoordinates[] positions) where TCoordinates : GeoJsonCoordinates
        {
            var exterior = new GeoJsonLinearRingCoordinates<TCoordinates>(positions);
            var coordinates = new GeoJsonPolygonCoordinates<TCoordinates>(exterior);
            return new GeoJsonPolygon<TCoordinates>(coordinates);
        }

        /// <summary>
        /// Creates the coordinates of a GeoJson Polygon object.
        /// </summary>
        /// <typeparam name="TCoordinates">The type of the coordinates.</typeparam>
        /// <param name="positions">The positions.</param>
        /// <returns>The coordinates of a GeoJson Polygon object.</returns>
        public static GeoJsonPolygonCoordinates<TCoordinates> PolygonCoordinates<TCoordinates>(params TCoordinates[] positions) where TCoordinates : GeoJsonCoordinates
        {
            var exterior = new GeoJsonLinearRingCoordinates<TCoordinates>(positions);
            return new GeoJsonPolygonCoordinates<TCoordinates>(exterior);
        }

        /// <summary>
        /// Creates the coordinates of a GeoJson Polygon object.
        /// </summary>
        /// <typeparam name="TCoordinates">The type of the coordinates.</typeparam>
        /// <param name="exterior">The exterior.</param>
        /// <param name="holes">The holes.</param>
        /// <returns>The coordinates of a GeoJson Polygon object.</returns>
        public static GeoJsonPolygonCoordinates<TCoordinates> PolygonCoordinates<TCoordinates>(GeoJsonLinearRingCoordinates<TCoordinates> exterior, params GeoJsonLinearRingCoordinates<TCoordinates>[] holes) where TCoordinates : GeoJsonCoordinates
        {
            return new GeoJsonPolygonCoordinates<TCoordinates>(exterior, holes);
        }

        /// <summary>
        /// Creates a GeoJson 2D position (x, y).
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns>A GeoJson 2D position.</returns>
        public static GeoJson2DCoordinates Position(double x, double y)
        {
            return new GeoJson2DCoordinates(x, y);
        }

        /// <summary>
        /// Creates a GeoJson 3D position (x, y, z).
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="z">The z.</param>
        /// <returns>A GeoJson 3D position.</returns>
        public static GeoJson3DCoordinates Position(double x, double y, double z)
        {
            return new GeoJson3DCoordinates(x, y, z);
        }

        /// <summary>
        /// Creates a GeoJson 2D projected position (easting, northing).
        /// </summary>
        /// <param name="easting">The easting.</param>
        /// <param name="northing">The northing.</param>
        /// <returns>A GeoJson 2D projected position.</returns>
        public static GeoJson2DProjectedCoordinates Projected(double easting, double northing)
        {
            return new GeoJson2DProjectedCoordinates(easting, northing);
        }

        /// <summary>
        /// Creates a GeoJson 3D projected position (easting, northing, altitude).
        /// </summary>
        /// <param name="easting">The easting.</param>
        /// <param name="northing">The northing.</param>
        /// <param name="altitude">The altitude.</param>
        /// <returns>A GeoJson 3D projected position.</returns>
        public static GeoJson3DProjectedCoordinates Projected(double easting, double northing, double altitude)
        {
            return new GeoJson3DProjectedCoordinates(easting, northing, altitude);
        }
    }
}
