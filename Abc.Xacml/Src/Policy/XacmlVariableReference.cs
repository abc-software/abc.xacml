// ----------------------------------------------------------------------------
// <copyright file="XacmlVariableReference.cs" company="ABC Software Ltd">
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
 
namespace Abc.Xacml.Policy {
    using System;

    /// <summary>
    /// The <c>XacmlVariableReference</c> class element is used to reference a value defined within the same encompassing &lt;Policy&gt; element.
    /// </summary>
    /// <remarks>See the xacml:VariableReference element defined in [XacmlCore3, 5.24] for more details.</remarks>
    /// <remarks>
    /// Used only from XACML 3.0
    /// </remarks>
    public class XacmlVariableReference : IXacmlApply {
        private string variableReference;

        /// <summary>
        /// Initializes a new instance of the <see cref="XacmlVariableReference"/> class.
        /// </summary>
        /// <param name="variableReference">The variable reference.</param>
        public XacmlVariableReference(string variableReference) {
            if (variableReference == null) {
                throw new ArgumentNullException(nameof(variableReference));
            }

            if (variableReference.Length == 0) {
                throw new ArgumentException("Value cannot be empty.", nameof(variableReference));
            }

            this.variableReference = variableReference;
        }

        /// <summary>
        /// Gets or sets the variable reference.
        /// </summary>
        /// <value>
        /// The variable reference.
        /// </value>
        public string VariableReference {
            get {
                return this.variableReference;
            }

            set {
                if (variableReference == null) {
                    throw new ArgumentNullException(nameof(variableReference));
                }

                if (variableReference.Length == 0) {
                    throw new ArgumentException("Value cannot be empty.", nameof(variableReference));
                }

                this.variableReference = value;
            }
        }
    }
}
