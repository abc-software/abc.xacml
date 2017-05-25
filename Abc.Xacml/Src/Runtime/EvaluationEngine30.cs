// ----------------------------------------------------------------------------
// <copyright file="EvaluationEngine30.cs" company="ABC Software Ltd">
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
 
namespace Abc.Xacml.Runtime {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Xml;
    using Abc.Xacml.Context;
    using Abc.Xacml.Policy;
#if NET40
    using Diagnostic;
#else
    using Abc.Diagnostics;
#endif

    public class EvaluationEngine30 : EvaluationEngine {
        protected IDictionary<XacmlEffectType, List<XacmlAdvice>> advices;
        protected IDictionary<XacmlEffectType, List<XacmlContextPolicyIdReference>> applicablePolicies;
        protected IDictionary<XacmlEffectType, List<XacmlContextPolicySetIdReference>> applicablePolicySets;

        public EvaluationEngine30(XacmlPolicy policy)
            : base(policy) {
        }

        public EvaluationEngine30(XacmlPolicySet policySet)
            : base(policySet) {
        }

        public override XacmlContextResponse Evaluate(XacmlContextRequest request, XmlDocument requestDoc = null) {
            this.advices = new Dictionary<XacmlEffectType, List<XacmlAdvice>>()
            {
                { XacmlEffectType.Permit, new List<XacmlAdvice>() },
                { XacmlEffectType.Deny, new List<XacmlAdvice>() }
            };

            this.applicablePolicies = new Dictionary<XacmlEffectType, List<XacmlContextPolicyIdReference>>()
            {
                { XacmlEffectType.Permit, new List<XacmlContextPolicyIdReference>() },
                { XacmlEffectType.Deny, new List<XacmlContextPolicyIdReference>() }
            };

            this.applicablePolicySets = new Dictionary<XacmlEffectType, List<XacmlContextPolicySetIdReference>>()
            {
                { XacmlEffectType.Permit, new List<XacmlContextPolicySetIdReference>() },
                { XacmlEffectType.Deny, new List<XacmlContextPolicySetIdReference>() }
            };

            return base.Evaluate(request, requestDoc);
        }

        protected override IEnumerable<XacmlContextResult> RequestEvaluate(XacmlContextRequest request) {
            // MultiRequests element in a Request
            if (request.RequestReferences.Count > 0) {
                var results = new List<XacmlContextResult>(request.RequestReferences.Count);
                foreach (var reference in request.RequestReferences) {
                    var refAttributes = request.Attributes.Where(x => reference.AttributeReferences.Contains(x.Id));
                    if (refAttributes.Count() != reference.AttributeReferences.Count) {
                        throw new XacmlInvalidSyntaxException("<RequestReference> contains an invalid reference.");
                    }

                    var refRequest = new XacmlContextRequest(request.ReturnPolicyIdList, false, refAttributes) { XPathVersion = request.XPathVersion };
                    results.AddRange(this.RequestEvaluate(refRequest));
                }

                return results;
            }

            // multiple instances of an Attributes element with the same category ID
            var category = request.Attributes
                .GroupBy(o => o.Category.OriginalString)
                .Where(x => x.Count() > 1)
                .Select(o => o.Key).FirstOrDefault();
            if (category != null) {
                var results = new List<XacmlContextResult>();
                var otherAttributes = request.Attributes.Where(x => x.Category.OriginalString != category);
                foreach (XacmlContextAttributes categoryAttribute in request.Attributes.Where(o => o.Category.OriginalString == category)) {
                    var refAttributes = otherAttributes.Concat(new XacmlContextAttributes[] { categoryAttribute });
                    var refRequest = new XacmlContextRequest(request.ReturnPolicyIdList, false, refAttributes) { XPathVersion = request.XPathVersion };
                    results.AddRange(this.RequestEvaluate(refRequest));
                }

                return results;
            }

            return base.RequestEvaluate(request);
        }

        protected override XacmlContextResult MakeResult(XacmlDecisionResult decision, XacmlContextStatus status) {
            XacmlContextDecision resultDecision = XacmlContextDecision.NotApplicable;
            switch (decision) {
                case XacmlDecisionResult.Deny:
                    resultDecision = XacmlContextDecision.Deny;
                    break;
                case XacmlDecisionResult.Indeterminate:
                case XacmlDecisionResult.IndeterminateD:
                case XacmlDecisionResult.IndeterminateP:
                case XacmlDecisionResult.IndeterminateDP:
                    resultDecision = XacmlContextDecision.Indeterminate;
                    break;
                case XacmlDecisionResult.Permit:
                    resultDecision = XacmlContextDecision.Permit;
                    break;
            }

            //PROFILE - Multiple Decision Profile - #POL01 (Fists())
            var result = new XacmlContextResult(resultDecision) {
                Status = status,
            };

            foreach (var attribute in this.pip.GetAttributesWithIncludeInResult()) {
                result.Attributes.Add(attribute);
            };

            if (decision == XacmlDecisionResult.Permit) {
                foreach (var obligation in this.obligations[XacmlEffectType.Permit]) {
                    result.Obligations.Add(obligation);
                }

                foreach (var advice in this.advices[XacmlEffectType.Permit]) {
                    result.Advices.Add(advice);
                }

                if (pip.ReturnPolicyIdList()) {
                    foreach (var policyIdReferences in this.applicablePolicies[XacmlEffectType.Permit]) {
                        result.PolicyIdReferences.Add(policyIdReferences);
                    }

                    foreach (var policySetIdReferences in this.applicablePolicySets[XacmlEffectType.Permit]) {
                        result.PolicySetIdReferences.Add(policySetIdReferences);
                    }
                }
            }

            if (decision == XacmlDecisionResult.Deny) {
                foreach (var obligation in this.obligations[XacmlEffectType.Deny]) {
                    result.Obligations.Add(obligation);
                }

                foreach (var advice in this.advices[XacmlEffectType.Deny]) {
                    result.Advices.Add(advice);
                }

                if (pip.ReturnPolicyIdList()) {
                    foreach (var policyIdReferences in this.applicablePolicies[XacmlEffectType.Deny]) {
                        result.PolicyIdReferences.Add(policyIdReferences);
                    }

                    foreach (var policySetIdReferences in this.applicablePolicySets[XacmlEffectType.Deny]) {
                        result.PolicySetIdReferences.Add(policySetIdReferences);
                    }
                }
            }

            return result;
        }

        public override XacmlDecisionResult PolicySetEvaluate(XacmlPolicySet policySet) {
            ///// <Target>                <Policy>                                    <Policy Set>
            ///// "Match"                 Don’t care                                  Specified by the rulecombining algorithm
            ///// “No-match”              Don’t care                                  “NotApplicable”
            ///// “Indeterminate”         See Table 7                                 See Table 7
            XacmlDecisionResult algResult = XacmlDecisionResult.Indeterminate;
            // Wrap for Obligations and Advice, read more on AttributeAssignmentsWrapper class description
            using (AttributeAssignmentsWrapper<XacmlObligation> obligationWrapper = new AttributeAssignmentsWrapper<XacmlObligation>(() => this.obligations, o => this.obligations = o, () => algResult)) {
                using (AttributeAssignmentsWrapper<XacmlAdvice> adviceWrapper = new AttributeAssignmentsWrapper<XacmlAdvice>(() => this.advices, o => this.advices = o, () => algResult)) {
                    using (AttributeAssignmentsWrapper<XacmlContextPolicyIdReference> policyRefWrapper = new AttributeAssignmentsWrapper<XacmlContextPolicyIdReference>(() => this.applicablePolicies, o => this.applicablePolicies = o, () => algResult)) {
                        using (AttributeAssignmentsWrapper<XacmlContextPolicySetIdReference> policySetRefWrapper = new AttributeAssignmentsWrapper<XacmlContextPolicySetIdReference>(() => this.applicablePolicySets, o => this.applicablePolicySets = o, () => algResult)) {
                            // Target
                            XacmlMatchResult targetResult;
                            targetResult = this.TargetEvaluate(policySet.Target);

                            if (targetResult == XacmlMatchResult.NoMatch) {
                                return XacmlDecisionResult.NotApplicable;
                            }

                            // Policy
                            List<Tuple<IEnumerable<XacmlCombinerParameter>, IDictionary<string, Func<object>>>> policyResultsFunctions = new List<Tuple<IEnumerable<XacmlCombinerParameter>, IDictionary<string, Func<object>>>>();

                            bool atLeastOnePolicy = policySet.Policies.Any()
                                || policySet.PolicySets.Any()
                                || policySet.PolicyIdReferences_3_0.Any()
                                || policySet.PolicySetIdReferences_3_0.Any();

                            // Policies
                            foreach (XacmlPolicy pol in policySet.Policies) {
                                policyResultsFunctions.Add(new Tuple<IEnumerable<XacmlCombinerParameter>, IDictionary<string, Func<object>>>(
                                    policySet.PolicyCombinerParameters.Where(o => string.Equals(o.PolicyIdRef.OriginalString, pol.PolicyId.OriginalString)).SelectMany(o => o.CombinerParameters),
                                    new Dictionary<string, Func<object>>()
                            {
                                {
                                    "evaluate",
                                    () => new Tuple<XacmlDecisionResult, string>(this.PolicyEvaluate(pol), string.Empty)
                                },
                                {
                                    "isApplicable",
                                    () =>
                                    {
                                        XacmlMatchResult policyTargetResult = this.TargetEvaluate(pol.Target);
                                        if(policyTargetResult == XacmlMatchResult.Indeterminate)
                                        {
                                            return null;
                                        }

                                        if (policyTargetResult == XacmlMatchResult.Match)
                                        {
                                            return true;
                                        }

                                        return false;
                                    }
                                }
                            }));
                            }

                            // PolicySets
                            foreach (XacmlPolicySet pol in policySet.PolicySets) {
                                policyResultsFunctions.Add(new Tuple<IEnumerable<XacmlCombinerParameter>, IDictionary<string, Func<object>>>(
                                    policySet.PolicySetCombinerParameters.Where(o => string.Equals(o.PolicySetIdRef.OriginalString, pol.PolicySetId.OriginalString)).SelectMany(o => o.CombinerParameters),
                                    new Dictionary<string, Func<object>>()
                            {
                                {
                                    "evaluate",
                                    () => new Tuple<XacmlDecisionResult, string>(this.PolicySetEvaluate(pol), string.Empty)
                                },
                                {
                                    "isApplicable",
                                    () =>
                                    {
                                        XacmlMatchResult policyTargetResult = this.TargetEvaluate(pol.Target);
                                        if(policyTargetResult == XacmlMatchResult.Indeterminate)
                                        {
                                            return null;
                                        }

                                        if (policyTargetResult == XacmlMatchResult.Match)
                                        {
                                            return true;
                                        }

                                        return false;
                                    }
                                }
                            }));
                            }

                            // Policy References
                            foreach (XacmlContextPolicyIdReference polRef in policySet.PolicyIdReferences_3_0) {
                                Uri uri;
                                if (Uri.TryCreate(polRef.Value, UriKind.RelativeOrAbsolute, out uri)) {
                                    XacmlPolicy pol = this.ch.RequestPolicy(uri);
                                    if (pol == null) {
                                        throw new XacmlIndeterminateException("Unknown Policy reference: " + polRef.ToString());
                                    }

                                    policyResultsFunctions.Add(new Tuple<IEnumerable<XacmlCombinerParameter>, IDictionary<string, Func<object>>>(
                                    policySet.PolicyCombinerParameters.Where(o => string.Equals(o.PolicyIdRef.OriginalString, pol.PolicyId.OriginalString)).SelectMany(o => o.CombinerParameters),
                                    new Dictionary<string, Func<object>>()
                            {
                                {
                                    "evaluate",
                                    () => new Tuple<XacmlDecisionResult, string>(this.PolicyEvaluate(pol), string.Empty)
                                },
                                {
                                    "isApplicable",
                                    () =>
                                    {
                                        XacmlMatchResult policyTargetResult = this.TargetEvaluate(pol.Target);
                                        if(policyTargetResult == XacmlMatchResult.Indeterminate)
                                        {
                                            return null;
                                        }

                                        if (policyTargetResult == XacmlMatchResult.Match)
                                        {
                                            return true;
                                        }

                                        return false;
                                    }
                                }
                            }));
                                }
                                else {
                                    throw DiagnosticTools.ExceptionUtil.ThrowHelperFatal("Policy reference not URI - Not implemented", new NotImplementedException("Policy reference not URI - Not implemented"));
                                }
                            }

                            // PolicySet References
                            foreach (XacmlContextPolicySetIdReference polRef in policySet.PolicySetIdReferences_3_0) {
                                Uri uri;
                                if (Uri.TryCreate(polRef.Value, UriKind.RelativeOrAbsolute, out uri)) {
                                    XacmlPolicySet pol = this.ch.RequestPolicySet(uri);
                                    if (pol == null) {
                                        throw DiagnosticTools.ExceptionUtil.ThrowHelperError(new XacmlIndeterminateException("Unknown PolicySet reference: " + polRef.ToString()));
                                    }

                                    policyResultsFunctions.Add(new Tuple<IEnumerable<XacmlCombinerParameter>, IDictionary<string, Func<object>>>(
                                    policySet.PolicySetCombinerParameters.Where(o => string.Equals(o.PolicySetIdRef.OriginalString, pol.PolicySetId.OriginalString)).SelectMany(o => o.CombinerParameters),
                                    new Dictionary<string, Func<object>>()
                            {
                                {
                                    "evaluate",
                                    () => new Tuple<XacmlDecisionResult, string>(this.PolicySetEvaluate(pol), string.Empty)
                                },
                                {
                                    "isApplicable",
                                    () =>
                                    {
                                        XacmlMatchResult policyTargetResult = this.TargetEvaluate(pol.Target);
                                        if(policyTargetResult == XacmlMatchResult.Indeterminate)
                                        {
                                            return null;
                                        }

                                        if (policyTargetResult == XacmlMatchResult.Match)
                                        {
                                            return true;
                                        }

                                        return false;
                                    }
                                }
                            }));
                                }
                                else {
                                    throw DiagnosticTools.ExceptionUtil.ThrowHelperFatal("PolicySet reference not URI - Not implemented", new NotImplementedException("PolicySet reference not URI - Not implemented"));
                                }
                            }

                            algResult = this.algorithms[policySet.PolicyCombiningAlgId.ToString()].Invoke(policyResultsFunctions,
                                policySet.CombinerParameters);

                            if (targetResult == XacmlMatchResult.Indeterminate) {
                                algResult = this.SpecifyCombiningAlgorithmResult(algResult);
                            }

                            // Permit
                            if (algResult == XacmlDecisionResult.Permit) {
                                if (policySet.Obligations_3_0.Count > 0) {
                                    IEnumerable<XacmlObligationExpression> obligationsWithPermit = policySet.Obligations_3_0.Where(o => o.FulfillOn == XacmlEffectType.Permit);
                                    foreach (XacmlObligationExpression expression in obligationsWithPermit) {
                                        List<XacmlAttributeAssignment> attributeAssigments = new List<XacmlAttributeAssignment>();
                                        foreach (XacmlAttributeAssignmentExpression ex in expression.AttributeAssignmentExpressions) {
                                            IEnumerable<XacmlAttributeAssignment> assignment = this.AttributeAssignmentExpressionEvaluate(ex);
                                            // ja Indeterminate, tad rezultāts arī
                                            if (assignment == null) {
                                                return XacmlDecisionResult.Indeterminate;
                                            }

                                            attributeAssigments.AddRange(assignment);
                                        }

                                        XacmlObligation obligation = new XacmlObligation(expression.ObligationId, attributeAssigments);
                                        this.obligations[expression.FulfillOn].Add(obligation);
                                    }
                                }

                                if (policySet.Advices.Count > 0) {
                                    IEnumerable<XacmlAdviceExpression> advicesWithPermit = policySet.Advices.Where(o => o.AppliesTo == XacmlEffectType.Permit);
                                    foreach (XacmlAdviceExpression expression in advicesWithPermit) {
                                        List<XacmlAttributeAssignment> attributeAssigments = new List<XacmlAttributeAssignment>();
                                        foreach (XacmlAttributeAssignmentExpression ex in expression.AttributeAssignmentExpressions) {
                                            IEnumerable<XacmlAttributeAssignment> assignment = this.AttributeAssignmentExpressionEvaluate(ex);
                                            // ja Indeterminate, tad rezultāts arī
                                            if (assignment == null) {
                                                return XacmlDecisionResult.Indeterminate;
                                            }

                                            attributeAssigments.AddRange(assignment);
                                        }

                                        XacmlAdvice advice = new XacmlAdvice(expression.AdviceId, attributeAssigments);
                                        this.advices[expression.AppliesTo].Add(advice);
                                    }
                                }

                                this.applicablePolicySets[XacmlEffectType.Permit].Add(new XacmlContextPolicySetIdReference() {
                                    Value = policySet.PolicySetId.OriginalString,
                                    Version = new XacmlVersionMatchType(policySet.Version)
                                });
                            }

                            // Deny
                            if (algResult == XacmlDecisionResult.Deny) {
                                if (policySet.Obligations_3_0.Count > 0) {
                                    IEnumerable<XacmlObligationExpression> obligationsWithPermit = policySet.Obligations_3_0.Where(o => o.FulfillOn == XacmlEffectType.Deny);
                                    foreach (XacmlObligationExpression expression in obligationsWithPermit) {
                                        List<XacmlAttributeAssignment> attributeAssigments = new List<XacmlAttributeAssignment>();
                                        foreach (XacmlAttributeAssignmentExpression ex in expression.AttributeAssignmentExpressions) {
                                            IEnumerable<XacmlAttributeAssignment> assignment = this.AttributeAssignmentExpressionEvaluate(ex);
                                            // ja Indeterminate, tad rezultāts arī
                                            if (assignment == null) {
                                                return XacmlDecisionResult.Indeterminate;
                                            }

                                            attributeAssigments.AddRange(assignment);
                                        }

                                        XacmlObligation obligation = new XacmlObligation(expression.ObligationId, attributeAssigments);
                                        this.obligations[expression.FulfillOn].Add(obligation);
                                    }
                                }

                                if (policySet.Advices.Count > 0) {
                                    IEnumerable<XacmlAdviceExpression> advicesWithPermit = policySet.Advices.Where(o => o.AppliesTo == XacmlEffectType.Deny);
                                    foreach (XacmlAdviceExpression expression in advicesWithPermit) {
                                        List<XacmlAttributeAssignment> attributeAssigments = new List<XacmlAttributeAssignment>();
                                        foreach (XacmlAttributeAssignmentExpression ex in expression.AttributeAssignmentExpressions) {
                                            IEnumerable<XacmlAttributeAssignment> assignment = this.AttributeAssignmentExpressionEvaluate(ex);
                                            // ja Indeterminate, tad rezultāts arī
                                            if (assignment == null) {
                                                return XacmlDecisionResult.Indeterminate;
                                            }

                                            attributeAssigments.AddRange(assignment);
                                        }

                                        XacmlAdvice advice = new XacmlAdvice(expression.AdviceId, attributeAssigments);
                                        this.advices[expression.AppliesTo].Add(advice);
                                    }
                                }

                                this.applicablePolicySets[XacmlEffectType.Deny].Add(new XacmlContextPolicySetIdReference() {
                                    Value = policySet.PolicySetId.OriginalString,
                                    Version = new XacmlVersionMatchType(policySet.Version)
                                });
                            }
                        }
                    }
                }
            }

            return algResult;
        }

        public override XacmlDecisionResult PolicyEvaluate(XacmlPolicy policy) {
            XacmlPolicy previousPolicy = this.currentEvaluatingPolicy;
            this.currentEvaluatingPolicy = policy;

            ///// <Target>                <Rule>                                      <Policy>
            ///// "Match"                 Don’t care                                  Specified by the rulecombining algorithm
            ///// “No-match”              Don’t care                                  “NotApplicable”
            ///// “Indeterminate”         See Table 7                                 See Table 7
            XacmlDecisionResult algResult = XacmlDecisionResult.Indeterminate;
            // Wrap for Obligations and Advice, read more on AttributeAssignmentsWrapper class description
            using (AttributeAssignmentsWrapper<XacmlObligation> obligationWrapper = new AttributeAssignmentsWrapper<XacmlObligation>(() => this.obligations, o => this.obligations = o, () => algResult)) {
                using (AttributeAssignmentsWrapper<XacmlAdvice> adviceWrapper = new AttributeAssignmentsWrapper<XacmlAdvice>(() => this.advices, o => this.advices = o, () => algResult)) {
                    // Target
                    XacmlMatchResult targetResult;
                    targetResult = this.TargetEvaluate(policy.Target);

                    if (targetResult == XacmlMatchResult.NoMatch) {
                        return XacmlDecisionResult.NotApplicable;
                    }

                    // Rules
                    List<Tuple<IEnumerable<XacmlCombinerParameter>, IDictionary<string, Func<object>>>> ruleResultsFunctions = new List<Tuple<IEnumerable<XacmlCombinerParameter>, IDictionary<string, Func<object>>>>();

                    foreach (XacmlRule rule in policy.Rules) {
                        ruleResultsFunctions.Add(new Tuple<IEnumerable<XacmlCombinerParameter>, IDictionary<string, Func<object>>>(
                            policy.RuleCombinerParameters.Where(o => string.Equals(o.RuleIdRef, rule.RuleId)).SelectMany(o => o.CombinerParameters),
                            new Dictionary<string, Func<object>>()
                            {
                                {
                                    "evaluate",
                                    () => this.RuleEvaluate(rule)
                                }
                            }));
                    }

                    algResult = this.algorithms[policy.RuleCombiningAlgId.ToString()].Invoke(ruleResultsFunctions,
                        policy.CombinerParameters.Concat(policy.ChoiceCombinerParameters));

                    this.currentEvaluatingPolicy = previousPolicy;

                    if (targetResult == XacmlMatchResult.Indeterminate) {
                        algResult = this.SpecifyCombiningAlgorithmResult(algResult);
                    }

                    // Permit
                    if (algResult == XacmlDecisionResult.Permit) {
                        if (policy.ObligationExpressions.Count > 0) {
                            IEnumerable<XacmlObligationExpression> obligationsWithPermit = policy.ObligationExpressions.Where(o => o.FulfillOn == XacmlEffectType.Permit);
                            foreach (XacmlObligationExpression expression in obligationsWithPermit) {
                                List<XacmlAttributeAssignment> attributeAssigments = new List<XacmlAttributeAssignment>();
                                foreach (XacmlAttributeAssignmentExpression ex in expression.AttributeAssignmentExpressions) {
                                    IEnumerable<XacmlAttributeAssignment> assignment = this.AttributeAssignmentExpressionEvaluate(ex);
                                    // ja Indeterminate, tad rezultāts arī
                                    if (assignment == null) {
                                        return XacmlDecisionResult.Indeterminate;
                                    }

                                    attributeAssigments.AddRange(assignment);
                                }

                                XacmlObligation obligation = new XacmlObligation(expression.ObligationId, attributeAssigments);
                                this.obligations[expression.FulfillOn].Add(obligation);
                            }
                        }

                        if (policy.AdviceExpressions.Count > 0) {
                            IEnumerable<XacmlAdviceExpression> advicesWithPermit = policy.AdviceExpressions.Where(o => o.AppliesTo == XacmlEffectType.Permit);
                            foreach (XacmlAdviceExpression expression in advicesWithPermit) {
                                List<XacmlAttributeAssignment> attributeAssigments = new List<XacmlAttributeAssignment>();
                                foreach (XacmlAttributeAssignmentExpression ex in expression.AttributeAssignmentExpressions) {
                                    IEnumerable<XacmlAttributeAssignment> assignment = this.AttributeAssignmentExpressionEvaluate(ex);
                                    // ja Indeterminate, tad rezultāts arī
                                    if (assignment == null) {
                                        return XacmlDecisionResult.Indeterminate;
                                    }

                                    attributeAssigments.AddRange(assignment);
                                }

                                XacmlAdvice advice = new XacmlAdvice(expression.AdviceId, attributeAssigments);
                                this.advices[expression.AppliesTo].Add(advice);
                            }
                        }
                    }

                    // Deny
                    if (algResult == XacmlDecisionResult.Deny) {
                        if (policy.ObligationExpressions.Count > 0) {
                            IEnumerable<XacmlObligationExpression> obligationsWithPermit = policy.ObligationExpressions.Where(o => o.FulfillOn == XacmlEffectType.Deny);
                            foreach (XacmlObligationExpression expression in obligationsWithPermit) {
                                List<XacmlAttributeAssignment> attributeAssigments = new List<XacmlAttributeAssignment>();
                                foreach (XacmlAttributeAssignmentExpression ex in expression.AttributeAssignmentExpressions) {
                                    IEnumerable<XacmlAttributeAssignment> assignment = this.AttributeAssignmentExpressionEvaluate(ex);
                                    // ja Indeterminate, tad rezultāts arī
                                    if (assignment == null) {
                                        return XacmlDecisionResult.Indeterminate;
                                    }

                                    attributeAssigments.AddRange(assignment);
                                }

                                XacmlObligation obligation = new XacmlObligation(expression.ObligationId, attributeAssigments);
                                this.obligations[expression.FulfillOn].Add(obligation);
                            }
                        }

                        if (policy.AdviceExpressions.Count > 0) {
                            IEnumerable<XacmlAdviceExpression> advicesWithPermit = policy.AdviceExpressions.Where(o => o.AppliesTo == XacmlEffectType.Deny);
                            foreach (XacmlAdviceExpression expression in advicesWithPermit) {
                                List<XacmlAttributeAssignment> attributeAssigments = new List<XacmlAttributeAssignment>();
                                foreach (XacmlAttributeAssignmentExpression ex in expression.AttributeAssignmentExpressions) {
                                    IEnumerable<XacmlAttributeAssignment> assignment = this.AttributeAssignmentExpressionEvaluate(ex);
                                    // ja Indeterminate, tad rezultāts arī
                                    if (assignment == null) {
                                        return XacmlDecisionResult.Indeterminate;
                                    }

                                    attributeAssigments.AddRange(assignment);
                                }

                                XacmlAdvice advice = new XacmlAdvice(expression.AdviceId, attributeAssigments);
                                this.advices[expression.AppliesTo].Add(advice);
                            }
                        }
                    }
                }
            }

            // add applicaple policies for PolicyIdentifierList in response
            if (algResult == XacmlDecisionResult.Permit) {
                this.applicablePolicies[XacmlEffectType.Permit].Add(new XacmlContextPolicyIdReference() {
                    Value = policy.PolicyId.OriginalString,
                    Version = new XacmlVersionMatchType(policy.Version)
                });
            }
            if (algResult == XacmlDecisionResult.Deny) {
                this.applicablePolicies[XacmlEffectType.Deny].Add(new XacmlContextPolicyIdReference() {
                    Value = policy.PolicyId.OriginalString,
                    Version = new XacmlVersionMatchType(policy.Version)
                });
            }

            return algResult;
        }

        protected override Tuple<XacmlDecisionResult, string> RuleEvaluate(XacmlRule rule) {
            ///// <Target>                        <Condition>         <Rule>
            ///// "Match" or no target            "True"              Effect
            ///// "Match" or no target            "False"             "NotApplicable"
            ///// "Match" or no target            "Indeterminate"     “Indeterminate{P}” if the Effect is Permit, or “Indeterminate{D}” if the Effect is Deny
            ///// "NoMatch"                        Don`t care          "NotApplicable"
            ///// "Indeternimate"                  Don`t care          “Indeterminate{P}” if the Effect is Permit, or “Indeterminate{D}” if the Effect is Deny
            XacmlDecisionResult ruleResult = XacmlDecisionResult.Indeterminate;

            // Wrap for Obligations and Advice, read more on AttributeAssignmentsWrapper class description
            using (AttributeAssignmentsWrapper<XacmlObligation> obligationWrapper = new AttributeAssignmentsWrapper<XacmlObligation>(() => this.obligations, o => this.obligations = o, () => ruleResult)) {
                using (AttributeAssignmentsWrapper<XacmlAdvice> adviceWrapper = new AttributeAssignmentsWrapper<XacmlAdvice>(() => this.advices, o => this.advices = o, () => ruleResult)) {
                    // Target
                    // If this element is omitted, then the target for the <Rule> SHALL be defined by the <Target> element of the enclosing <Policy> element.
                    XacmlTarget target = rule.Target ?? this.currentEvaluatingPolicy.Target;
                    XacmlMatchResult targetResult = this.TargetEvaluate(target);

                    if (targetResult == XacmlMatchResult.NoMatch) {
                        return new Tuple<XacmlDecisionResult, string>(XacmlDecisionResult.NotApplicable, Enum.GetName(typeof(XacmlEffectType), rule.Effect));
                    }

                    if (targetResult == XacmlMatchResult.Indeterminate) {
                        return new Tuple<XacmlDecisionResult, string>(rule.Effect == XacmlEffectType.Permit ? XacmlDecisionResult.IndeterminateP : XacmlDecisionResult.IndeterminateD, Enum.GetName(typeof(XacmlEffectType), rule.Effect));
                    }

                    // Condition
                    // if XACML v 1.0
                    // The condition value SHALL be "True" if the <Condition> element is absent
                    bool? conditionResult = true;
                    if (rule.Condition != null) {
                        conditionResult = this.ConditionEvaluate(rule.Condition);
                    }

                    // Target is Match
                    if (!conditionResult.HasValue) {
                        ruleResult = rule.Effect == XacmlEffectType.Permit ? XacmlDecisionResult.IndeterminateP : XacmlDecisionResult.IndeterminateD;
                    }
                    else if (conditionResult.Value) {
                        ruleResult = (XacmlDecisionResult)(Enum.Parse(typeof(XacmlDecisionResult), Enum.GetName(typeof(XacmlEffectType), rule.Effect)));
                    }
                    else {
                        ruleResult = XacmlDecisionResult.NotApplicable;
                    }

                    // Permit
                    if (ruleResult == XacmlDecisionResult.Permit) {
                        if (rule.Obligations.Count > 0) {
                            IEnumerable<XacmlObligationExpression> obligationsWithPermit = rule.Obligations.Where(o => o.FulfillOn == XacmlEffectType.Permit);
                            foreach (XacmlObligationExpression expression in obligationsWithPermit) {
                                List<XacmlAttributeAssignment> attributeAssigments = new List<XacmlAttributeAssignment>();
                                foreach (XacmlAttributeAssignmentExpression ex in expression.AttributeAssignmentExpressions) {
                                    IEnumerable<XacmlAttributeAssignment> assignment = this.AttributeAssignmentExpressionEvaluate(ex);
                                    // ja Indeterminate, tad rezultāts arī
                                    if (assignment == null) {
                                        return new Tuple<XacmlDecisionResult, string>(XacmlDecisionResult.Indeterminate, Enum.GetName(typeof(XacmlEffectType), rule.Effect));
                                    }

                                    attributeAssigments.AddRange(assignment);
                                }

                                XacmlObligation obligation = new XacmlObligation(expression.ObligationId, attributeAssigments);
                                this.obligations[expression.FulfillOn].Add(obligation);
                            }
                        }

                        if (rule.Advices.Count > 0) {
                            IEnumerable<XacmlAdviceExpression> advicesWithPermit = rule.Advices.Where(o => o.AppliesTo == XacmlEffectType.Permit);
                            foreach (XacmlAdviceExpression expression in advicesWithPermit) {
                                List<XacmlAttributeAssignment> attributeAssigments = new List<XacmlAttributeAssignment>();
                                foreach (XacmlAttributeAssignmentExpression ex in expression.AttributeAssignmentExpressions) {
                                    IEnumerable<XacmlAttributeAssignment> assignment = this.AttributeAssignmentExpressionEvaluate(ex);
                                    // ja Indeterminate, tad rezultāts arī
                                    if (assignment == null) {
                                        return new Tuple<XacmlDecisionResult, string>(XacmlDecisionResult.Indeterminate, Enum.GetName(typeof(XacmlEffectType), rule.Effect));
                                    }

                                    attributeAssigments.AddRange(assignment);
                                }

                                XacmlAdvice advice = new XacmlAdvice(expression.AdviceId, attributeAssigments);
                                this.advices[expression.AppliesTo].Add(advice);
                            }
                        }
                    }

                    // Deny
                    if (ruleResult == XacmlDecisionResult.Deny) {
                        if (rule.Obligations.Count > 0) {
                            IEnumerable<XacmlObligationExpression> obligationsWithPermit = rule.Obligations.Where(o => o.FulfillOn == XacmlEffectType.Deny);
                            foreach (XacmlObligationExpression expression in obligationsWithPermit) {
                                List<XacmlAttributeAssignment> attributeAssigments = new List<XacmlAttributeAssignment>();
                                foreach (XacmlAttributeAssignmentExpression ex in expression.AttributeAssignmentExpressions) {
                                    IEnumerable<XacmlAttributeAssignment> assignment = this.AttributeAssignmentExpressionEvaluate(ex);
                                    // ja Indeterminate, tad rezultāts arī
                                    if (assignment == null) {
                                        return new Tuple<XacmlDecisionResult, string>(XacmlDecisionResult.Indeterminate, Enum.GetName(typeof(XacmlEffectType), rule.Effect));
                                    }

                                    attributeAssigments.AddRange(assignment);
                                }

                                XacmlObligation obligation = new XacmlObligation(expression.ObligationId, attributeAssigments);
                                this.obligations[expression.FulfillOn].Add(obligation);
                            }
                        }

                        if (rule.Advices.Count > 0) {
                            IEnumerable<XacmlAdviceExpression> advicesWithPermit = rule.Advices.Where(o => o.AppliesTo == XacmlEffectType.Deny);
                            foreach (XacmlAdviceExpression expression in advicesWithPermit) {
                                List<XacmlAttributeAssignment> attributeAssigments = new List<XacmlAttributeAssignment>();
                                foreach (XacmlAttributeAssignmentExpression ex in expression.AttributeAssignmentExpressions) {
                                    IEnumerable<XacmlAttributeAssignment> assignment = this.AttributeAssignmentExpressionEvaluate(ex);
                                    // ja Indeterminate, tad rezultāts arī
                                    if (assignment == null) {
                                        return new Tuple<XacmlDecisionResult, string>(XacmlDecisionResult.Indeterminate, Enum.GetName(typeof(XacmlEffectType), rule.Effect));
                                    }

                                    attributeAssigments.AddRange(assignment);
                                }

                                XacmlAdvice advice = new XacmlAdvice(expression.AdviceId, attributeAssigments);
                                this.advices[expression.AppliesTo].Add(advice);
                            }
                        }
                    }
                }
            }

            return new Tuple<XacmlDecisionResult, string>(ruleResult, Enum.GetName(typeof(XacmlEffectType), rule.Effect));
        }

        /// <summary>
        /// XacmlAttributeAssignmentExpression ----> XacmlAttributeAssignment
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        protected virtual IEnumerable<XacmlAttributeAssignment> AttributeAssignmentExpressionEvaluate(XacmlAttributeAssignmentExpression expression) {
            Contract.Requires<ArgumentNullException>(expression != null);

            object expressionResult = this.ExpressionEvaluate(expression.Property);

            if (expressionResult == null) {
                return null;
            }

            List<XacmlAttributeAssignment> result = new List<XacmlAttributeAssignment>();

            if ((typeof(IEnumerable)).IsAssignableFrom(expressionResult.GetType()) && !(expressionResult is string)) {
                foreach (object elem in (IEnumerable)expressionResult) {
                    result.Add(this.AttributeAssignmentCreate(elem, expression));
                }
            }
            else {
                result.Add(this.AttributeAssignmentCreate(expressionResult, expression));
            }

            return result;
        }

        protected virtual XacmlAttributeAssignment AttributeAssignmentCreate(object value, XacmlAttributeAssignmentExpression expression) {
            Contract.Requires<ArgumentNullException>(value != null);
            Contract.Requires<ArgumentNullException>(expression != null);

            TypeConverter con = TypeDescriptor.GetConverter(value.GetType());
            string type = this.types[con];

            Uri typeUri;
            Uri.TryCreate(this.types[con], UriKind.RelativeOrAbsolute, out typeUri);

            XacmlAttributeAssignment result = new XacmlAttributeAssignment(expression.AttributeId, typeUri, value.ToString()) {
                Category = expression.Category,
                Issuer = expression.Issuer
            };

            return result;
        }

        protected override object ExpressionEvaluate(IXacmlApply expression) {
            object result = null;

            Type applyElemType = expression.GetType();
            if (applyElemType == typeof(XacmlVariableReference)) {
                XacmlVariableReference reference = expression as XacmlVariableReference;
                XacmlVariableDefinition definition = this.currentEvaluatingPolicy.VariableDefinitions.SingleOrDefault(o => o.VariableId == reference.VariableReference);
                if (definition == null) {
                    throw DiagnosticTools.ExceptionUtil.ThrowHelperError(new XacmlInvalidSyntaxException("Missing Variable definition " + reference.VariableReference));
                }

                // Cache value
                if (definition.CalculatedValue != null) {
                    result = definition.CalculatedValue;
                }
                else {
                    result = this.ExpressionEvaluate(definition.Property);
                    definition.CalculatedValue = result;
                }
            }
            else if (applyElemType == typeof(XacmlAttributeSelector)) {
                result = this.GetAttributeSelector(expression as XacmlAttributeSelector);
            }
            else if (applyElemType == typeof(XacmlAttributeDesignator)) {
                var design = expression as XacmlAttributeDesignator;
                IEnumerable<string> designatorsNames = this.GetAttributeDesignator(design);

                if (designatorsNames == null) {
                    throw new XacmlIndeterminateException("XacmlAttributeDesignator indeterminate");
                }

                TypeConverterWrapper typeConverter = this.types[design.DataType.ToString()];
                result = typeConverter.ConvertEnumerable(designatorsNames.Select(o => typeConverter.ConvertFromString(o, design)));
            }
            else if (applyElemType == typeof(XacmlAttributeValue)) {
                XacmlAttributeValue attributeValue = expression as XacmlAttributeValue;
                result = this.types[attributeValue.DataType.ToString()].ConvertFromString(attributeValue.Value, attributeValue);
            }
            else if (applyElemType == typeof(XacmlFunction)) {
                XacmlFunction functionDelegate = expression as XacmlFunction;
                result = this.functions[functionDelegate.FunctionId.ToString()];
            }
            else if (applyElemType == typeof(XacmlApply)) {
                result = this.ApplyEvaluate(expression as XacmlApply);
            }

            return result;
        }

        protected override XacmlMatchResult TargetEvaluate(XacmlTarget target) {
            if (target.AnyOf.Count == 0) {
                return XacmlMatchResult.Match;
            }

            //// <AnyOf> values                   Target value
            //// All “Match”                      “Match”
            //// At least one “No Match”          "No Match"
            //// Otherwise                        “Indeterminate”
            XacmlMatchResult result = XacmlMatchResult.Match;
            foreach (XacmlAnyOf anyOf in target.AnyOf) {
                //// <AllOf> values                                   <AnyOf> Value
                //// At least one “Match”                             “Match”
                //// None matches and at least one “Indeterminate”    “Indeterminate”
                //// All “No match”                                   “No match”
                XacmlMatchResult anyOfResult = XacmlMatchResult.NoMatch;
                foreach (XacmlAllOf allOf in anyOf.AllOf) {
                    //// <Match> values                               <AllOf> Value
                    //// All “True”                                   “Match”
                    //// No “False” and at least one “Indeterminate”  “Indeterminate”
                    //// At least one “False”                         “No match”
                    XacmlMatchResult allOfResult = XacmlMatchResult.Match;
                    foreach (XacmlMatch match in allOf.Matches) {
                        bool? matchResult = this.MatchEvaluation(match);

                        // indeterminate
                        if (!matchResult.HasValue) {
                            allOfResult = XacmlMatchResult.Indeterminate;
                        }
                        else if (!matchResult.Value) {
                            allOfResult = XacmlMatchResult.NoMatch;
                            break;
                        }
                    }

                    if (allOfResult == XacmlMatchResult.Match) {
                        anyOfResult = XacmlMatchResult.Match;
                        break;
                    }

                    if (allOfResult == XacmlMatchResult.Indeterminate) {
                        anyOfResult = XacmlMatchResult.Indeterminate;
                    }
                }

                if (anyOfResult == XacmlMatchResult.NoMatch) {
                    result = XacmlMatchResult.NoMatch;
                    break;
                }

                if (anyOfResult == XacmlMatchResult.Indeterminate) {
                    result = XacmlMatchResult.Indeterminate;
                }
            }

            return result;
        }

        protected bool? MatchEvaluation(XacmlMatch match) {
            Contract.Requires<ArgumentNullException>(match != null);

            // get function
            DelegateWrapper matchFunction = this.functions[match.MatchId.ToString()];

            // get attribute value
            object attribute1 = this.types[match.AttributeValue.DataType.ToString()].ConvertFromString(match.AttributeValue.Value, match.AttributeValue);

            IEnumerable<string> attribute2Values;
            string dataType;
            object source;

            // get second attribute
            if (match.AttributeDesignator != null) {
                dataType = match.AttributeDesignator.DataType.ToString();
                attribute2Values = this.GetAttributeDesignator(match.AttributeDesignator as XacmlAttributeDesignator);
                source = match.AttributeDesignator;
            }
            else {
                dataType = match.AttributeSelector.DataType.ToString();
                attribute2Values = this.GetAttributeSelector(match.AttributeSelector);
                source = match.AttributeSelector;
            }

            if (attribute2Values == null) {
                // If an operational error were to occur while evaluating the <AttributeDesignator> or <AttributeSelector> element, then the result of the entire expression SHALL be "Indeterminate".
                return null;
            }
            else {
                // If the <AttributeDesignator> or <AttributeSelector> element were to evaluate to an empty bag, then the result of the expression SHALL be "False".
                if (!attribute2Values.Any()) {
                    return false;
                }
            }

            bool? matchResult = false;
            foreach (string designatorValue in attribute2Values) {
                object attribute2 = this.types[dataType].ConvertFromString(designatorValue, source);

                // evaluate
                bool? functionResult = (bool)matchFunction.DynamicInvoke(new XPathContext(this.xpathVersion, this.requestDoc, this.namespaces), attribute1, attribute2);

                // Otherwise, if at least one of the function applications results in "Indeterminate", then the result SHALL be "Indeterminate".
                if (!functionResult.HasValue) {
                    matchResult = null;
                }
                // If at least one of those function applications were to evaluate to "True", then the result of the entire expression SHALL be "True".
                else if (functionResult.Value) {
                    return true;
                }
            }

            return matchResult;
        }

        protected override IEnumerable<string> GetAttributeSelector(XacmlAttributeSelector selector) {
            IEnumerable<XmlNode> attributeBag = this.pip.GetAttributeByXPath(this.xpathVersion, selector.Path, selector.Category, selector.ContextSelectorId, this.namespaces);

            if (!attributeBag.Any()) {
                if (selector.MustBePresent.HasValue && selector.MustBePresent.Value) {
                    // return "Indeterminate”
                    return null;
                }
            }

            return attributeBag.Select(o => o.Value);
        }

        protected IEnumerable<string> GetAttributeDesignator(XacmlAttributeDesignator designator) {
            Contract.Requires<ArgumentNullException>(designator != null);

            IEnumerable<string> attributeBag = this.pip.GetAttributeDesignatorValues(
                    designator.AttributeId,
                    designator.DataType,
                    designator.Issuer,
                    designator.Category
                    );

            if (!attributeBag.Any()) {
                if (designator.MustBePresent.HasValue && designator.MustBePresent.Value) {
                    // return "Indeterminate”
                    return null;
                }
            }

            return attributeBag;
        }

        protected XacmlDecisionResult SpecifyCombiningAlgorithmResult(XacmlDecisionResult combAlgRes) {
            switch (combAlgRes) {
                case XacmlDecisionResult.NotApplicable:
                    return XacmlDecisionResult.NotApplicable;

                case XacmlDecisionResult.Permit:
                    return XacmlDecisionResult.IndeterminateP;

                case XacmlDecisionResult.Deny:
                    return XacmlDecisionResult.IndeterminateD;

                case XacmlDecisionResult.Indeterminate:
                    return XacmlDecisionResult.IndeterminateDP;

                case XacmlDecisionResult.IndeterminateDP:
                    return XacmlDecisionResult.IndeterminateDP;

                case XacmlDecisionResult.IndeterminateP:
                    return XacmlDecisionResult.IndeterminateP;

                case XacmlDecisionResult.IndeterminateD:
                    return XacmlDecisionResult.IndeterminateD;

                default:
                    return XacmlDecisionResult.IndeterminateDP;
            }
        }
    }
}