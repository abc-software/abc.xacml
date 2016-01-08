// ----------------------------------------------------------------------------
// <copyright file="AlgorithmsProcessor.cs" company="ABC Software Ltd">
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
    using System.ComponentModel.Composition.Hosting;
    using Abc.Xacml.Interfaces;
    using Abc.Xacml.Policy;

    public class AlgorithmsProcessor {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="algorithmsElementsData">
        /// <code> IEnumerable<Tuple<IEnumerable<XacmlCombinerParameter>, IDictionary<string, Func<object>>>> </code>
        ///        IEnumerable - katrai Policy/Role
        ///                    Tuple - parametru kopa, kura tiek nodota Algoritmu funkcijai
        ///                         IEnumerable<XacmlCombinerParameters> - saraksts ar parametriem, kuri tiek piesaistīti konkrētam Policy/Rule
        ///                                                                IDictionary<string, Func<object> saraksts ar funkcijam, kuras var izmantot algoritmi
        ///                                                                                   Func - funkcija, kuru izpildot tiek iegūts Policy/Rule rezultāts
        ///                                                                                             XacmlDecisionResult - Policy/Rule rezultāts
        ///                                                                                             string - Rule EffectType
        /// </param>
        /// <param name="additionalParams">
        /// Papildparametru saraksts, CombinerParameters elements
        /// </param>
        /// <returns></returns>
        public delegate XacmlDecisionResult AlgorithmsRunner(IEnumerable<Tuple<IEnumerable<XacmlCombinerParameter>, IDictionary<string, Func<object>>>> algorithmsElementsData, IEnumerable<XacmlCombinerParameter> additionalParams);

        private static AlgorithmsProcessor processor = null;
        private static object locker = new object();

        /// <summary>
        /// Nepadodu pilnu rule objektu, lai funkcijam nedot iespēju mainīt rule objektu
        /// </summary>
        private static SortedDictionary<string, AlgorithmsRunner> algorithms = new SortedDictionary<string, AlgorithmsRunner>()
        {
            { "urn:oasis:names:tc:xacml:1.0:rule-combining-algorithm:deny-overrides", RuleDenyOverrides  },
            { "urn:oasis:names:tc:xacml:1.1:rule-combining-algorithm:ordered-deny-overrides", RuleDenyOverrides  },
            { "urn:oasis:names:tc:xacml:1.0:rule-combining-algorithm:permit-overrides", RulePermitOverrides  },
            { "urn:oasis:names:tc:xacml:1.1:rule-combining-algorithm:ordered-permit-overrides", RulePermitOverrides  },
            { "urn:oasis:names:tc:xacml:1.0:rule-combining-algorithm:first-applicable", RuleFirstApplicable  },

            { "urn:oasis:names:tc:xacml:1.0:policy-combining-algorithm:deny-overrides", PolicyDenyOverrides  },
            { "urn:oasis:names:tc:xacml:1.1:policy-combining-algorithm:ordered-deny-overrides", PolicyDenyOverrides  },
            { "urn:oasis:names:tc:xacml:1.0:policy-combining-algorithm:permit-overrides", PolicyPermitOverrides  },
            { "urn:oasis:names:tc:xacml:1.1:policy-combining-algorithm:ordered-permit-overrides", PolicyPermitOverrides  },
            { "urn:oasis:names:tc:xacml:1.0:policy-combining-algorithm:first-applicable", PolicyFirstApplicable  },
            { "urn:oasis:names:tc:xacml:1.0:policy-combining-algorithm:only-one-applicable", PolicyOnlyOneApplicable  },

            // 3.0
            { "urn:oasis:names:tc:xacml:3.0:rule-combining-algorithm:deny-overrides", DenyOvverides_30  },
            { "urn:oasis:names:tc:xacml:3.0:policy-combining-algorithm:deny-overrides", DenyOvverides_30  },
            { "urn:oasis:names:tc:xacml:3.0:rule-combining-algorithm:ordered-deny-overrides", DenyOvverides_30  },
            { "urn:oasis:names:tc:xacml:3.0:policy-combining-algorithm:ordered-deny-overrides", DenyOvverides_30  },
            { "urn:oasis:names:tc:xacml:3.0:rule-combining-algorithm:permit-overrides", PermitOvverides_30  },
            { "urn:oasis:names:tc:xacml:3.0:policy-combining-algorithm:permit-overrides", PermitOvverides_30  },
            { "urn:oasis:names:tc:xacml:3.0:rule-combining-algorithm:ordered-permit-overrides", PermitOvverides_30  },
            { "urn:oasis:names:tc:xacml:3.0:policy-combining-algorithm:ordered-permit-overrides", PermitOvverides_30  },
            { "urn:oasis:names:tc:xacml:3.0:rule-combining-algorithm:deny-unless-permit", DenyUnlessPermit_30  },
            { "urn:oasis:names:tc:xacml:3.0:policy-combining-algorithm:deny-unless-permit", DenyUnlessPermit_30  },
            { "urn:oasis:names:tc:xacml:3.0:rule-combining-algorithm:permit-unless-deny", PermitUnlessDeny_30  },
            { "urn:oasis:names:tc:xacml:3.0:policy-combining-algorithm:permit-unless-deny", PermitUnlessDeny_30  },
        };

        /// <summary>
        /// Prevents a default instance of the <see cref="AlgorithmsProcessor"/> class from being created.
        /// </summary>
        private AlgorithmsProcessor() {
        }

        public static XacmlDecisionResult RuleDenyOverrides(IEnumerable<Tuple<IEnumerable<XacmlCombinerParameter>, IDictionary<string, Func<object>>>> results, IEnumerable<XacmlCombinerParameter> additionalParams) {
            bool atLeastOneError = false;
            bool potentialDeny = false;
            bool atLeastOnePermit = false;

            foreach (Tuple<IEnumerable<XacmlCombinerParameter>, IDictionary<string, Func<object>>> resultDataFunc in results) {
                Tuple<XacmlDecisionResult, string> resultData = (Tuple<XacmlDecisionResult, string>)resultDataFunc.Item2["evaluate"]();

                XacmlDecisionResult result = resultData.Item1;

                if (result == XacmlDecisionResult.Deny) {
                    return XacmlDecisionResult.Deny;
                }

                if (result == XacmlDecisionResult.Permit) {
                    atLeastOnePermit = true;
                    continue;
                }

                if (result == XacmlDecisionResult.NotApplicable) {
                    continue;
                }

                if (result == XacmlDecisionResult.Indeterminate) {
                    atLeastOneError = true;

                    if (resultData.Item2 == "Deny") {
                        potentialDeny = true;
                    }
                    continue;
                }
            }

            if (potentialDeny) {
                return XacmlDecisionResult.Indeterminate;
            }

            if (atLeastOnePermit) {
                return XacmlDecisionResult.Permit;
            }

            if (atLeastOneError) {
                return XacmlDecisionResult.Indeterminate;
            }

            return XacmlDecisionResult.NotApplicable;
        }

        public static XacmlDecisionResult PolicyDenyOverrides(IEnumerable<Tuple<IEnumerable<XacmlCombinerParameter>, IDictionary<string, Func<object>>>> results, IEnumerable<XacmlCombinerParameter> additionalParams) {
            bool atLeastOnePermit = false;
            foreach (Tuple<IEnumerable<XacmlCombinerParameter>, IDictionary<string, Func<object>>> resultFunc in results) {
                Tuple<XacmlDecisionResult, string> result = (Tuple<XacmlDecisionResult, string>)resultFunc.Item2["evaluate"]();

                if (result.Item1 == XacmlDecisionResult.Deny) {
                    return XacmlDecisionResult.Deny;
                }
                if (result.Item1 == XacmlDecisionResult.Permit) {
                    atLeastOnePermit = true;
                    continue;
                }
                if (result.Item1 == XacmlDecisionResult.NotApplicable) {
                    continue;
                }
                if (result.Item1 == XacmlDecisionResult.Indeterminate) {
                    return XacmlDecisionResult.Deny;
                }
            }
            if (atLeastOnePermit) {
                return XacmlDecisionResult.Permit;
            }
            return XacmlDecisionResult.NotApplicable;
        }

        public static XacmlDecisionResult RulePermitOverrides(IEnumerable<Tuple<IEnumerable<XacmlCombinerParameter>, IDictionary<string, Func<object>>>> results, IEnumerable<XacmlCombinerParameter> additionalParams) {
            bool atLeastOneError = false;
            bool potentialPermit = false;
            bool atLeastOneDeny = false;

            foreach (Tuple<IEnumerable<XacmlCombinerParameter>, IDictionary<string, Func<object>>> resultDataFunc in results) {
                Tuple<XacmlDecisionResult, string> resultData = (Tuple<XacmlDecisionResult, string>)resultDataFunc.Item2["evaluate"]();

                XacmlDecisionResult result = resultData.Item1;

                if (result == XacmlDecisionResult.Deny) {
                    atLeastOneDeny = true;
                    continue;
                }

                if (result == XacmlDecisionResult.Permit) {
                    return XacmlDecisionResult.Permit;
                }

                if (result == XacmlDecisionResult.NotApplicable) {
                    continue;
                }

                if (result == XacmlDecisionResult.Indeterminate) {
                    atLeastOneError = true;

                    if (resultData.Item2 == "Permit") {
                        potentialPermit = true;
                    }
                    continue;
                }
            }

            if (potentialPermit) {
                return XacmlDecisionResult.Indeterminate;
            }

            if (atLeastOneDeny) {
                return XacmlDecisionResult.Deny;
            }

            if (atLeastOneError) {
                return XacmlDecisionResult.Indeterminate;
            }

            return XacmlDecisionResult.NotApplicable;
        }

        public static XacmlDecisionResult PolicyPermitOverrides(IEnumerable<Tuple<IEnumerable<XacmlCombinerParameter>, IDictionary<string, Func<object>>>> results, IEnumerable<XacmlCombinerParameter> additionalParams) {
            bool atLeastOneError = false;
            bool atLeastOneDeny = false;
            foreach (Tuple<IEnumerable<XacmlCombinerParameter>, IDictionary<string, Func<object>>> resultDataFunc in results) {
                Tuple<XacmlDecisionResult, string> result = (Tuple<XacmlDecisionResult, string>)resultDataFunc.Item2["evaluate"]();

                if (result.Item1 == XacmlDecisionResult.Deny) {
                    atLeastOneDeny = true;
                    continue;
                }
                if (result.Item1 == XacmlDecisionResult.Permit) {
                    return XacmlDecisionResult.Permit;
                }
                if (result.Item1 == XacmlDecisionResult.NotApplicable) {
                    continue;
                }
                if (result.Item1 == XacmlDecisionResult.Indeterminate) {
                    atLeastOneError = true;
                    continue;
                }
            }
            if (atLeastOneDeny) {
                return XacmlDecisionResult.Deny;
            }

            if (atLeastOneError) {
                return XacmlDecisionResult.Indeterminate;
            }

            return XacmlDecisionResult.NotApplicable;
        }

        public static XacmlDecisionResult RuleFirstApplicable(IEnumerable<Tuple<IEnumerable<XacmlCombinerParameter>, IDictionary<string, Func<object>>>> results, IEnumerable<XacmlCombinerParameter> additionalParams) {
            foreach (Tuple<IEnumerable<XacmlCombinerParameter>, IDictionary<string, Func<object>>> resultDataFunc in results) {
                Tuple<XacmlDecisionResult, string> resultData = (Tuple<XacmlDecisionResult, string>)resultDataFunc.Item2["evaluate"]();

                XacmlDecisionResult result = resultData.Item1;

                if (result == XacmlDecisionResult.Deny) {
                    return XacmlDecisionResult.Deny;
                }

                if (result == XacmlDecisionResult.Permit) {
                    return XacmlDecisionResult.Permit;
                }

                if (result == XacmlDecisionResult.NotApplicable) {
                    continue;
                }

                if (result == XacmlDecisionResult.Indeterminate
                    || result == XacmlDecisionResult.IndeterminateD
                    || result == XacmlDecisionResult.IndeterminateP
                    || result == XacmlDecisionResult.IndeterminateDP) {
                    return XacmlDecisionResult.Indeterminate;
                }
            }

            return XacmlDecisionResult.NotApplicable;
        }

        public static XacmlDecisionResult PolicyFirstApplicable(IEnumerable<Tuple<IEnumerable<XacmlCombinerParameter>, IDictionary<string, Func<object>>>> results, IEnumerable<XacmlCombinerParameter> additionalParams) {
            foreach (Tuple<IEnumerable<XacmlCombinerParameter>, IDictionary<string, Func<object>>> resultDataFunc in results) {
                Tuple<XacmlDecisionResult, string> resultData = (Tuple<XacmlDecisionResult, string>)resultDataFunc.Item2["evaluate"]();

                XacmlDecisionResult result = resultData.Item1;

                if (result == XacmlDecisionResult.Deny) {
                    return XacmlDecisionResult.Deny;
                }

                if (result == XacmlDecisionResult.Permit) {
                    return XacmlDecisionResult.Permit;
                }

                if (result == XacmlDecisionResult.NotApplicable) {
                    continue;
                }

                if (result == XacmlDecisionResult.Indeterminate
                    || result == XacmlDecisionResult.IndeterminateD
                    || result == XacmlDecisionResult.IndeterminateP
                    || result == XacmlDecisionResult.IndeterminateDP) {
                    return XacmlDecisionResult.Indeterminate;
                }
            }

            return XacmlDecisionResult.NotApplicable;
        }

        public static XacmlDecisionResult PolicyOnlyOneApplicable(IEnumerable<Tuple<IEnumerable<XacmlCombinerParameter>, IDictionary<string, Func<object>>>> results, IEnumerable<XacmlCombinerParameter> additionalParams) {
            bool atLeastOne = false;
            Tuple<IEnumerable<XacmlCombinerParameter>, IDictionary<string, Func<object>>> selectedPolicy = null;

            foreach (Tuple<IEnumerable<XacmlCombinerParameter>, IDictionary<string, Func<object>>> resultDataFunc in results) {
                bool? isApplicable = (bool?)resultDataFunc.Item2["isApplicable"]();

                if (!isApplicable.HasValue) {
                    return XacmlDecisionResult.Indeterminate;
                }

                if (isApplicable.Value) {
                    if (atLeastOne) {
                        return XacmlDecisionResult.Indeterminate;
                    }
                    else {
                        atLeastOne = true;
                        selectedPolicy = resultDataFunc;
                    }
                }

                if (!isApplicable.Value) {
                    continue;
                }
            }

            if (atLeastOne) {
                return ((Tuple<XacmlDecisionResult, string>)selectedPolicy.Item2["evaluate"]()).Item1;
            }
            else {
                return XacmlDecisionResult.NotApplicable;
            }
        }

        public static XacmlDecisionResult DenyOvverides_30(IEnumerable<Tuple<IEnumerable<XacmlCombinerParameter>, IDictionary<string, Func<object>>>> results, IEnumerable<XacmlCombinerParameter> additionalParams) {
            Boolean atLeastOneErrorD = false;
            Boolean atLeastOneErrorP = false;
            Boolean atLeastOneErrorDP = false;
            Boolean atLeastOnePermit = false;

            foreach (Tuple<IEnumerable<XacmlCombinerParameter>, IDictionary<string, Func<object>>> elem in results) {
                Tuple<XacmlDecisionResult, string> decisionRes = (Tuple<XacmlDecisionResult, string>)elem.Item2["evaluate"]();
                XacmlDecisionResult decision = decisionRes.Item1;
                if (decision == XacmlDecisionResult.Deny) {
                    return XacmlDecisionResult.Deny;
                }

                if (decision == XacmlDecisionResult.Permit) {
                    atLeastOnePermit = true;
                    continue;
                }

                if (decision == XacmlDecisionResult.NotApplicable) {
                    continue;
                }

                if (decision == XacmlDecisionResult.IndeterminateD) {
                    atLeastOneErrorD = true;
                    continue;
                }

                if (decision == XacmlDecisionResult.IndeterminateP) {
                    atLeastOneErrorP = true;
                    continue;
                }

                if (decision == XacmlDecisionResult.IndeterminateDP) {
                    atLeastOneErrorDP = true;
                    continue;
                }
            }

            if (atLeastOneErrorDP) {
                return XacmlDecisionResult.IndeterminateDP;
            }

            if (atLeastOneErrorD && (atLeastOneErrorP || atLeastOnePermit)) {
                return XacmlDecisionResult.IndeterminateDP;
            }

            if (atLeastOneErrorD) {
                return XacmlDecisionResult.IndeterminateD;
            }

            if (atLeastOnePermit) {
                return XacmlDecisionResult.Permit;
            }

            if (atLeastOneErrorP) {
                return XacmlDecisionResult.IndeterminateP;
            }

            return XacmlDecisionResult.NotApplicable;
        }

        public static XacmlDecisionResult PermitOvverides_30(IEnumerable<Tuple<IEnumerable<XacmlCombinerParameter>, IDictionary<string, Func<object>>>> results, IEnumerable<XacmlCombinerParameter> additionalParams) {
            Boolean atLeastOneErrorD = false;
            Boolean atLeastOneErrorP = false;
            Boolean atLeastOneErrorDP = false;
            Boolean atLeastOneDeny = false;

            foreach (Tuple<IEnumerable<XacmlCombinerParameter>, IDictionary<string, Func<object>>> elem in results) {
                Tuple<XacmlDecisionResult, string> decisionRes = (Tuple<XacmlDecisionResult, string>)elem.Item2["evaluate"]();
                XacmlDecisionResult decision = decisionRes.Item1;
                if (decision == XacmlDecisionResult.Deny) {
                    atLeastOneDeny = true;
                    continue;
                }

                if (decision == XacmlDecisionResult.Permit) {
                    return XacmlDecisionResult.Permit;
                }

                if (decision == XacmlDecisionResult.NotApplicable) {
                    continue;
                }

                if (decision == XacmlDecisionResult.IndeterminateD) {
                    atLeastOneErrorD = true;
                    continue;
                }

                if (decision == XacmlDecisionResult.IndeterminateP) {
                    atLeastOneErrorP = true;
                    continue;
                }

                if (decision == XacmlDecisionResult.IndeterminateDP) {
                    atLeastOneErrorDP = true;
                    continue;
                }
            }

            if (atLeastOneErrorDP) {
                return XacmlDecisionResult.IndeterminateDP;
            }

            if (atLeastOneErrorP && (atLeastOneErrorD || atLeastOneDeny)) {
                return XacmlDecisionResult.IndeterminateDP;
            }

            if (atLeastOneErrorP) {
                return XacmlDecisionResult.IndeterminateP;
            }

            if (atLeastOneDeny) {
                return XacmlDecisionResult.Deny;
            }

            if (atLeastOneErrorD) {
                return XacmlDecisionResult.IndeterminateD;
            }

            return XacmlDecisionResult.NotApplicable;
        }

        public static XacmlDecisionResult DenyUnlessPermit_30(IEnumerable<Tuple<IEnumerable<XacmlCombinerParameter>, IDictionary<string, Func<object>>>> results, IEnumerable<XacmlCombinerParameter> additionalParams) {
            foreach (Tuple<IEnumerable<XacmlCombinerParameter>, IDictionary<string, Func<object>>> elem in results) {
                Tuple<XacmlDecisionResult, string> decisionRes = (Tuple<XacmlDecisionResult, string>)elem.Item2["evaluate"]();
                XacmlDecisionResult decision = decisionRes.Item1;

                if (decision == XacmlDecisionResult.Permit) {
                    return XacmlDecisionResult.Permit;
                }
            }

            return XacmlDecisionResult.Deny;
        }

        public static XacmlDecisionResult PermitUnlessDeny_30(IEnumerable<Tuple<IEnumerable<XacmlCombinerParameter>, IDictionary<string, Func<object>>>> results, IEnumerable<XacmlCombinerParameter> additionalParams) {
            foreach (Tuple<IEnumerable<XacmlCombinerParameter>, IDictionary<string, Func<object>>> elem in results) {
                Tuple<XacmlDecisionResult, string> decisionRes = (Tuple<XacmlDecisionResult, string>)elem.Item2["evaluate"]();
                XacmlDecisionResult decision = decisionRes.Item1;

                if (decision == XacmlDecisionResult.Deny) {
                    return XacmlDecisionResult.Deny;
                }
            }

            return XacmlDecisionResult.Permit;
        }

        internal static AlgorithmsProcessor Instance {
            get {
                if (processor == null) {
                    lock (locker) {
                        if (processor == null) {
                            processor = new AlgorithmsProcessor();

                            var catalog = new DirectoryCatalog(AppDomain.CurrentDomain.BaseDirectory, "Abc.Xacml.*.dll");
                            var container = new CompositionContainer(catalog);
                            var exportedTypes = container.GetExportedValues<IAlgorithmsExtender>();
                            foreach (var t in exportedTypes) {
                                IDictionary<string, AlgorithmsRunner> extensionTypes = t.GetExtensionAlgorithms();
                                foreach (var extensionType in extensionTypes) {
                                    AlgorithmsProcessor.algorithms.Add(extensionType.Key, extensionType.Value);
                                }
                            }
                        }
                    }
                }

                return processor;
            }
        }

        public AlgorithmsRunner this[string value] {
            get {
                AlgorithmsRunner action;
                if (algorithms.TryGetValue(value, out action)) {
                    return action;
                }

                throw Diagnostic.DiagnosticTools.ExceptionUtil.ThrowHelperArgumentNull("Unknows combining algorithm name");
            }
        }
    }
}
