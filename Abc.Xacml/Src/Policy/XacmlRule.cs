// ----------------------------------------------------------------------------
// <copyright file="XacmlRule.cs" company="ABC Software Ltd">
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
    /// class XacmlRule
    /// </summary>
    public class XacmlRule {
        private readonly ICollection<XacmlObligationExpression> obligations = new Collection<XacmlObligationExpression>();
        private readonly ICollection<XacmlAdviceExpression> advices = new Collection<XacmlAdviceExpression>();
        private string ruleId;

        /// <summary>
        /// Initializes a new instance of the <see cref="XacmlRule"/> class.
        /// </summary>
        /// <param name="effect">The rule effect.</param>
        public XacmlRule(XacmlEffectType effect)
            : this(XacmlUtils.GenerateRuleId(), effect) {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XacmlRule"/> class.
        /// </summary>
        /// <param name="ruleId">The rule identifier.</param>
        /// <param name="effect">The rule effect.</param>
        public XacmlRule(string ruleId, XacmlEffectType effect) {
            if (ruleId == null) {
                throw new ArgumentNullException(nameof(ruleId));
            }

            this.ruleId = ruleId;
            this.Effect = effect;
        }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the target.
        /// </summary>
        public XacmlTarget Target { get; set; }

        /// <summary>
        /// Gets or sets the condition.
        /// </summary>
        public XacmlExpression Condition { get; set; }

        /// <summary>
        /// Gets or sets the rule identifier.
        /// </summary>
        /// <value>
        /// The rule identifier.
        /// </value>
        public string RuleId {
            get {
                return this.ruleId;
            }

            set {
                if (value == null) {
                    throw new ArgumentNullException(nameof(value));
                }

                this.ruleId = value;
            }
        }

        /// <summary>
        /// Gets or sets the effect.
        /// </summary>
        /// <value>
        /// The effect.
        /// </value>
        public XacmlEffectType Effect { get; set; }

        /// <summary>
        /// Gets the obligations.
        /// </summary>
        /// <value>
        /// The obligations.
        /// </value>
        /// <remarks>
        /// Used only for XACML 3.0
        /// </remarks>
        public ICollection<XacmlObligationExpression> Obligations {
            get {
                return this.obligations;
            }
        }

        /// <summary>
        /// Gets the advices.
        /// </summary>
        /// <value>
        /// The advices.
        /// </value>
        /// <remarks>
        /// Used only for XACML 3.0
        /// </remarks>
        public ICollection<XacmlAdviceExpression> Advices {
            get {
                return this.advices;
            }
        }
    }
}