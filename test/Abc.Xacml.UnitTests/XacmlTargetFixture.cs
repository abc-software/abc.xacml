using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Abc.Xacml.Policy;
using NUnit.Framework;

namespace Abc.Xacml.UnitTests {
    [TestFixture]
    public class XacmlTargetFixture {
        [Test]
        public void Add() {
            var target = new XacmlTarget();

            var subject1 = new XacmlSubject(
                new XacmlSubjectMatch[] 
                {  
                    new XacmlSubjectMatch(
                        new Uri("http://subject1"),
                        new XacmlAttributeValue(new Uri("http://www.DataType.www")), 
                        new XacmlSubjectAttributeDesignator(new Uri("http://www.AttributeId.www"), new Uri("http://www.DataType.www")) { Issuer = "String", MustBePresent = false, Category = new Uri("http://www.subjectCategory.www") })
                });

            var subject2 = new XacmlSubject(
                new XacmlSubjectMatch[] 
                {  
                    new XacmlSubjectMatch(
                        new Uri("http://subject2"),
                        new XacmlAttributeValue(new Uri("http://www.DataType.www")), 
                        new XacmlSubjectAttributeDesignator(new Uri("http://www.AttributeId.www"), new Uri("http://www.DataType.www")) { Issuer = "String", MustBePresent = false, Category = new Uri("http://www.subjectCategory.www") })
                });

            var resource1 = new XacmlResource(
            new XacmlResourceMatch[] 
                { 
                    new XacmlResourceMatch(
                        new Uri("http://resource1"), 
                        new XacmlAttributeValue(new Uri("http://www.DataType.www") /*, "xxxx" */), 
                        new XacmlResourceAttributeDesignator(new Uri("http://www.AttributeId.www"), new Uri("http://www.DataType.www")) { Issuer = "String", MustBePresent = false} 
                        )
                });

            var resource2 = new XacmlResource(
            new XacmlResourceMatch[] 
                { 
                    new XacmlResourceMatch(
                        new Uri("http://resource1"), 
                        new XacmlAttributeValue(new Uri("http://www.DataType.www") /*, "xxxx" */), 
                        new XacmlResourceAttributeDesignator(new Uri("http://www.AttributeId.www"), new Uri("http://www.DataType.www")) { Issuer = "String", MustBePresent = false} 
                        )
                });

            {
                target.AnyOf.Add(new XacmlAnyOf(new XacmlAllOf[] { subject1 }));

                Assert.AreEqual(1, target.AnyOf.Count);
                Assert.AreEqual(1, target.AnyOf.SelectMany(x => x.AllOf).Count());
                Assert.AreEqual(1, target.Subjects.Count);
                Assert.AreEqual(0, target.Resources.Count);
                Assert.AreEqual(new Uri("http://subject1"), target.Subjects.First().Matches.First().MatchId);
            }

            {
                target.Subjects.Add(subject2);

                Assert.AreEqual(1, target.AnyOf.Count);
                Assert.AreEqual(2, target.AnyOf.SelectMany(x => x.AllOf).Count());
                Assert.AreEqual(2, target.Subjects.Count);
                Assert.AreEqual(0, target.Resources.Count);
            }

            {
                target.AnyOf.Add(new XacmlAnyOf(new XacmlAllOf[] { resource1 }));

                Assert.AreEqual(2, target.AnyOf.Count);
                Assert.AreEqual(3, target.AnyOf.SelectMany(x => x.AllOf).Count());
                Assert.AreEqual(2, target.Subjects.Count);
                Assert.AreEqual(1, target.Resources.Count);
            }

            {
                target.Resources.Add(resource2);

                Assert.AreEqual(2, target.AnyOf.Count);
                Assert.AreEqual(4, target.AnyOf.SelectMany(x => x.AllOf).Count());
                Assert.AreEqual(2, target.Subjects.Count);
                Assert.AreEqual(2, target.Resources.Count);

                foreach (var res in target.Resources) {
                }

                Assert.AreEqual(true, target.Resources.Contains(resource1));
            }

            {
                target.Subjects.Remove(subject1);

                Assert.AreEqual(2, target.AnyOf.Count);
                Assert.AreEqual(3, target.AnyOf.SelectMany(x => x.AllOf).Count());
                Assert.AreEqual(1, target.Subjects.Count);
                Assert.AreEqual(2, target.Resources.Count);
            }

            {
                target.Resources.Clear();

                Assert.AreEqual(1, target.AnyOf.Count);
                Assert.AreEqual(1, target.AnyOf.SelectMany(x => x.AllOf).Count());
                Assert.AreEqual(1, target.Subjects.Count);
                Assert.AreEqual(0, target.Resources.Count);
            }
        }
    }
}
