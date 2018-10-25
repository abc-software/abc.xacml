// ----------------------------------------------------------------------------
// <copyright file="TypeExtensions.cs" company="ABC Software Ltd">
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
    using System.Collections.Generic;
#if NET40 || NET45
    using System.ComponentModel.Composition;
#endif
#if NETSTANDARD1_6 || NETSTANDARD2_0
    using System.Composition;
#endif
    using Abc.Xacml.Interfaces;
    using Abc.Xacml.Runtime;

    /// <summary>
    /// Geo XACML types.
    /// </summary>
    /// <seealso cref="Abc.Xacml.Interfaces.ITypesExtender" />
    [Export(typeof(ITypesExtender))]
    public class TypeExtension : ITypesExtender {
        /// <inheritdoc/>
        public IDictionary<string, TypeConverterWrapper> GetExtensionTypes() {
            return new Dictionary<string, TypeConverterWrapper>()
            {
                {"urn:ogc:def:dataType:geoxacml:1.0:geometry", new TypeConverterWrapper(new GeometryConverter(), typeof(Geometry))}
            };
        }
    }
}
