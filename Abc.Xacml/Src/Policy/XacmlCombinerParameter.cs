// ----------------------------------------------------------------------------
// <copyright file="XacmlCombinerParameter.cs" company="ABC Software Ltd">
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
    /// The <c>XacmlCombinerParameter</c> class element conveys a single parameter for a policy- or rule-combining algorithm.
    /// </summary>
    /// <remarks>See the xacml:CombinerParameters element defined in [XacmlCore3, 5.17] for more details.</remarks>
    /// <remarks>
    /// Used only from XACML 3.0
    /// </remarks>
    public class XacmlCombinerParameter {
        private string parameterName;
        private XacmlAttributeValue attributeValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="XacmlCombinerParameter"/> class.
        /// </summary>
        /// <param name="parameterName">The identifier of the parameter.</param>
        /// <param name="attributeValue">The value of the parameter.</param>
        public XacmlCombinerParameter(string parameterName, XacmlAttributeValue attributeValue) {
            if (parameterName == null) {
                throw new ArgumentNullException(nameof(parameterName));
            }

            if (parameterName.Length == 0) {
                throw new ArgumentException("Value cannot be empty.", nameof(parameterName));
            }

            if (attributeValue == null) {
                throw new ArgumentNullException(nameof(attributeValue));
            }

            this.parameterName = parameterName;
            this.attributeValue = attributeValue;
        }

        /// <summary>
        /// Gets or sets the identifier of the parameter.
        /// </summary>
        /// <value>
        /// The identifier of the parameter.
        /// </value>
        public string ParameterName {
            get {
                return this.parameterName;
            }

            set {
                if (value == null) {
                    throw new ArgumentNullException(nameof(value));
                }

                if (value.Length == 0) {
                    throw new ArgumentException("Value cannot be empty.", nameof(value));
                }

                this.parameterName = value;
            }
        }

        /// <summary>
        /// Gets or sets the value of the parameter.
        /// </summary>
        /// <value>
        /// The value of the parameter.
        /// </value>
        public XacmlAttributeValue AttributeValue {
            get {
                return this.attributeValue;
            }

            set {
                if (value == null) {
                    throw new ArgumentNullException(nameof(value));
                }

                this.attributeValue = value;
            }
        }
    }
}
