// ----------------------------------------------------------------------------
// <copyright file="FunctionExtensions.cs" company="ABC Software Ltd">
//    Copyright © 2015 ABC Software Ltd. All rights reserved.
//
//    This library is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Lesser General Public
//    License  as published by the Free Software Foundation, either 
//    version 3 of the License, or (at your option) any later version. 
//
//    This library is distributed in the hope that it will be useful, 
//    but WITHOUT ANY WARRANTY; without even the implied warranty of 
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU 
//    Lesser General Public License for more details.
//
//    You should have received a copy of the GNU Lesser General Public 
//    License along with the library. If not, see http://www.gnu.org/licenses/.
// </copyright>
// ----------------------------------------------------------------------------

namespace Abc.Xacml {
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Linq;
    using System.Reflection;
    using Abc.Xacml.Interfaces;
    using Abc.Xacml.Runtime;
    using Convertinator;
    using GeoAPI.Geometries;

    [Export(typeof(IFunctionsExtender))]
    public class FunctionExtensions : IFunctionsExtender {
        private static SortedDictionary<string, DelegateWrapper> functions = new SortedDictionary<string, DelegateWrapper>()
        {
            { "urn:ogc:def:function:geoxacml:1.0:geometry-equals", new DelegateWrapper(Delegate.CreateDelegate(typeof(Func<Geometry, Geometry, bool>), null, typeof(FunctionExtensions).GetMethod("Equals", BindingFlags.Static | BindingFlags.Public)))},
            { "urn:ogc:def:function:geoxacml:1.0:geometry-disjoint", new DelegateWrapper(Delegate.CreateDelegate(typeof(Func<Geometry, Geometry, bool>), null, typeof(FunctionExtensions).GetMethod("Disjoint", BindingFlags.Static | BindingFlags.Public)))},
            { "urn:ogc:def:function:geoxacml:1.0:geometry-touches", new DelegateWrapper(Delegate.CreateDelegate(typeof(Func<Geometry, Geometry, bool>), null, typeof(FunctionExtensions).GetMethod("Touches", BindingFlags.Static | BindingFlags.Public)))},
            { "urn:ogc:def:function:geoxacml:1.0:geometry-crosses", new DelegateWrapper(Delegate.CreateDelegate(typeof(Func<Geometry, Geometry, bool>), null, typeof(FunctionExtensions).GetMethod("Crosses", BindingFlags.Static | BindingFlags.Public)))},
            { "urn:ogc:def:function:geoxacml:1.0:geometry-within", new DelegateWrapper(Delegate.CreateDelegate(typeof(Func<Geometry, Geometry, bool>), null, typeof(FunctionExtensions).GetMethod("Within", BindingFlags.Static | BindingFlags.Public)))},
            { "urn:ogc:def:function:geoxacml:1.0:geometry-contains", new DelegateWrapper(Delegate.CreateDelegate(typeof(Func<Geometry, Geometry, bool>), null, typeof(FunctionExtensions).GetMethod("Contains", BindingFlags.Static | BindingFlags.Public)))},
            { "urn:ogc:def:function:geoxacml:1.0:geometry-overlaps", new DelegateWrapper(Delegate.CreateDelegate(typeof(Func<Geometry, Geometry, bool>), null, typeof(FunctionExtensions).GetMethod("Overlaps", BindingFlags.Static | BindingFlags.Public)))},
            { "urn:ogc:def:function:geoxacml:1.0:geometry-intersects", new DelegateWrapper(Delegate.CreateDelegate(typeof(Func<Geometry, Geometry, bool>), null, typeof(FunctionExtensions).GetMethod("Intersects", BindingFlags.Static | BindingFlags.Public)))},
            
            { "urn:ogc:def:function:geoxacml:1.0:geometry-buffer", new DelegateWrapper(Delegate.CreateDelegate(typeof(Func<Geometry, double, IEnumerable<Geometry>>), null, typeof(FunctionExtensions).GetMethod("Buffer", BindingFlags.Static | BindingFlags.Public)))},
            { "urn:ogc:def:function:geoxacml:1.0:geometry-boundary", new DelegateWrapper(Delegate.CreateDelegate(typeof(Func<Geometry, IEnumerable<Geometry>>), null, typeof(FunctionExtensions).GetMethod("Boundary", BindingFlags.Static | BindingFlags.Public)))},
            { "urn:ogc:def:function:geoxacml:1.0:geometry-convex-hull", new DelegateWrapper(Delegate.CreateDelegate(typeof(Func<Geometry, Geometry>), null, typeof(FunctionExtensions).GetMethod("ConvexHull", BindingFlags.Static | BindingFlags.Public)))},
            { "urn:ogc:def:function:geoxacml:1.0:geometry-centroid", new DelegateWrapper(Delegate.CreateDelegate(typeof(Func<Geometry, Geometry>), null, typeof(FunctionExtensions).GetMethod("Centroid", BindingFlags.Static | BindingFlags.Public)))},
            { "urn:ogc:def:function:geoxacml:1.0:geometry-difference", new DelegateWrapper(Delegate.CreateDelegate(typeof(Func<Geometry, Geometry, IEnumerable<Geometry>>), null, typeof(FunctionExtensions).GetMethod("Difference", BindingFlags.Static | BindingFlags.Public)))},
            { "urn:ogc:def:function:geoxacml:1.0:geometry-sym-difference", new DelegateWrapper(Delegate.CreateDelegate(typeof(Func<Geometry, Geometry, IEnumerable<Geometry>>), null, typeof(FunctionExtensions).GetMethod("SymDifference", BindingFlags.Static | BindingFlags.Public)))},
            { "urn:ogc:def:function:geoxacml:1.0:geometry-intersection", new DelegateWrapper(Delegate.CreateDelegate(typeof(Func<Geometry, Geometry, IEnumerable<Geometry>>), null, typeof(FunctionExtensions).GetMethod("Intersection", BindingFlags.Static | BindingFlags.Public)))},
            { "urn:ogc:def:function:geoxacml:1.0:geometry-union", new DelegateWrapper(Delegate.CreateDelegate(typeof(Func<Geometry, Geometry, IEnumerable<Geometry>>), null, typeof(FunctionExtensions).GetMethod("Union", BindingFlags.Static | BindingFlags.Public)))},
        
            { "urn:ogc:def:function:geoxacml:1.0:geometry-area", new DelegateWrapper(Delegate.CreateDelegate(typeof(Func<Geometry, double>), null, typeof(FunctionExtensions).GetMethod("Area", BindingFlags.Static | BindingFlags.Public)))},
            { "urn:ogc:def:function:geoxacml:1.0:geometry-distance", new DelegateWrapper(Delegate.CreateDelegate(typeof(Func<Geometry, Geometry, double>), null, typeof(FunctionExtensions).GetMethod("Distance", BindingFlags.Static | BindingFlags.Public)))},
            { "urn:ogc:def:function:geoxacml:1.0:geometry-is-within-distance", new DelegateWrapper(Delegate.CreateDelegate(typeof(Func<Geometry, Geometry, double, bool>), null, typeof(FunctionExtensions).GetMethod("IsWithinDistance", BindingFlags.Static | BindingFlags.Public)))},
            { "urn:ogc:def:function:geoxacml:1.0:geometry-length", new DelegateWrapper(Delegate.CreateDelegate(typeof(Func<Geometry, double>), null, typeof(FunctionExtensions).GetMethod("Length", BindingFlags.Static | BindingFlags.Public)))},

            { "urn:ogc:def:function:geoxacml:1.0:geometry-is-simple", new DelegateWrapper(Delegate.CreateDelegate(typeof(Func<Geometry, bool>), null, typeof(FunctionExtensions).GetMethod("IsSimple", BindingFlags.Static | BindingFlags.Public)))},
            { "urn:ogc:def:function:geoxacml:1.0:geometry-is-closed", new DelegateWrapper(Delegate.CreateDelegate(typeof(Func<Geometry, bool>), null, typeof(FunctionExtensions).GetMethod("IsClosed", BindingFlags.Static | BindingFlags.Public)))},
            { "urn:ogc:def:function:geoxacml:1.0:geometry-is-valid", new DelegateWrapper(Delegate.CreateDelegate(typeof(Func<Geometry, bool>), null, typeof(FunctionExtensions).GetMethod("IsValid", BindingFlags.Static | BindingFlags.Public)))},

            { "urn:ogc:def:function:geoxacml:1.0:geometry-one-and-only", new DelegateWrapper(Delegate.CreateDelegate(typeof(Func<IEnumerable<Geometry>, Geometry>), null, FunctionsProcessor.GetGenericMethodInfo(typeof(FunctionsProcessor), "OneAndOnly", typeof(Geometry), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) }))) },
            { "urn:ogc:def:function:geoxacml:1.0:geometry-bag-size", new DelegateWrapper(Delegate.CreateDelegate(typeof(Func<IEnumerable<Geometry>, int>), null, FunctionsProcessor.GetGenericMethodInfo(typeof(FunctionsProcessor), "BagSize", typeof(Geometry), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) }))) },
            { "urn:ogc:def:function:geoxacml:1.0:geometry-is-in", new DelegateWrapper(Delegate.CreateDelegate(typeof(Func<Geometry, IEnumerable<Geometry>, bool>), null, FunctionsProcessor.GetGenericMethodInfo(typeof(FunctionsProcessor), "TypeIsIn", typeof(Geometry), o => new Type[]{ o, typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) }))) },
            { "urn:ogc:def:function:geoxacml:1.0:geometry-bag", new DelegateWrapper(Delegate.CreateDelegate(typeof(FunctionsProcessor.ParamsToEnumerable<Geometry>), null, FunctionsProcessor.GetGenericMethodInfo(typeof(FunctionsProcessor), "CreateBag", typeof(Geometry), o => new Type[]{ o.MakeArrayType() }))) },
        
            { "urn:ogc:def:function:geoxacml:1.0:geometry-bag-intersection", new DelegateWrapper(Delegate.CreateDelegate(typeof(Func<IEnumerable<Geometry>, IEnumerable<Geometry>, IEnumerable<Geometry>>), null, FunctionsProcessor.GetGenericMethodInfo(typeof(FunctionsProcessor), "TypeIntersection", typeof(Geometry), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}), typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) }))) },
            { "urn:ogc:def:function:geoxacml:1.0:geometry-at-least-one-member-of", new DelegateWrapper(Delegate.CreateDelegate(typeof(Func<IEnumerable<Geometry>, IEnumerable<Geometry>, bool>), null, FunctionsProcessor.GetGenericMethodInfo(typeof(FunctionsProcessor), "TypeAtLeastOneMemberOf", typeof(Geometry), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}), typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) }))) },
            { "urn:ogc:def:function:geoxacml:1.0:geometry-bag-union", new DelegateWrapper(Delegate.CreateDelegate(typeof(Func<IEnumerable<Geometry>, IEnumerable<Geometry>, IEnumerable<Geometry>>), null, FunctionsProcessor.GetGenericMethodInfo(typeof(FunctionsProcessor), "TypeUnion", typeof(Geometry), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}), typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) }))) },
            { "urn:ogc:def:function:geoxacml:1.0:geometry-bag-subset", new DelegateWrapper(Delegate.CreateDelegate(typeof(Func<IEnumerable<Geometry>, IEnumerable<Geometry>, bool>), null, FunctionsProcessor.GetGenericMethodInfo(typeof(FunctionsProcessor), "TypeSubset", typeof(Geometry), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}), typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) }))) },
            { "urn:ogc:def:function:geoxacml:1.0:geometry-set-equals", new DelegateWrapper(Delegate.CreateDelegate(typeof(Func<IEnumerable<Geometry>, IEnumerable<Geometry>, bool>), null, FunctionsProcessor.GetGenericMethodInfo(typeof(FunctionsProcessor), "TypeSetEquals", typeof(Geometry), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}), typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) }))) },
            
            { "urn:ogc:def:function:geoxacml:1.0:convert-to-metre", new DelegateWrapper(Delegate.CreateDelegate(typeof(Func<double, string, double>), null, typeof(FunctionExtensions).GetMethod("ConvertToMetre", BindingFlags.Static | BindingFlags.Public)))},
            { "urn:ogc:def:function:geoxacml:1.0:convert-to-square-metre", new DelegateWrapper(Delegate.CreateDelegate(typeof(Func<double, string, double>), null, typeof(FunctionExtensions).GetMethod("ConvertToSquareMetre", BindingFlags.Static | BindingFlags.Public)))},
        };

        public IDictionary<string, DelegateWrapper> GetExtensionFunctions() {
            return functions;
        }

        #region Topological Functions

        public static bool Contains(Geometry g1, Geometry g2) {
            return g1.Value.Contains(g2.Value);
        }

        public static bool Crosses(Geometry g1, Geometry g2) {
            return g1.Value.Crosses(g2.Value);
        }

        public static bool Disjoint(Geometry g1, Geometry g2) {
            return g1.Value.Disjoint(g2.Value);
        }

        public static bool Equals(Geometry g1, Geometry g2) {
            return g1.Value.EqualsTopologically(g2.Value);
        }

        public static bool Intersects(Geometry g1, Geometry g2) {
            return g1.Value.Intersects(g2.Value);
        }

        public static bool Overlaps(Geometry g1, Geometry g2) {
            return g1.Value.Overlaps(g2.Value);
        }

        public static bool Touches(Geometry g1, Geometry g2) {
            return g1.Value.Touches(g2.Value);
        }

        public static bool Within(Geometry g1, Geometry g2) {
            return g1.Value.Within(g2.Value);
        }

        #endregion

        #region Geometric Functions

        public static IEnumerable<Geometry> Buffer(Geometry g, double d) {
            return GeometryToList(g.Value.Buffer(d)).Select(o => new Geometry(o));
        }

        public static IEnumerable<Geometry> Boundary(Geometry g) {
            return GeometryToList(g.Value.Boundary).Select(o => new Geometry(o));
        }

        public static Geometry ConvexHull(Geometry g) {
            return new Geometry(g.Value.ConvexHull());
        }

        public static Geometry Centroid(Geometry g) {
            return new Geometry(g.Value.Centroid);
        }

        public static IEnumerable<Geometry> Difference(Geometry g1, Geometry g2) {
            return GeometryToList(g1.Value.Difference(g2.Value)).Select(o => new Geometry(o));
        }

        public static IEnumerable<Geometry> SymDifference(Geometry g1, Geometry g2) {
            return GeometryToList(g1.Value.SymmetricDifference(g2.Value)).Select(o => new Geometry(o));
        }

        public static IEnumerable<Geometry> Intersection(Geometry g1, Geometry g2) {
            return GeometryToList(g1.Value.Intersection(g2.Value)).Select(o => new Geometry(o));
        }

        public static IEnumerable<Geometry> Union(Geometry g1, Geometry g2) {
            return GeometryToList(g1.Value.Union(g2.Value)).Select(o => new Geometry(o));
        }

        #endregion

        #region Scalar Geometric Functions

        public static double Area(Geometry g) {
            return g.Value.Area;
        }

        public static double Distance(Geometry g1, Geometry g2) {
            return g1.Value.Distance(g2.Value);
        }

        public static bool IsWithinDistance(Geometry g1, Geometry g2, double d) {
            return g1.Value.IsWithinDistance(g2.Value, d);
        }

        public static double Length(Geometry g) {
            return g.Value.Length;
        }

        #endregion

        #region Miscellaneous Geometric Functions

        public static bool IsSimple(Geometry g) {
            return g.Value.IsSimple;
        }

        public static bool IsClosed(Geometry g) {
            return g.Value.IsRectangle;
        }

        public static bool IsValid(Geometry g) {
            return g.Value.IsSimple;
        }

        #endregion

        #region Conversion Functions

        public static double ConvertToMetre(double d, string unitMeasure) {
            Unit meter = new Unit("meter").IsAlsoCalled("metre").CanBeAbbreviated("m");
            Unit angstrom = new Unit("ångström").CanBeAbbreviated("Å");
            Unit nanometer = new Unit("nanometer").CanBeAbbreviated("nm");
            Unit astronomical_unit = new Unit("astronomical unit").CanBeAbbreviated("ua");
            Unit chain = new Unit("chain").CanBeAbbreviated("ch");
            Unit fathom = new Unit("fathom");
            Unit femtometer = new Unit("femtometer").CanBeAbbreviated("fm");
            Unit fermi = new Unit("fermi");
            Unit foot = new Unit("foot").CanBeAbbreviated("ft");
            Unit inch = new Unit("inch").CanBeAbbreviated("in");
            Unit light_year = new Unit("light year").CanBeAbbreviated("l. y.");
            Unit microinch = new Unit("microinch");
            Unit micrometer = new Unit("micrometer").CanBeAbbreviated("μm");
            Unit micron = new Unit("micron").CanBeAbbreviated("μ");
            Unit mil = new Unit("mil");
            Unit mile = new Unit("mile").CanBeAbbreviated("mi");
            Unit kilometer = new Unit("kilometer").CanBeAbbreviated("km");
            Unit parsec = new Unit("parsec").CanBeAbbreviated("pc");
            Unit pica = new Unit("pica");
            Unit millimeter = new Unit("millimeter").CanBeAbbreviated("mm");
            Unit point = new Unit("point");
            Unit rod = new Unit("rod").CanBeAbbreviated("rd");
            Unit yard = new Unit("yard").CanBeAbbreviated("yd");

            ConversionGraph system = new ConversionGraph();
            system.AddConversion(
                Conversions.From(angstrom).To(meter).MultiplyBy((decimal)1.0E-10),
                Conversions.From(angstrom).To(nanometer).MultiplyBy((decimal)1.0E-01),
                Conversions.From(astronomical_unit).To(meter).MultiplyBy((decimal)1.495979E+11),
                Conversions.From(chain).To(meter).MultiplyBy((decimal)2.011684E+1),
                Conversions.From(fathom).To(meter).MultiplyBy((decimal)1.828804E+00),
                Conversions.From(fermi).To(meter).MultiplyBy((decimal)1.0E-15),
                Conversions.From(fermi).To(femtometer).MultiplyBy((decimal)1.0E+00),
                Conversions.From(foot).To(meter).MultiplyBy((decimal)3.048E-01),
                Conversions.From(inch).To(meter).MultiplyBy((decimal)2.54E-02),
                Conversions.From(light_year).To(meter).MultiplyBy((decimal)9.46073E+15),
                Conversions.From(microinch).To(meter).MultiplyBy((decimal)2.54E-08),
                Conversions.From(microinch).To(micrometer).MultiplyBy((decimal)2.54E-02),

                Conversions.From(micron).To(meter).MultiplyBy((decimal)1.0E-06),
                Conversions.From(micron).To(micrometer).MultiplyBy((decimal)1.0E+00),
                Conversions.From(mil).To(meter).MultiplyBy((decimal)2.54E-05),
                Conversions.From(mil).To(millimeter).MultiplyBy((decimal)2.54E-02),
                Conversions.From(mile).To(meter).MultiplyBy((decimal)1.609344E+03),
                Conversions.From(mile).To(kilometer).MultiplyBy((decimal)1.609344E+00),
                Conversions.From(parsec).To(meter).MultiplyBy((decimal)3.085678E+16),
                Conversions.From(pica).To(meter).MultiplyBy((decimal)4.233333E-03),
                Conversions.From(pica).To(millimeter).MultiplyBy((decimal)4.233333E+00),
                Conversions.From(point).To(meter).MultiplyBy((decimal)3.527778E-04),
                Conversions.From(point).To(millimeter).MultiplyBy((decimal)3.527778E-01),
                Conversions.From(rod).To(meter).MultiplyBy((decimal)5.029210E+00),
                Conversions.From(yard).To(meter).MultiplyBy((decimal)9.144E-01));

            Measurement measurement = new Measurement(unitMeasure, (decimal)d);
            return (double)system.Convert(measurement, meter);
        }

        public static double ConvertToSquareMetre(double d, string unitMeasure) {
            Unit square_meter = new Unit("square meter").CanBeAbbreviated("m2", "m^2");
            Unit acre = new Unit("acre");
            Unit are = new Unit("are").CanBeAbbreviated("a");
            Unit barn = new Unit("barn").CanBeAbbreviated("b");
            Unit circular_mil = new Unit("circular mil");
            Unit square_millimeter = new Unit("square millimeter").CanBeAbbreviated("mm2", "mm^2");
            Unit foot_to_the_fourth_power = new Unit("foot to the fourth power").CanBeAbbreviated("ft4", "ft^4");
            Unit meter_to_the_fourth_power = new Unit("meter to the fourth power").CanBeAbbreviated("m4", "m^4");
            Unit hectare = new Unit("hectare").CanBeAbbreviated("ha");
            Unit inch_to_the_fourth_power = new Unit("inch to the fourth power").CanBeAbbreviated("in4", "in^4");
            Unit square_foot = new Unit("square foot").CanBeAbbreviated("ft2", "ft^2");
            Unit square_inch = new Unit("square inch").CanBeAbbreviated("in2", "in^2");
            Unit square_centimeter = new Unit("square centimeter").CanBeAbbreviated("cm2", "cm^2");
            Unit square_mile = new Unit("square mile").CanBeAbbreviated("mi2", "mi^2");
            Unit square_kilometer = new Unit("square kilometer").CanBeAbbreviated("km2", "km^2");
            Unit square_yard = new Unit("square yard ").CanBeAbbreviated("yd2", "yd^2");

            ConversionGraph system = new ConversionGraph();
            system.AddConversion(
                Conversions.From(acre).To(square_meter).MultiplyBy((decimal)4.046873E+03),
                Conversions.From(are).To(square_meter).MultiplyBy((decimal)1.0E+02),
                Conversions.From(barn).To(square_meter).MultiplyBy((decimal)1.0E-28),
                Conversions.From(circular_mil).To(square_meter).MultiplyBy((decimal)5.067075E-10),
                Conversions.From(circular_mil).To(square_millimeter).MultiplyBy((decimal)5.067075E-04),
                Conversions.From(foot_to_the_fourth_power).To(meter_to_the_fourth_power).MultiplyBy((decimal)8.630975E-03),
                Conversions.From(hectare).To(square_meter).MultiplyBy((decimal)1.0E+04),
                Conversions.From(inch_to_the_fourth_power).To(meter_to_the_fourth_power).MultiplyBy((decimal)4.162314E-07),
                Conversions.From(square_foot).To(square_meter).MultiplyBy((decimal)9.290304E-02),
                Conversions.From(square_inch).To(square_meter).MultiplyBy((decimal)6.4516E-04),
                Conversions.From(square_inch).To(square_centimeter).MultiplyBy((decimal)6.4516E+00),
                Conversions.From(square_mile).To(square_meter).MultiplyBy((decimal)2.589988E+06),
                Conversions.From(square_mile).To(square_kilometer).MultiplyBy((decimal)2.589988E+00),
                Conversions.From(square_yard).To(square_meter).MultiplyBy((decimal)8.361274E-01));

            Measurement measurement = new Measurement(unitMeasure, (decimal)d);
            return (double)system.Convert(measurement, square_meter);
        }

        #endregion

        public static IEnumerable<IGeometry> GeometryToList(IGeometry g) {
            IGeometryCollection collection = g as IGeometryCollection;
            if (collection != null && !(collection is IMultiPoint) && !(collection is IMultiLineString) && !(collection is IMultiCurve) && !(collection is IMultiPolygon)) {
                return collection;
            }
            else {
                return new List<IGeometry>()
                {
                    g
                };
            }
        }
    }
}
