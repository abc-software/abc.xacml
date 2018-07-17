// ----------------------------------------------------------------------------
// <copyright file="XacmlCombinerParameters.cs" company="ABC Software Ltd">
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

namespace Abc.Xacml.Policy {
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    /// <summary>
    /// The <c>XacmlCombinerParameters</c> class element conveys parameters for a policy- or rule-combining algorithm.
    /// </summary>
    /// <remarks>See the xacml:CombinerParameters element defined in [XacmlCore3, 5.16] for more details.</remarks>
    /// <remarks>
    /// Used only from XACML 3.0
    /// </remarks>
    public class XacmlCombinerParameters {
        private readonly ICollection<XacmlCombinerParameter> combinerParameters = new Collection<XacmlCombinerParameter>();

        /// <summary>
        /// Initializes a new instance of the <see cref="XacmlCombinerParameters"/> class.
        /// </summary>
        public XacmlCombinerParameters() {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XacmlCombinerParameters" /> class.
        /// </summary>
        /// <param name="paramaters">The paramaters.</param>
        public XacmlCombinerParameters(IEnumerable<XacmlCombinerParameter> paramaters) {
            if (paramaters == null) {
                throw new ArgumentNullException(nameof(paramaters));
            }

            foreach (var item in paramaters) {
                this.combinerParameters.Add(item);
            }
        }

        /// <summary>
        /// Gets the combiner parameters.
        /// </summary>
        /// <value>
        /// The combiner parameters.
        /// </value>
        public ICollection<XacmlCombinerParameter> CombinerParameters {
            get {
                return this.combinerParameters;
            }
        }
    }
}
