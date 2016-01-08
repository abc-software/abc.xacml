// ----------------------------------------------------------------------------
// <copyright file="XacmlMatch.cs" company="ABC Software Ltd">
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
    using System.Diagnostics.Contracts;

    /// <summary>
    /// class XacmlMatch
    /// </summary>
    public class XacmlMatch {
        private Uri matchId;
        private XacmlAttributeValue attributeValue;
        private XacmlAttributeDesignator attributeDesignator;
        private XacmlAttributeSelector attributeSelector;

        /// <summary>
        /// Initializes a new instance of the <see cref="XacmlMatch"/> class.
        /// </summary>
        /// <param name="matchId">The match identifier.</param>
        /// <param name="attributeValue">The attribute value.</param>
        /// <param name="attributeDesignator">The attribute designator.</param>
        public XacmlMatch(Uri matchId, XacmlAttributeValue attributeValue, XacmlAttributeDesignator attributeDesignator) {
            Contract.Requires<ArgumentNullException>(matchId != null);
            Contract.Requires<ArgumentNullException>(attributeValue != null);
            Contract.Requires<ArgumentNullException>(attributeDesignator != null);

            this.matchId = matchId;
            this.attributeValue = attributeValue;
            this.attributeDesignator = attributeDesignator;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XacmlMatch"/> class.
        /// </summary>
        /// <param name="matchId">The match identifier.</param>
        /// <param name="attributeValue">The attribute value.</param>
        /// <param name="attributeSelector">The attribute selector.</param>
        public XacmlMatch(Uri matchId, XacmlAttributeValue attributeValue, XacmlAttributeSelector attributeSelector) {
            Contract.Requires<ArgumentNullException>(matchId != null);
            Contract.Requires<ArgumentNullException>(attributeValue != null);
            Contract.Requires<ArgumentNullException>(attributeSelector != null);

            this.matchId = matchId;
            this.attributeValue = attributeValue;
            this.attributeSelector = attributeSelector;
        }

        /// <summary>
        /// Gets or sets the AttributeValue
        /// </summary>
        public XacmlAttributeValue AttributeValue {
            get {
                return this.attributeValue;
            }

            set {
                Contract.Requires<ArgumentNullException>(value != null);
                this.attributeValue = value;
            }
        }

        /// <summary>
        /// Gets or sets the SubjectAttributeDesignator.
        /// </summary>
        public XacmlAttributeDesignator AttributeDesignator {
            get {
                return this.attributeDesignator;
            }

            set {
                Contract.Requires<ArgumentNullException>(value != null);
                this.attributeDesignator = value;
            }
        }

        /// <summary>
        /// Gets or sets the AttributeSelector.
        /// </summary>
        public XacmlAttributeSelector AttributeSelector {
            get {
                return this.attributeSelector;
            }

            set {
                Contract.Requires<ArgumentNullException>(value != null);
                this.attributeSelector = value;
            }
        }

        /// <summary>
        /// Gets or sets the MatchId
        /// </summary>
        public Uri MatchId {
            get {
                return this.matchId;
            }

            set {
                Contract.Requires<ArgumentNullException>(value != null);
                this.matchId = value;
            }
        }
    }
}