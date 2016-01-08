// ----------------------------------------------------------------------------
// <copyright file="XacmlObligationExpression.cs" company="ABC Software Ltd">
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
    using System.Diagnostics.Contracts;

    public class XacmlObligationExpression {
        private Uri obligationId;
        private XacmlEffectType fulfillOn;
        private readonly ICollection<XacmlAttributeAssignmentExpression> attributeAssignmentExpressions = new Collection<XacmlAttributeAssignmentExpression>();

        public XacmlObligationExpression(Uri obligationId, XacmlEffectType effectType) {
            Contract.Requires<ArgumentNullException>(obligationId != null);

            this.obligationId = obligationId;
            this.fulfillOn = effectType;
        }

        public Uri ObligationId {
            get {
                return this.obligationId;
            }

            set {
                Contract.Requires<ArgumentNullException>(value != null);
                this.obligationId = value;
            }
        }

        public XacmlEffectType FulfillOn {
            get {
                return this.fulfillOn;
            }

            set {
                this.fulfillOn = value;
            }
        }

        public ICollection<XacmlAttributeAssignmentExpression> AttributeAssignmentExpressions {
            get {
                return this.attributeAssignmentExpressions;
            }
        }

    }
}
