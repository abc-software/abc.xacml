// ----------------------------------------------------------------------------
// <copyright file="Geometry.cs" company="ABC Software Ltd">
//    Copyright © 2018 ABC Software Ltd. All rights reserved.
//
//    This library is free software; you can redistribute it and/or.
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

namespace Abc.Xacml.Geo {
    using System;
    using System.ComponentModel;
    using GeoAPI.Geometries;
    using NetTopologySuite.IO.GML2;

    /// <summary>
    /// The geometry type.
    /// </summary>
    /// <seealso cref="System.IEquatable{Geometry}" />
    [TypeConverter(typeof(GeometryConverter))]
    [ImmutableObject(true)]
    public sealed class Geometry : IEquatable<Geometry> {
        /// <summary>
        /// Initializes a new instance of the <see cref="Geometry"/> class.
        /// </summary>
        /// <param name="geometry">The geometry.</param>
        public Geometry(IGeometry geometry) {
            this.Value = geometry;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Geometry"/> class.
        /// </summary>
        /// <param name="gml">The GML.</param>
        public Geometry(string gml) {
            GMLReader gmlReader = new GMLReader();
            this.Value = gmlReader.Read(gml);
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public IGeometry Value { get; private set; }

        /// <inheritdoc/>
        public bool Equals(Geometry other) {
            return this == other;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) {
            Geometry t = obj as Geometry;
            if (t != null) {
                return this.Equals(t);
            }
            else {
                return false;
            }
        }

        /// <inheritdoc/>
        public override int GetHashCode() {
            return this.Value.GetHashCode();
        }

        /// <inheritdoc/>
        public override string ToString() {
            return this.Value.ToString();
        }
    }
}
