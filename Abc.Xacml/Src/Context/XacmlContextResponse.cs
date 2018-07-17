// ----------------------------------------------------------------------------
// <copyright file="XacmlContextResponse.cs" company="ABC Software Ltd">
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

namespace Abc.Xacml.Context {
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    /// <summary>
    /// The <c>XacmlContextResponse</c> class encapsulates the authorization decision produced by the PDP.
    /// </summary>
    /// <remarks>See the xacml-context:Response element defined in [XacmlCore, 6.9] for more details.</remarks>
    public class XacmlContextResponse {
        private readonly ICollection<XacmlContextResult> results = new Collection<XacmlContextResult>();

        /// <summary>
        /// Initializes a new instance of the <see cref="XacmlContextResponse"/> class.
        /// </summary>
        /// <param name="results">The result.</param>
        public XacmlContextResponse(XacmlContextResult result)
            : this(new XacmlContextResult[] { result }) {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XacmlContextResponse"/> class.
        /// </summary>
        /// <param name="results">The results.</param>
        public XacmlContextResponse(IEnumerable<XacmlContextResult> results) {
            if (results == null) {
                throw new ArgumentNullException(nameof(results));
            }

            foreach (var item in results) {
                this.results.Add(item);
            }
        }

        /// <summary>
        /// Gets the results.
        /// </summary>
        public ICollection<XacmlContextResult> Results {
            get {
                return this.results;
            }
        }
    }
}