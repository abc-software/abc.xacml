// ----------------------------------------------------------------------------
// <copyright file="Xacml10ProtocolSerializer.cs" company="ABC Software Ltd">
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
 
namespace Abc.Xacml {
    /// <summary>
    /// The XACML 1.0 Protocol serializer
    /// </summary>
    public class Xacml10ProtocolSerializer : XacmlProtocolSerializer {
        /// <summary>
        /// Initializes a new instance of the <see cref="Xacml10ProtocolSerializer"/> class.
        /// </summary>
        public Xacml10ProtocolSerializer()
            : base(XacmlVersion.Xacml10) {
        }
    }
}
