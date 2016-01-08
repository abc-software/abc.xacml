// ----------------------------------------------------------------------------
// <copyright file="AttributeAssignmentsWrapper.cs" company="ABC Software Ltd">
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
    using Abc.Xacml.Policy;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Klasse, kas kontrole AttributeAssignment atbilstību Rule/Policy/PolicySet rezultātiem
    /// Klasse laiž cauri tikai tadus elementus, kuriem Effect type atbilst rezultātam
    /// Sanāk, ka līdz augstakām līmēnim nonaks tikai tadi elementi, kuriem Effect type atbilst gala rezultātam
    /// Veikt šo parbaudi tikai vienu reizi, beigās, nevar, jo starpposmi var nelaist cauri dažus elementus
    /// Piemēram, rezultāts Permit, bet iekļautības līmeņi Permit/Deny/Permit
    /// Klasse nenodrošina pašu elementu apstrādi, to ir javeic Evaluate metodēs, darbojoties ar klasses globālu mainīgu
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AttributeAssignmentsWrapper<T> : IDisposable where T : class {
        IDictionary<XacmlEffectType, List<T>> savedOriginalCollection;
        Ref<IDictionary<XacmlEffectType, List<T>>> reference;
        Func<XacmlDecisionResult> resultGetter;

        public AttributeAssignmentsWrapper(Func<IDictionary<XacmlEffectType, List<T>>> getter, Action<IDictionary<XacmlEffectType, List<T>>> setter, Func<XacmlDecisionResult> resultGetter) {
            reference = new Ref<IDictionary<XacmlEffectType, List<T>>>(getter, setter);
            this.savedOriginalCollection = reference.Value;

            reference.Value = new Dictionary<XacmlEffectType, List<T>>()
            {
                { XacmlEffectType.Permit, new List<T>() },
                { XacmlEffectType.Deny, new List<T>() }
            };

            this.resultGetter = resultGetter;
        }

        public void Dispose() {
            XacmlDecisionResult result = this.resultGetter();
            IDictionary<XacmlEffectType, List<T>> dict = this.reference.Value;

            if (result == XacmlDecisionResult.Permit && dict.ContainsKey(XacmlEffectType.Permit)) {
                if (!this.savedOriginalCollection.ContainsKey(XacmlEffectType.Permit)) {
                    this.savedOriginalCollection.Add(XacmlEffectType.Permit, new List<T>());
                }

                this.savedOriginalCollection[XacmlEffectType.Permit].AddRange(dict[XacmlEffectType.Permit]);
            }

            if (result == XacmlDecisionResult.Deny && dict.ContainsKey(XacmlEffectType.Deny)) {
                if (!this.savedOriginalCollection.ContainsKey(XacmlEffectType.Deny)) {
                    this.savedOriginalCollection.Add(XacmlEffectType.Deny, new List<T>());
                }

                this.savedOriginalCollection[XacmlEffectType.Deny].AddRange(dict[XacmlEffectType.Deny]);
            }

            this.reference.Value = savedOriginalCollection;
        }

        private class Ref<T> {
            private Func<T> getter;
            private Action<T> setter;
            public Ref(Func<T> getter, Action<T> setter) {
                this.getter = getter;
                this.setter = setter;
            }
            public T Value {
                get { return getter(); }
                set { setter(value); }
            }
        }
    }
}
