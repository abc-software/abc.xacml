// ----------------------------------------------------------------------------
// <copyright file="EvaluationEngine.cs" company="ABC Software Ltd">
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
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Xml;
    using Abc.Xacml.Context;
    using Abc.Xacml.Policy;

    public class EvaluationEngine {
        public IXacmlPolicyRepository ch;

        protected DataTypeProcessor types;
        protected FunctionsProcessor functions;
        protected AlgorithmsProcessor algorithms;
        protected PolicyInformationPoint pip;

        protected Uri xpathVersion;
        protected IDictionary<string, string> namespaces;
        protected XmlDocument requestDoc;

        protected XacmlPolicySet policySet = null;
        protected XacmlPolicy policy = null;

        protected IDictionary<XacmlEffectType, List<XacmlObligation>> obligations = null;

        /// <summary>
        /// Policy, kuras elementi tiek apstradati paslaik
        /// Paņemot jaunu policy apstradei šo mainīgu ir jaaizvieto,
        /// Beidzot apstrādi ir jaatgriež iepriekšejo vertību
        /// Tiek izdarīts, lai nebutu vajadzibas visu laiku staipīt Policy elementu
        /// </summary>
        protected XacmlPolicy currentEvaluatingPolicy = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="EvaluationEngine"/> class.
        /// </summary>
        /// <param name="policy">The XACML policy.</param>
        public EvaluationEngine(XacmlPolicy policy) {
            Contract.Requires<ArgumentNullException>(policy != null);

            this.types = DataTypeProcessor.Instance;
            this.functions = FunctionsProcessor.Instance;
            this.algorithms = AlgorithmsProcessor.Instance;
            this.policy = policy;
            this.namespaces = policy.Namespaces;
            this.xpathVersion = policy.XPathVersion;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EvaluationEngine"/> class.
        /// </summary>
        /// <param name="policySet">The XACML policy set.</param>
        public EvaluationEngine(XacmlPolicySet policySet) {
            Contract.Requires<ArgumentNullException>(policySet != null);

            this.types = DataTypeProcessor.Instance;
            this.functions = FunctionsProcessor.Instance;
            this.algorithms = AlgorithmsProcessor.Instance;
            this.policySet = policySet;
            this.namespaces = policySet.Namespaces;
            this.xpathVersion = policySet.XPathVersion;
        }

        /// <summary>
        /// Evaluates the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="requestDoc">The request document.</param>
        /// <returns>The response.</returns>
        public virtual XacmlContextResponse Evaluate(XacmlContextRequest request, XmlDocument requestDoc) {
            Contract.Requires<ArgumentNullException>(request != null);
            Contract.Requires<ArgumentNullException>(requestDoc != null);

            this.requestDoc = requestDoc;
            this.obligations = new Dictionary<XacmlEffectType, List<XacmlObligation>>()
            {
                { XacmlEffectType.Permit, new List<XacmlObligation>() },
                { XacmlEffectType.Deny, new List<XacmlObligation>() }
            };

            return new XacmlContextResponse(this.RequestEvaluate(request));
        }

        protected virtual IEnumerable<XacmlContextResult> RequestEvaluate(XacmlContextRequest request) {
            Contract.Requires<ArgumentNullException>(request != null);
            Contract.Assert(this.requestDoc != null);

            this.pip = new PolicyInformationPoint(request, this.requestDoc);

            // Hierarchical resources
            /*
            var scopeAttribute = request.Resources.SelectMany(x => x.Attributes).FirstOrDefault(y => y.AttributeId.OriginalString == "urn:oasis:names:tc:xacml:1.0:resource:scope");
            if (scopeAttribute != null) {
                
                var resourceAttrubute = request.Resources.SelectMany(x => x.Attributes).FirstOrDefault(y => y.AttributeId.OriginalString == "urn:oasis:names:tc:xacml:1.0:resource:resource-id");
                if (resourceAttrubute == null) {
                    // TODO: throw new XacmlPo
                }

                var resource = new XacmlContextResource(resourceAttrubute); 

                var refRequest = new XacmlContextRequest(resource, request.Action, request.Subjects);
                return this.RequestEvaluate(refRequest);
            }
             */ 

            XacmlContextResult result = null;
            try {
                XacmlDecisionResult decisionResult; 
                if (this.policySet != null) {
                    decisionResult = this.PolicySetEvaluate(this.policySet);
                }
                else if (this.policy != null) {
                    decisionResult = this.PolicyEvaluate(this.policy);
                }
                else {
                    throw Diagnostic.DiagnosticTools.ExceptionUtil.ThrowHelperError(new InvalidOperationException("Policy missing"));
                }

                result = this.MakeResult(decisionResult, new XacmlContextStatus(XacmlContextStatusCode.Success));
            }
            catch (XacmlException ex) {
                Diagnostic.DiagnosticTools.ExceptionUtil.ThrowHelperError(ex);
                result = this.MakeResult(XacmlDecisionResult.Indeterminate, new XacmlContextStatus(new XacmlContextStatusCode(ex.StatusCode)) { StatusMessage = ex.Message });
            }

            return new XacmlContextResult[] { result };
        }

        protected virtual XacmlContextResult MakeResult(XacmlDecisionResult decision, XacmlContextStatus status) {
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

            var result = new XacmlContextResult(resultDecision) {
                Status = status,
            };

            if (decision == XacmlDecisionResult.Permit) {
                foreach (var obligation in this.obligations[XacmlEffectType.Permit]) {
                    result.Obligations.Add(obligation);
                }
            }

            if (decision == XacmlDecisionResult.Deny) {
                foreach (var obligation in this.obligations[XacmlEffectType.Deny]) {
                    result.Obligations.Add(obligation);
                }
            }

            return result;
        }

        /// <summary>
        /// Apstrāda PolicySet objektu
        /// </summary>
        /// <param name="policySet">Izveidotājs PolicySet objekts</param>
        /// <returns></returns>
        public virtual XacmlDecisionResult PolicySetEvaluate(XacmlPolicySet policySet) {
            Contract.Requires<ArgumentNullException>(policySet != null);

            ///// <Target>                <Policy>                                    <Policy Set>
            ///// "Match"                 At least one rule value is its Decision     Specified by the policy combining algorithm
            ///// "Match"                 All rule values are “NotApplicable”         “NotApplicable”
            ///// “Match”                 At least one rule value is “Indeterminate”  Specified by the policy combining algorithm
            ///// “No-match”              Don’t care                                  “NotApplicable”
            ///// “Indeterminate”         Don’t care                                  “Indeterminate”

            // Target
            XacmlMatchResult targetResult;
            targetResult = this.TargetEvaluate(policySet.Target);

            if (targetResult == XacmlMatchResult.Indeterminate) {
                return XacmlDecisionResult.Indeterminate;
            }

            if (targetResult == XacmlMatchResult.NoMatch) {
                return XacmlDecisionResult.NotApplicable;
            }

            // Policy
            List<Tuple<IEnumerable<XacmlCombinerParameter>, IDictionary<string, Func<object>>>> policyResultsFunctions = new List<Tuple<IEnumerable<XacmlCombinerParameter>, IDictionary<string, Func<object>>>>();

            bool atLeastOnePolicy = policySet.Policies.Any()
                || policySet.PolicySets.Any()
                || policySet.PolicyIdReferences.Any()
                || policySet.PolicySetIdReferences.Any();

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
                                if(policyTargetResult == XacmlMatchResult.Indeterminate) {
                                    return null;
                                }

                                if (policyTargetResult == XacmlMatchResult.Match) {
                                    return true;
                                }

                                return false;
                            }
                        }
                    }));
            }

            // Policy References
            foreach (Uri polRef in policySet.PolicyIdReferences) {
                XacmlPolicy pol = this.ch.RequestPolicy(polRef);
                if (pol == null) {
                    throw Diagnostic.DiagnosticTools.ExceptionUtil.ThrowHelperError(new XacmlIndeterminateException("Unknown Policy reference: " + polRef.ToString()));
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
                                if(policyTargetResult == XacmlMatchResult.Indeterminate) {
                                    return null;
                                }

                                if (policyTargetResult == XacmlMatchResult.Match) {
                                    return true;
                                }

                                return false;
                            }
                        }
                    }));
            }

            // PolicySet References
            foreach (Uri polRef in policySet.PolicySetIdReferences) {
                XacmlPolicySet pol = this.ch.RequestPolicySet(polRef);
                if (pol == null) {
                    throw Diagnostic.DiagnosticTools.ExceptionUtil.ThrowHelperError(new XacmlIndeterminateException("Unknown PolicySet reference: " + polRef.ToString()));
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

            if (!atLeastOnePolicy) {
                return XacmlDecisionResult.NotApplicable;
            }

            XacmlDecisionResult algResult = this.algorithms[policySet.PolicyCombiningAlgId.ToString()].Invoke(policyResultsFunctions,
                policySet.CombinerParameters);

            if (algResult == XacmlDecisionResult.Permit) {
                this.obligations[XacmlEffectType.Permit].AddRange(policySet.Obligations.Where(o => o.FulfillOn == XacmlEffectType.Permit));
            }

            if (algResult == XacmlDecisionResult.Deny) {
                this.obligations[XacmlEffectType.Deny].AddRange(policySet.Obligations.Where(o => o.FulfillOn == XacmlEffectType.Deny));
            }

            return algResult;
        }

        public virtual XacmlDecisionResult PolicyEvaluate(XacmlPolicy policy) {
            Contract.Requires<ArgumentNullException>(policy != null);

            XacmlPolicy previousPolicy = this.currentEvaluatingPolicy;
            this.currentEvaluatingPolicy = policy;
            ///// <Target>                <Rule>                                      <Policy>
            ///// "Match"                 At least one rule value is its Effect       Specified by the rulecombining algorithm
            ///// "Match"                 All rule values are “NotApplicable”         “NotApplicable”
            ///// “Match”                 At least one rule value is “Indeterminate”  Specified by the rulecombining algorithm
            ///// “No-match”              Don’t care                                  “NotApplicable”
            ///// “Indeterminate”         Don’t care                                  “Indeterminate”

            // Target
            XacmlMatchResult targetResult;
            targetResult = this.TargetEvaluate(policy.Target);

            if (targetResult == XacmlMatchResult.Indeterminate) {
                return XacmlDecisionResult.Indeterminate;
            }

            if (targetResult == XacmlMatchResult.NoMatch) {
                return XacmlDecisionResult.NotApplicable;
            }

            // Rules
            List<Tuple<IEnumerable<XacmlCombinerParameter>, IDictionary<string, Func<object>>>> ruleResultsFunctions = new List<Tuple<IEnumerable<XacmlCombinerParameter>, IDictionary<string, Func<object>>>>();

            bool allRulesNotApplicable = true;
            foreach (XacmlRule rule in policy.Rules) {
                ruleResultsFunctions.Add(new Tuple<IEnumerable<XacmlCombinerParameter>, IDictionary<string, Func<object>>>(
                    policy.RuleCombinerParameters.Where(o => string.Equals(o.RuleIdRef, rule.RuleId)).SelectMany(o => o.CombinerParameters),
                    new Dictionary<string, Func<object>>()
                    {
                        {
                            "evaluate",
                            () =>
                            {
                                Tuple<XacmlDecisionResult, string> res = this.RuleEvaluate(rule);
                                if (res.Item1 != XacmlDecisionResult.NotApplicable)
                                    allRulesNotApplicable = false;
                                return res;
                            }
                        }
                    }));
            }

            XacmlDecisionResult algResult = this.algorithms[policy.RuleCombiningAlgId.ToString()].Invoke(ruleResultsFunctions,
                policy.CombinerParameters.Concat(policy.ChoiceCombinerParameters));

            this.currentEvaluatingPolicy = previousPolicy;

            if (allRulesNotApplicable) {
                return XacmlDecisionResult.NotApplicable;
            }
            else {
                if (algResult == XacmlDecisionResult.Permit) {
                    this.obligations[XacmlEffectType.Permit].AddRange(policy.Obligations.Where(o => o.FulfillOn == XacmlEffectType.Permit));
                }

                if (algResult == XacmlDecisionResult.Deny) {
                    this.obligations[XacmlEffectType.Deny].AddRange(policy.Obligations.Where(o => o.FulfillOn == XacmlEffectType.Deny));
                }

                return algResult;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="rule"></param>
        /// <param name="policy"></param>
        /// <returns>Rezultats un Rule Effect type, lai varētu korrekti piemerot Rule Combined Algorithms</returns>
        protected virtual Tuple<XacmlDecisionResult, string> RuleEvaluate(XacmlRule rule) {
            Contract.Requires<ArgumentNullException>(rule != null);

            ///// <Target>            <Condition>         <Rule>
            ///// "Match"             "True"              Effect
            ///// "Match"             "False"             "NotApplicable"
            ///// "Match"             "Indeterminate"     "Indeterminate"
            ///// "NoMatch"           Don`t care          "NotApplicable"
            ///// "Indeternimate"     Don`t care          "Indeterminate"

            // Target
            // If this element is omitted, then the target for the <Rule> SHALL be defined by the <Target> element of the enclosing <Policy> element.
            XacmlTarget target = rule.Target ?? this.currentEvaluatingPolicy.Target;
            XacmlMatchResult targetResult = this.TargetEvaluate(target);

            if (targetResult == XacmlMatchResult.NoMatch) {
                return new Tuple<XacmlDecisionResult, string>(XacmlDecisionResult.NotApplicable, Enum.GetName(typeof(XacmlEffectType), rule.Effect));
            }

            if (targetResult == XacmlMatchResult.Indeterminate) {
                return new Tuple<XacmlDecisionResult, string>(XacmlDecisionResult.Indeterminate, Enum.GetName(typeof(XacmlEffectType), rule.Effect));
            }

            // Condition
            // if XACML v 1.0
            // The condition value SHALL be "True" if the <Condition> element is absent
            bool? conditionResult = true;
            if (rule.Condition != null) {
                conditionResult = this.ConditionEvaluate(rule.Condition);
            }

            // Target is Match
            XacmlDecisionResult ruleResult;
            if (!conditionResult.HasValue) {
                ruleResult = XacmlDecisionResult.Indeterminate;
            }
            else if (conditionResult.Value) {
                ruleResult = (XacmlDecisionResult)(Enum.Parse(typeof(XacmlDecisionResult), Enum.GetName(typeof(XacmlEffectType), rule.Effect)));
            }
            else {
                ruleResult = XacmlDecisionResult.NotApplicable;
            }

            return new Tuple<XacmlDecisionResult, string>(ruleResult, Enum.GetName(typeof(XacmlEffectType), rule.Effect));
        }

        public bool? ConditionEvaluate(XacmlExpression condition) {
            Contract.Requires<ArgumentNullException>(condition != null);

            try {
                object conditionResult = this.ExpressionEvaluate(condition.Property);
                return conditionResult as bool?;
            }
            catch (XacmlIndeterminateException ex) {
                Diagnostic.DiagnosticTools.ExceptionUtil.TraceHandledException(ex, System.Diagnostics.TraceEventType.Warning);
                return null;
            }
            catch (InvalidOperationException ex) {
                Diagnostic.DiagnosticTools.ExceptionUtil.TraceHandledException(ex, System.Diagnostics.TraceEventType.Warning);
                return null;
            }
        }

        protected virtual object ExpressionEvaluate(IXacmlApply expression) {
            Contract.Requires<ArgumentNullException>(expression != null);

            object result = null;

            Type applyElemType = expression.GetType();
            if (applyElemType == typeof(XacmlVariableReference)) {
                XacmlVariableReference reference = expression as XacmlVariableReference;
                XacmlVariableDefinition definition = this.currentEvaluatingPolicy.VariableDefinitions.SingleOrDefault(o => o.VariableId == reference.VariableReference);
                if (definition == null) {
                    throw Diagnostic.DiagnosticTools.ExceptionUtil.ThrowHelperError(new XacmlInvalidSyntaxException("Missing Variable definition " + reference.VariableReference));
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
            else if (applyElemType == typeof(XacmlResourceAttributeDesignator)) {
                XacmlResourceAttributeDesignator design = expression as XacmlResourceAttributeDesignator;
                IEnumerable<string> designatorsNames = this.GetResourceAttributeDesignator(design);

                if (designatorsNames == null) {
                    throw new XacmlIndeterminateException("XacmlResourceAttributeDesignator indeterminate");
                }

                TypeConverterWrapper typeConverter = this.types[design.DataType.ToString()];
                result = typeConverter.ConvertEnumerable(designatorsNames.Select(o => typeConverter.ConvertFromString(o, design)));
            }
            else if (applyElemType == typeof(XacmlActionAttributeDesignator)) {
                XacmlActionAttributeDesignator design = expression as XacmlActionAttributeDesignator;
                IEnumerable<string> designatorsNames = this.GetActionAttributeDesignator(design);

                if (designatorsNames == null) {
                    throw Diagnostic.DiagnosticTools.ExceptionUtil.ThrowHelperError(new XacmlIndeterminateException("XacmlActionAttributeDesignator indeterminate"));
                }

                TypeConverterWrapper typeConverter = this.types[design.DataType.ToString()];
                result = typeConverter.ConvertEnumerable(designatorsNames.Select(o => typeConverter.ConvertFromString(o, design)));
            }
            else if (applyElemType == typeof(XacmlEnvironmentAttributeDesignator)) {
                XacmlEnvironmentAttributeDesignator design = expression as XacmlEnvironmentAttributeDesignator;
                IEnumerable<string> designatorsNames = this.GetEnvironmentAttributeDesignator(design);

                if (designatorsNames == null) {
                    throw new XacmlIndeterminateException("XacmlEnvironmentAttributeDesignator indeterminate");
                }

                TypeConverterWrapper typeConverter = this.types[design.DataType.ToString()];
                result = typeConverter.ConvertEnumerable(designatorsNames.Select(o => typeConverter.ConvertFromString(o, design)));
            }
            else if (applyElemType == typeof(XacmlSubjectAttributeDesignator)) {
                XacmlSubjectAttributeDesignator design = expression as XacmlSubjectAttributeDesignator;
                IEnumerable<string> designatorsNames = this.GetSubjectAttributeDesignator(design);

                if (designatorsNames == null) {
                    throw new XacmlIndeterminateException("XacmlSubjectAttributeDesignator indeterminate");
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

        protected object ApplyEvaluate(XacmlApply apply) {
            Contract.Requires<ArgumentNullException>(apply != null);

            // get function
            DelegateWrapper matchFunction = this.functions[apply.FunctionId.ToString()];
            object[] parameters = apply.Parameters.Select(o => this.ExpressionEvaluate(o)).ToArray();
            object result = matchFunction.DynamicInvoke(new XPathContext(this.xpathVersion, this.requestDoc, this.namespaces), parameters);
            return result;
        }

        protected virtual XacmlMatchResult TargetEvaluate(XacmlTarget target) {
            Contract.Requires<ArgumentNullException>(target != null);

            //// Subjects
            ///// <Subject> values                <Subjects> Value
            ///// At least one “Match”            “Match”
            ///// None matches and at             “Indeterminate”
            ///// least one “Indeterminate”
            ///// All “No match”                  “No match”
            XacmlMatchResult subjectResult = XacmlMatchResult.NoMatch;
            if (target.Subjects.Count == 0)
                subjectResult = XacmlMatchResult.Match;

            foreach (XacmlSubject subj in target.Subjects) {
                ///// <SubjectMatch> values       <Subject> Value
                ///// All “True”                  “Match”
                ///// No “False” and at least     “Indeterminate”
                ///// one “Indeterminate”
                ///// At least one “False”        “No match”
                XacmlMatchResult matchResult = XacmlMatchResult.Match;
                foreach (XacmlSubjectMatch match in subj.Matches.OfType<XacmlSubjectMatch>()) {
                    bool? mres = this.SubjectMatchEvaluation(match);
                    if (!mres.HasValue) {
                        matchResult = XacmlMatchResult.Indeterminate;
                    }
                    else {
                        if (!mres.Value) {
                            matchResult = XacmlMatchResult.NoMatch;
                            break;
                        }
                    }
                }

                if (matchResult == XacmlMatchResult.Indeterminate) {
                    subjectResult = XacmlMatchResult.Indeterminate;
                }
                else {
                    if (matchResult == XacmlMatchResult.Match) {
                        subjectResult = XacmlMatchResult.Match;
                        break;
                    }
                }
            }

            // Resources
            ///// <Resource> values               <Resources> Value
            ///// At least one “Match”            “Match”
            ///// None matches and at             “Indeterminate”
            ///// least one “Indeterminate”
            ///// All “No match”                  “No match”
            XacmlMatchResult resourceResult = XacmlMatchResult.NoMatch;
            if (target.Resources.Count == 0)
                resourceResult = XacmlMatchResult.Match;

            foreach (XacmlResource res in target.Resources) {
                ///// <ResourceMatch> values      <Resource> Value
                ///// All “True”                  “Match”
                ///// No “False” and at least     “Indeterminate”
                ///// one “Indeterminate”
                ///// At least one “False”        “No match”
                XacmlMatchResult matchResult = XacmlMatchResult.Match;
                foreach (XacmlResourceMatch match in res.Matches.OfType<XacmlResourceMatch>()) {
                    bool? mres = this.ResourceMatchEvaluation(match);
                    if (!mres.HasValue) {
                        matchResult = XacmlMatchResult.Indeterminate;
                    }
                    else {
                        if (!mres.Value) {
                            matchResult = XacmlMatchResult.NoMatch;
                            break;
                        }
                    }
                }

                if (matchResult == XacmlMatchResult.Indeterminate) {
                    resourceResult = XacmlMatchResult.Indeterminate;
                }
                else {
                    if (matchResult == XacmlMatchResult.Match) {
                        resourceResult = XacmlMatchResult.Match;
                        break;
                    }
                }
            }

            // Actions
            ///// <Action> values                 <Actions> Value
            ///// At least one “Match”            “Match”
            ///// None matches and at             “Indeterminate”
            ///// least one “Indeterminate”
            ///// All “No match”                  “No match”
            XacmlMatchResult actionsResult = XacmlMatchResult.NoMatch;
            if (target.Actions.Count == 0)
                actionsResult = XacmlMatchResult.Match;

            foreach (XacmlAction act in target.Actions) {
                ///// <ActionMatch> values        <Action> Value
                ///// All “True”                  “Match”
                ///// No “False” and at least     “Indeterminate”
                ///// one “Indeterminate”
                ///// At least one “False”        “No match”
                XacmlMatchResult matchResult = XacmlMatchResult.Match;
                foreach (XacmlActionMatch match in act.Matches.OfType<XacmlActionMatch>()) {
                    bool? mres = this.ActionMatchEvaluation(match);
                    if (!mres.HasValue) {
                        matchResult = XacmlMatchResult.Indeterminate;
                    }
                    else {
                        if (!mres.Value) {
                            matchResult = XacmlMatchResult.NoMatch;
                            break;
                        }
                    }
                }

                if (matchResult == XacmlMatchResult.Indeterminate) {
                    actionsResult = XacmlMatchResult.Indeterminate;
                }
                else {
                    if (matchResult == XacmlMatchResult.Match) {
                        actionsResult = XacmlMatchResult.Match;
                        break;
                    }
                }
            }

            // Environments
            ///// <Environment> values            <Environments> Value
            ///// At least one “Match”            “Match”
            ///// None matches and at             “Indeterminate”
            ///// least one “Indeterminate”
            ///// All “No match”                  “No match”
            XacmlMatchResult environmentResult = XacmlMatchResult.NoMatch;
            if (target.Environments.Count == 0)
                environmentResult = XacmlMatchResult.Match;

            foreach (XacmlEnvironment env in target.Environments) {
                ///// <EnvironmentMatch> values   <Environment> Value
                ///// All “True”                  “Match”
                ///// No “False” and at least     “Indeterminate”
                ///// one “Indeterminate”
                ///// At least one “False”        “No match”
                XacmlMatchResult matchResult = XacmlMatchResult.Match;
                foreach (XacmlEnvironmentMatch match in env.Matches.OfType<XacmlEnvironmentMatch>()) {
                    bool? mres = this.EnvironmentMatchEvaluation(match);
                    if (!mres.HasValue) {
                        matchResult = XacmlMatchResult.Indeterminate;
                    }
                    else {
                        if (!mres.Value) {
                            matchResult = XacmlMatchResult.NoMatch;
                            break;
                        }
                    }
                }

                if (matchResult == XacmlMatchResult.Indeterminate) {
                    environmentResult = XacmlMatchResult.Indeterminate;
                }
                else {
                    if (matchResult == XacmlMatchResult.Match) {
                        environmentResult = XacmlMatchResult.Match;
                        break;
                    }
                }
            }

            ///// Any "Indeterminate"         "Indeterminate"
            ///// Any "No Match"              "No Match"
            ///// All Match                   "Match"
            XacmlMatchResult targetMatchResult;
            if (subjectResult == XacmlMatchResult.Indeterminate
                || resourceResult == XacmlMatchResult.Indeterminate
                || actionsResult == XacmlMatchResult.Indeterminate
                || environmentResult == XacmlMatchResult.Indeterminate) {
                targetMatchResult = XacmlMatchResult.Indeterminate;
            }
            else if (subjectResult == XacmlMatchResult.NoMatch
                || resourceResult == XacmlMatchResult.NoMatch
                || actionsResult == XacmlMatchResult.NoMatch
                || environmentResult == XacmlMatchResult.NoMatch) {
                targetMatchResult = XacmlMatchResult.NoMatch;
            }
            else {
                targetMatchResult = XacmlMatchResult.Match;
            }

            return targetMatchResult;
        }

        public bool? SubjectMatchEvaluation(XacmlSubjectMatch match) {
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
                attribute2Values = this.GetSubjectAttributeDesignator(match.AttributeDesignator as XacmlSubjectAttributeDesignator);
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

            bool? subjectMatchResult = false;
            foreach (string designatorValue in attribute2Values) {
                object attribute2 = this.types[dataType].ConvertFromString(designatorValue, source);

                // evaluate
                bool? functionResult = (bool)matchFunction.DynamicInvoke(new XPathContext(this.xpathVersion, this.requestDoc, this.namespaces), attribute1, attribute2);

                // Otherwise, if at least one of the function applications results in "Indeterminate", then the result SHALL be "Indeterminate".
                if (!functionResult.HasValue) {
                    subjectMatchResult = null;
                }
                // If at least one of those function applications were to evaluate to "True", then the result of the entire expression SHALL be "True".
                else if (functionResult.Value) {
                    return true;
                }
            }

            return subjectMatchResult;
        }

        public bool? ResourceMatchEvaluation(XacmlResourceMatch match) {
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
                attribute2Values = this.GetResourceAttributeDesignator(match.AttributeDesignator as XacmlResourceAttributeDesignator);
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

            bool? resourceMatchResult = false;
            foreach (string designatorValue in attribute2Values) {
                object attribute2 = this.types[dataType].ConvertFromString(designatorValue, source);

                // evaluate
                bool? functionResult = (bool)matchFunction.DynamicInvoke(new XPathContext(this.xpathVersion, this.requestDoc, this.namespaces), attribute1, attribute2);

                // Otherwise, if at least one of the function applications results in "Indeterminate", then the result SHALL be "Indeterminate".
                if (!functionResult.HasValue) {
                    resourceMatchResult = null;
                }
                // If at least one of those function applications were to evaluate to "True", then the result of the entire expression SHALL be "True".
                else if (functionResult.Value) {
                    return true;
                }
            }

            return resourceMatchResult;
        }

        public bool? ActionMatchEvaluation(XacmlActionMatch match) {
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
                attribute2Values = this.GetActionAttributeDesignator(match.AttributeDesignator as XacmlActionAttributeDesignator);
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

            bool? actionMatchResult = false;
            foreach (string designatorValue in attribute2Values) {
                object attribute2 = this.types[dataType].ConvertFromString(designatorValue, source);

                // evaluate
                bool? functionResult = (bool)matchFunction.DynamicInvoke(new XPathContext(this.xpathVersion, this.requestDoc, this.namespaces), attribute1, attribute2);

                // Otherwise, if at least one of the function applications results in "Indeterminate", then the result SHALL be "Indeterminate".
                if (!functionResult.HasValue) {
                    actionMatchResult = null;
                }
                // If at least one of those function applications were to evaluate to "True", then the result of the entire expression SHALL be "True".
                else if (functionResult.Value) {
                    return true;
                }
            }

            return actionMatchResult;
        }

        public bool? EnvironmentMatchEvaluation(XacmlEnvironmentMatch match) {
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
                attribute2Values = this.GetEnvironmentAttributeDesignator(match.AttributeDesignator as XacmlEnvironmentAttributeDesignator);
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

            bool? environmentMatchResult = false;
            foreach (string designatorValue in attribute2Values) {
                object attribute2 = this.types[dataType].ConvertFromString(designatorValue, source);

                // evaluate
                bool? functionResult = (bool)matchFunction.DynamicInvoke(new XPathContext(this.xpathVersion, this.requestDoc, this.namespaces), attribute1, attribute2);

                // Otherwise, if at least one of the function applications results in "Indeterminate", then the result SHALL be "Indeterminate".
                if (!functionResult.HasValue) {
                    environmentMatchResult = null;
                }
                // If at least one of those function applications were to evaluate to "True", then the result of the entire expression SHALL be "True".
                else if (functionResult.Value) {
                    return true;
                }
            }

            return environmentMatchResult;
        }

        protected virtual IEnumerable<string> GetAttributeSelector(XacmlAttributeSelector selector) {
            Contract.Requires<ArgumentNullException>(selector != null);

            IEnumerable<XmlNode> attributeBag = this.pip.GetAttributeByXPath(this.xpathVersion, selector.Path, this.namespaces);

            if (!attributeBag.Any()) {
                if (selector.MustBePresent.HasValue && selector.MustBePresent.Value) {
                    // return "Indeterminate”
                    return null;
                }
            }

            return attributeBag.Select(o => o.Value);
        }

        protected IEnumerable<string> GetSubjectAttributeDesignator(XacmlSubjectAttributeDesignator designator) {
            Contract.Requires<ArgumentNullException>(designator != null);

            IEnumerable<string> attributeBag = this.pip.GetSubjectAttributeDesignatorValues(
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

        protected IEnumerable<string> GetResourceAttributeDesignator(XacmlResourceAttributeDesignator designator) {
            Contract.Requires<ArgumentNullException>(designator != null);

            IEnumerable<string> attributeBag = this.pip.GetResourceAttributeDesignatorValues(
                    designator.AttributeId,
                    designator.DataType,
                    designator.Issuer
                    );

            if (!attributeBag.Any()) {
                if (designator.MustBePresent.HasValue && designator.MustBePresent.Value) {
                    // return "Indeterminate”
                    return null;
                }
            }

            return attributeBag;
        }

        protected IEnumerable<string> GetActionAttributeDesignator(XacmlActionAttributeDesignator designator) {
            Contract.Requires<ArgumentNullException>(designator != null);

            IEnumerable<string> attributeBag = this.pip.GetActionAttributeDesignatorValues(
                    designator.AttributeId,
                    designator.DataType,
                    designator.Issuer
                    );

            if (!attributeBag.Any()) {
                if (designator.MustBePresent.HasValue && designator.MustBePresent.Value) {
                    // return "Indeterminate”
                    return null;
                }
            }

            return attributeBag;
        }

        protected IEnumerable<string> GetEnvironmentAttributeDesignator(XacmlEnvironmentAttributeDesignator designator) {
            Contract.Requires<ArgumentNullException>(designator != null);

            IEnumerable<string> attributeBag = this.pip.GetEnvironmentAttributeDesignatorValues(
                    designator.AttributeId,
                    designator.DataType,
                    designator.Issuer
                    );

            if (!attributeBag.Any()) {
                if (designator.MustBePresent.HasValue && designator.MustBePresent.Value) {
                    // return "Indeterminate”
                    return null;
                }
            }

            return attributeBag;
        }
    }
}