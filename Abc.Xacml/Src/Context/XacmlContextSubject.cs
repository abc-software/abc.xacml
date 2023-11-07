// ----------------------------------------------------------------------------
// <copyright file="XacmlContextSubject.cs" company="ABC Software Ltd">
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

namespace Abc.Xacml.Context {
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The <c>XacmlContextSubject</c> class contains the value of an attribute.
    /// </summary>
    /// <remarks>See the xacml-context:Subject element defined in [XacmlCore, 6.2] for more details.</remarks>
    public class XacmlContextSubject : XacmlContextBase {
        private static readonly Uri DefaultSubjectCategory = new Uri("urn:oasis:names:tc:xacml:1.0:subject-category:access-subject");
        private Uri subjectCategory = DefaultSubjectCategory;

        /// <summary>
        /// Initializes a new instance of the <see cref="XacmlContextSubject"/> class.
        /// </summary>
        public XacmlContextSubject()
            : base() {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XacmlContextSubject"/> class.
        /// </summary>
        /// <param name="attributes">The attribute.</param>
        public XacmlContextSubject(XacmlContextAttribute attribute)
            : this(new XacmlContextAttribute[] { attribute }) {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XacmlContextSubject"/> class.
        /// </summary>
        /// <param name="attributes">The attributes.</param>
        public XacmlContextSubject(IEnumerable<XacmlContextAttribute> attributes)
            : base(attributes) {
        }

        /// <summary>
        /// Gets or sets the subject category.
        /// </summary>
        /// <value>
        /// The subject category.
        /// </value>
        public Uri SubjectCategory {
            get {
                return this.subjectCategory;
            }

            set {
                if (value == null) {
                    this.subjectCategory = DefaultSubjectCategory;
                }
                else {
                    this.subjectCategory = value;
                }
            }
        }
    }
}
