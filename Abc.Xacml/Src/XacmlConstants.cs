// ----------------------------------------------------------------------------
// <copyright file="XacmlConstants.cs" company="ABC Software Ltd">
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
 
namespace Abc.Xacml {
    using System;
    using System.Diagnostics.CodeAnalysis;

#pragma warning disable 1591

    [SuppressMessage("Microsoft.StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Constants is not commented.")]
    [ExcludeFromCodeCoverage]
    public sealed class XacmlConstants {
        private XacmlConstants() {
        }

        public sealed class Prefixes {
            /// <summary>
            /// The XACML policy namespace prefix.
            /// </summary>
            public const string Policy = "xacml";

            /// <summary>
            /// The XACML Context namespace prefix.
            /// </summary>
            public const string Context = "xacml-context";

            /// <summary>
            /// The XACML SAML2.0 assertion namespace prefix.
            /// </summary>
            public const string Assertion = "xacml-saml";

            /// <summary>
            /// The XACML SAML2.0 protocol namespace prefix.
            /// </summary>
            public const string Protocol = "xacml-samlp";

            /// <summary>
            /// The XML namespace prefix.
            /// </summary>
            public const string Xml = "xml";
        }

        public sealed class Namespaces {
            public const string Namespace = "urn:oasis:names:tc:xacml:1.0:policy";
            public const string XmlNamespace = "http://www.w3.org/XML/1998/namespace";
        }

        [SuppressMessage("Microsoft.StyleCop.CSharp.OrderingRules", "SA1202:ElementsMustBeOrderedByAccess", Justification = "Reviewed.")]
        public sealed class StatusCodes {
            /// <summary>
            /// This identifier indicates success.
            /// </summary>
            public static readonly Uri Success = new Uri(SuccessString);
            private const string SuccessString = "urn:oasis:names:tc:xacml:1.0:status:ok";

            /// <summary>
            /// This identifier indicates that all the attributes necessary to make a policy decision were not available.
            /// </summary>
            public static readonly Uri MissingAttribute = new Uri(MissingAttributeString);
            private const string MissingAttributeString = "urn:oasis:names:tc:xacml:1.0:status:missing-attribute";

            /// <summary>
            /// This identifier indicates that some attribute value contained a syntax error, such as a letter in a numeric field.
            /// </summary>
            public static readonly Uri SyntaxError = new Uri(SyntaxErrorString);
            private const string SyntaxErrorString = "urn:oasis:names:tc:xacml:1.0:status:syntax-error";

            /// <summary>
            /// This identifier indicates that an error occurred during policy evaluation. An example would be division by zero.
            /// </summary>
            public static readonly Uri ProcessingError = new Uri(ProcessingErrorString);
            private const string ProcessingErrorString = "urn:oasis:names:tc:xacml:1.0:status:processing-error";

            private StatusCodes() {
            }
        }

        public sealed class AttributeNames {
            public const string PolicySetId = "PolicySetId";
            public const string PolicyCombiningAlgId = "PolicyCombiningAlgId";
            public const string FulfillOn = "FulfillOn";
            public const string MatchId = "MatchId";
            public const string ObligationId = "ObligationId";
            public const string AttributeId = "AttributeId";
            public const string DataType = "DataType";
            public const string Issuer = "Issuer";
            public const string MustBePresent = "MustBePresent";
            public const string RequestContextPath = "RequestContextPath";
            public const string SubjectCategory = "SubjectCategory";
            public const string Category = "Category";

            public const string PolicyId = "PolicyId";
            public const string RuleCombiningAlgId = "RuleCombiningAlgId";

            public const string RuleId = "RuleId";
            public const string Effect = "Effect";
            public const string FunctionId = "FunctionId";

            // 2.0
            public const string Version = "Version";
            public const string ParameterName = "ParameterName";
            public const string VariableId = "VariableId";
            public const string RuleIdRef = "RuleIdRef";
            public const string PolicyIdRef = "PolicyIdRef";
            public const string PolicySetIdRef = "PolicySetIdRef";
            public const string ResourceId = "ResourceId";
            public const string Value = "Value";

            // 3.0
            public const string MaxDelegationDepth = "MaxDelegationDepth";
            public const string IncludeInResult = "IncludeInResult";
            public const string AppliesTo = "AppliesTo";
            public const string AdviceId = "AdviceId";
            public const string ReturnPolicyIdList = "ReturnPolicyIdList";
            public const string CombinedDecision = "CombinedDecision";
            public const string ReferenceId = "ReferenceId";
            public const string Id = "id";
            public const string EarliestVersion = "EarliestVersion";
            public const string LatestVersion = "LatestVersion";
            public const string ContextSelectorId = "ContextSelectorId";
            public const string Path = "Path";

            // context
            public const string IssueInstant = "IssueInstant";

            private AttributeNames() {
            }
        }

        public sealed class ElementNames {
            public const string PolicySet = "PolicySet";
            public const string Policy = "Policy";
            public const string Rule = "Rule";
            public const string Description = "Description";
            public const string PolicySetIdReference = "PolicySetIdReference";
            public const string PolicyIdReference = "PolicyIdReference";

            public const string PolicySetDefaults = "PolicySetDefaults";
            public const string PolicyDefaults = "PolicyDefaults";
            public const string XPathVersion = "XPathVersion";

            public const string Target = "Target";
            public const string Subjects = "Subjects";
            public const string Subject = "Subject";
            public const string AnySubject = "AnySubject";
            public const string SubjectMatch = "SubjectMatch";
            public const string AttributeValue = "AttributeValue";
            public const string SubjectAttributeDesignator = "SubjectAttributeDesignator";
            public const string AttributeSelector = "AttributeSelector";

            public const string Resources = "Resources";
            public const string Resource = "Resource";
            public const string AnyResource = "AnyResource";
            public const string ResourceMatch = "ResourceMatch";
            public const string ResourceAttributeDesignator = "ResourceAttributeDesignator";

            public const string Actions = "Actions";
            public const string Action = "Action";
            public const string AnyAction = "AnyAction";
            public const string ActionMatch = "ActionMatch";
            public const string ActionAttributeDesignator = "ActionAttributeDesignator";

            public const string Environments = "Environments";
            public const string Environment = "Environment";
            public const string EnvironmentMatch = "EnvironmentMatch";
            public const string EnvironmentAttributeDesignator = "EnvironmentAttributeDesignator";

            public const string Obligations = "Obligations";
            public const string Obligation = "Obligation";
            public const string AttributeAssignment = "AttributeAssignment";

            public const string Condition = "Condition";

            public const string Apply = "Apply";
            public const string Function = "Function";

            // 2.0
            public const string CombinerParameters = "CombinerParameters";
            public const string CombinerParameter = "CombinerParameter";
            public const string RuleCombinerParameters = "RuleCombinerParameters";
            public const string PolicyCombinerParameters = "PolicyCombinerParameters";
            public const string PolicySetCombinerParameters = "PolicySetCombinerParameters";
            public const string VariableDefinition = "VariableDefinition";
            public const string VariableReference = "VariableReference";

            // 3.0
            public const string AnyOf = "AnyOf";
            public const string AllOf = "AllOf";
            public const string Match = "Match";
            public const string AttributeDesignator = "AttributeDesignator";
            public const string PolicyIssuer = "PolicyIssuer";
            public const string Content = "Content";
            public const string ObligationExpressions = "ObligationExpressions";
            public const string ObligationExpression = "ObligationExpression";
            public const string AdviceExpressions = "AdviceExpressions";
            public const string AdviceExpression = "AdviceExpression";
            public const string AttributeAssignmentExpression = "AttributeAssignmentExpression";

            public const string RequestDefaults = "RequestDefaults";
            public const string Attributes = "Attributes";
            public const string MultiRequests = "MultiRequests";
            public const string RequestReference = "RequestReference";
            public const string AttributesReference = "AttributesReference";
            public const string Advice = "Advice";
            public const string AssociatedAdvice = "AssociatedAdvice";
            public const string PolicyIdentifierList = "PolicyIdentifierList";

            // Context
            public const string Attribute = "Attribute";
            public const string Decision = "Decision";
            public const string Request = "Request";
            public const string ResourceContent = "ResourceContent";
            public const string Response = "Response";
            public const string Result = "Result";
            public const string Status = "Status";
            public const string StatusCode = "StatusCode";
            public const string StatusDetail = "StatusDetail";
            public const string StatusMessage = "StatusMessage";

            private ElementNames() {
            }
        }
    }

#pragma warning restore 1591
}
