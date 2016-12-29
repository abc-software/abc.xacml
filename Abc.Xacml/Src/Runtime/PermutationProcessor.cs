// ----------------------------------------------------------------------------
// <copyright file="PermutationProcessor.cs" company="ABC Software Ltd">
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
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Class for all permutation generation
    /// Call function on every permutation
    /// If function return true -> break
    /// </summary>
    public class PermutationProcessor {
        private readonly IPermutationNode root;

        /// <summary>
        /// Processors constructor
        /// </summary>
        /// <param name="values">All objects, every element - one permutation item</param>
        /// <param name="fun">To break -> return true</param>
        public PermutationProcessor(IEnumerable values, Func<IEnumerable<object>, bool> fun) {
            IEnumerable<object> reverseList = values.Cast<object>().Reverse();

            object[] generatedParams = new object[reverseList.Count()];

            IPermutationNode firstNode = new PermutationProcessorFinalNode(fun, generatedParams);

            int resultIndex = reverseList.Count() - 1;

            foreach (object val in values.Cast<object>().Reverse()) {
                if (val.GetType().IsArray || (val.GetType().IsGenericType && val.GetType().ReflectedType == typeof(Enumerable))) {
                    firstNode = new PermutationProcessorNode((IEnumerable)val, resultIndex, firstNode, generatedParams);
                }
                else {
                    firstNode = new PermutationProcessorOneElementNode(val, resultIndex, firstNode, generatedParams);
                }

                resultIndex--;
            }

            this.root = firstNode;
        }

        public void Run() {
            this.root.Next();
        }

        internal interface IPermutationNode {
            bool Next();
        }

        private class PermutationProcessorOneElementNode : IPermutationNode {
            private readonly IPermutationNode next;

            internal PermutationProcessorOneElementNode(object value, int resultIndex, IPermutationNode next, object[] generatedParams) {
                Contract.Requires(resultIndex >= 0);
                Contract.Requires(next != null);
                Contract.Requires(generatedParams != null);

                this.next = next;
                generatedParams[resultIndex] = value;
            }

            public bool Next() {
                bool breakFun = this.next.Next();
                if (breakFun) {
                    return true;
                }

                return false;
            }
        }

        private class PermutationProcessorNode : IPermutationNode {
            private readonly IEnumerable values;
            private readonly IPermutationNode next;
            private readonly int resultIndex;
            private readonly object[] generatedParams;

            internal PermutationProcessorNode(IEnumerable values, int resultIndex, IPermutationNode next, object[] generatedParams) {
                Contract.Requires(values != null);
                Contract.Requires(resultIndex >= 0);
                Contract.Requires(next != null);
                Contract.Requires(generatedParams != null);

                this.values = values;
                this.next = next;
                this.resultIndex = resultIndex;
                this.generatedParams = generatedParams;
            }

            public bool Next() {
                foreach (object val in values) {
                    this.generatedParams[this.resultIndex] = val;

                    bool breakFun = this.next.Next();
                    if (breakFun) {
                        return true;
                    }
                }

                return false;
            }
        }

        internal class PermutationProcessorFinalNode : IPermutationNode {
            private readonly Func<IEnumerable<object>, bool> functionEvaluator;
            private readonly object[] generatedParams;

            public PermutationProcessorFinalNode(Func<IEnumerable<object>, bool> fun, object[] generatedParams) {
                this.functionEvaluator = fun;
                this.generatedParams = generatedParams;
            }

            public bool Next() {
                return this.functionEvaluator(this.generatedParams);
            }
        }

    }
}
