// ----------------------------------------------------------------------------
// <copyright file="XacmlTarget.cs" company="ABC Software Ltd">
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
    using System.Linq;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Identifies the set of decision requests that the parent element is intended to evaluate.
    /// </summary>
    /// <remarks>
    /// See the xacml:Target element defined in [XacmlCore2, 5.5][XacmlCore3, 5.6] for more details.
    /// Used only from XACML 1.0/1.1/2.0
    /// </remarks>
    public class XacmlTarget {
        private readonly ICollection<XacmlAnyOf> anyOf = new Collection<XacmlAnyOf>();

        /// <summary>
        /// Initializes a new instance of the <see cref="XacmlTarget"/> class.
        /// </summary>
        public XacmlTarget() {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XacmlTarget"/> class.
        /// </summary>
        /// <param name="subject">The subject.</param>
        /// <param name="resource">The resource.</param>
        /// <param name="action">The action.</param>
        /// <param name="environment">The environment.</param>
        /// <remarks>
        /// Used only from XACML 1.0/1.1/2.0
        /// </remarks>
        public XacmlTarget(XacmlSubject subject, XacmlResource resource, XacmlAction action)
            : this(subject != null ? new XacmlSubject[] { subject } : null, resource != null ? new XacmlResource[] { resource } : null, action != null ? new XacmlAction[] { action } : null, null) {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XacmlTarget"/> class.
        /// </summary>
        /// <param name="subjects">The subjects.</param>
        /// <param name="resources">The resources.</param>
        /// <param name="actions">The actions.</param>
        /// <param name="environment">The environments.</param>
        /// <remarks>
        /// Used only from XACML 1.0/1.1/2.0
        /// </remarks>
        public XacmlTarget(IEnumerable<XacmlSubject> subjects, IEnumerable<XacmlResource> resources, IEnumerable<XacmlAction> actions)
            : this(subjects, resources, actions, null) {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XacmlTarget" /> class.
        /// </summary>
        /// <param name="subject">The subject.</param>
        /// <param name="resource">The resource.</param>
        /// <param name="action">The action.</param>
        /// <param name="environment">The environment.</param>
        /// <remarks>
        /// Used only from XACML 2.0
        /// </remarks>
        public XacmlTarget(XacmlSubject subject, XacmlResource resource, XacmlAction action, XacmlEnvironment environment)
            : this(subject != null ? new XacmlSubject[] { subject } : null, resource != null ? new XacmlResource[] { resource } : null, action != null ? new XacmlAction[] { action } : null, environment != null ? new XacmlEnvironment[] { environment } : null) {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XacmlTarget"/> class.
        /// </summary>
        /// <param name="subjects">The subject.</param>
        /// <param name="resources">The resource.</param>
        /// <param name="actions">The action.</param>
        /// <param name="environments">The environment.</param>
        /// <remarks>
        /// Used only from XACML 2.0
        /// </remarks>
        public XacmlTarget(IEnumerable<XacmlSubject> subjects, IEnumerable<XacmlResource> resources, IEnumerable<XacmlAction> actions, IEnumerable<XacmlEnvironment> environments) {
            if (subjects != null) {
                this.anyOf.Add(new XacmlAnyOf(subjects));
            }

            if (resources != null) {
                this.anyOf.Add(new XacmlAnyOf(resources));
            }

            if (actions != null) {
                this.anyOf.Add(new XacmlAnyOf(actions));
            }

            if (environments != null) {
                this.anyOf.Add(new XacmlAnyOf(environments));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XacmlTarget"/> class.
        /// </summary>
        /// <param name="anyOf">Any of.</param>
        /// <remarks>
        /// Used only from XACML 3.0
        /// </remarks>
        public XacmlTarget(IEnumerable<XacmlAnyOf> anyOf) {
            if (anyOf != null) {
                foreach (var item in anyOf) {
                    this.anyOf.Add(item);
                }
            }
        }

        /// <summary>
        /// Gets a value Subjects.
        /// </summary>
        /// <remarks>
        /// Used only from XACML 1.0/1.1/2.0
        /// </remarks>
        public ICollection<XacmlSubject> Subjects {
            get {
                // return new ReadOnlyCollection<XacmlSubject>(this.anyOf.SelectMany(x => x.AllOf.OfType<XacmlSubject>()).ToList());
                return new LinkedCollection<XacmlSubject>(this.anyOf);
            }
        }

        /// <summary>
        /// Gets a value Resources.
        /// </summary>
        /// <remarks>
        /// Used only from XACML 1.0/1.1/2.0
        /// </remarks>
        public ICollection<XacmlResource> Resources {
            get {
                // return new ReadOnlyCollection<XacmlResource>(this.anyOf.SelectMany(x => x.AllOf.OfType<XacmlResource>()).ToList());
                return new LinkedCollection<XacmlResource>(this.anyOf);
            }
        }

        /// <summary>
        /// Gets a value Actions.
        /// </summary>
        /// <remarks>
        /// Used only from XACML 1.0/1.1/2.0
        /// </remarks>
        public ICollection<XacmlAction> Actions {
            get {
                // return new ReadOnlyCollection<XacmlAction>(this.anyOf.SelectMany(x => x.AllOf.OfType<XacmlAction>()).ToList());
                return new LinkedCollection<XacmlAction>(this.anyOf);
            }
        }

        /// <summary>
        /// Gets a value Environments.
        /// </summary>
        /// <remarks>
        /// Used only from XACML 2.0
        /// </remarks>
        public ICollection<XacmlEnvironment> Environments {
            get {
                // return new ReadOnlyCollection<XacmlEnvironment>(this.anyOf.SelectMany(x => x.AllOf.OfType<XacmlEnvironment>()).ToList());
                return new LinkedCollection<XacmlEnvironment>(this.anyOf);
            }
        }

        /// <summary>
        /// Gets a value Environments.
        /// </summary>
        /// <remarks>
        /// Used only from XACML 3.0
        /// </remarks>
        public ICollection<XacmlAnyOf> AnyOf {
            get {
                return this.anyOf;
            }
        }

        private class LinkedCollection<T> : ICollection<T> where T : XacmlAllOf {
            ICollection<XacmlAnyOf> anyOf;
            public LinkedCollection(ICollection<XacmlAnyOf> anyOf) {
                this.anyOf = anyOf;
            }

            public void Add(T item) {
                var anyOf = this.anyOf.FirstOrDefault(x => x.AllOf.OfType<T>().Any());
                if (anyOf != null) {
                    anyOf.AllOf.Add(item);
                }
                else {
                    this.anyOf.Add(new XacmlAnyOf(new XacmlAllOf[] { item }));
                }
            }

            public void Clear() {
                var anyOfList = this.anyOf.Where(x => x.AllOf.OfType<T>().Any()).ToList();
                foreach (var anyOf in anyOfList) {
                    this.anyOf.Remove(anyOf);
                }
            }

            public bool Contains(T item) {
                return this.anyOf.SelectMany(x => x.AllOf.OfType<T>()).Contains(item);
            }

            public void CopyTo(T[] array, int arrayIndex) {
                this.anyOf.SelectMany(x => x.AllOf.OfType<T>()).ToList().CopyTo(array, arrayIndex);
            }

            public int Count {
                get { return this.anyOf.SelectMany(x => x.AllOf.OfType<T>()).Count(); }
            }

            public bool IsReadOnly {
                get { return false; }
            }

            public bool Remove(T item) {
                var anyOfList = this.anyOf.Where(x => x.AllOf.OfType<T>().Any()).ToList();
                foreach (var anyOf in anyOfList) {
                    var b = anyOf.AllOf.Remove(item);
                    if (b) {
                        return true;
                    }
                }

                return false;
            }

            public IEnumerator<T> GetEnumerator() {
                return this.anyOf.SelectMany(x => x.AllOf.OfType<T>()).GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
                return this.anyOf.SelectMany(x => x.AllOf.OfType<T>()).GetEnumerator();
            }
        }
    }
}