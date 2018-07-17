// ----------------------------------------------------------------------------
// <copyright file="FunctionsProcessor.cs" company="ABC Software Ltd">
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
    using Abc.Xacml.DataTypes;
    using Abc.Xacml.Interfaces;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
#if NET40 || NET45
    using System.ComponentModel.Composition.Hosting;
#endif
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Xml;
    using System.Xml.Linq;

    public sealed class FunctionsProcessor {
        private static FunctionsProcessor processor = null;
        private static object locker = new object();

        public delegate IEnumerable<T> ParamsToEnumerable<T>(params T[] values); // TODO: JG
        delegate T EnumerableToGenericParam<T>(params T[] values);
        delegate R EnumerableToGenericParamWithOneArg<in T1, in T2, out R>(T2 arg, params T1[] values);
        delegate R EnumerableToGenericParamWithTwoArg<in T1, in T2, in T3, out R>(T2 arg1, T3 arg2, params T1[] values);

        private static SortedDictionary<string, DelegateWrapper> functions = new SortedDictionary<string, DelegateWrapper>()
        {
            { "urn:oasis:names:tc:xacml:1.0:function:string-equal", new DelegateWrapper(typeof(Func<string, string, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("EqualFunc", typeof(string), o => new Type[]{ o, o })) },
            { "urn:oasis:names:tc:xacml:1.0:function:boolean-equal", new DelegateWrapper(typeof(Func<bool, bool, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("EqualFunc", typeof(bool), o => new Type[]{ o, o })) },
            { "urn:oasis:names:tc:xacml:1.0:function:integer-equal", new DelegateWrapper(typeof(Func<int, int, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("EqualFunc", typeof(int), o => new Type[]{ o, o })) },
            { "urn:oasis:names:tc:xacml:1.0:function:double-equal", new DelegateWrapper(typeof(Func<double, double, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("EqualFunc", typeof(double), o => new Type[]{ o, o })) },
            { "urn:oasis:names:tc:xacml:1.0:function:date-equal", new DelegateWrapper(typeof(Func<Date, Date, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("EqualFunc", typeof(Date), o => new Type[]{ o, o })) },
            { "urn:oasis:names:tc:xacml:1.0:function:time-equal", new DelegateWrapper(typeof(Func<Time, Time, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("EqualFunc", typeof(Time), o => new Type[]{ o, o })) },
            { "urn:oasis:names:tc:xacml:1.0:function:dateTime-equal", new DelegateWrapper(typeof(Func<DateTime, DateTime, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("EqualFunc", typeof(DateTime), o => new Type[]{ o, o })) },
            { "urn:oasis:names:tc:xacml:1.0:function:dayTimeDuration-equal", new DelegateWrapper(typeof(Func<DayTimeDuration, DayTimeDuration, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("EqualFunc", typeof(DayTimeDuration), o => new Type[]{ o, o })) },
            { "urn:oasis:names:tc:xacml:1.0:function:yearMonthDuration-equal", new DelegateWrapper(typeof(Func<YearMonthDuration, YearMonthDuration, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("EqualFunc", typeof(YearMonthDuration), o => new Type[]{ o, o })) },
            { "urn:oasis:names:tc:xacml:1.0:function:anyURI-equal", new DelegateWrapper(typeof(Func<Uri, Uri, bool>), typeof(FunctionsProcessor).GetMethod("EqualFunc", new Type[] { typeof(Uri), typeof(Uri) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:x500Name-equal", new DelegateWrapper(typeof(Func<x500Name, x500Name, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("EqualFunc", typeof(x500Name), o => new Type[]{ o, o })) },
            { "urn:oasis:names:tc:xacml:1.0:function:rfc822Name-equal", new DelegateWrapper(typeof(Func<Rfc822Name, Rfc822Name, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("EqualFunc", typeof(Rfc822Name), o => new Type[]{ o, o })) },
            { "urn:oasis:names:tc:xacml:1.0:function:hexBinary-equal", new DelegateWrapper(typeof(Func<HexBinary, HexBinary, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("EqualFunc", typeof(HexBinary), o => new Type[]{ o, o })) },
            { "urn:oasis:names:tc:xacml:1.0:function:base64Binary-equal", new DelegateWrapper(typeof(Func<Base64Binary, Base64Binary, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("EqualFunc", typeof(Base64Binary), o => new Type[]{ o, o })) },
            { "urn:oasis:names:tc:xacml:3.0:function:dayTimeDuration-equal", new DelegateWrapper(typeof(Func<DayTimeDuration, DayTimeDuration, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("EqualFunc", typeof(DayTimeDuration), o => new Type[]{ o, o })) },
            { "urn:oasis:names:tc:xacml:3.0:function:yearMonthDuration-equal", new DelegateWrapper(typeof(Func<YearMonthDuration, YearMonthDuration, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("EqualFunc", typeof(YearMonthDuration), o => new Type[]{ o, o })) },
            { "urn:oasis:names:tc:xacml:3.0:function:string-equal-ignore-case", new DelegateWrapper(typeof(Func<string, string, bool>), typeof(FunctionsProcessor).GetMethod("StringEqualIgnoreCaseFunc", BindingFlags.Static | BindingFlags.Public)) },

            // Bag function
            { "urn:oasis:names:tc:xacml:1.0:function:string-one-and-only", new DelegateWrapper(typeof(Func<IEnumerable<string>, string>), typeof(FunctionsProcessor).GetGenericMethodInfo("OneAndOnly", typeof(string), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:boolean-one-and-only", new DelegateWrapper(typeof(Func<IEnumerable<bool>, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("OneAndOnly", typeof(bool), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:integer-one-and-only", new DelegateWrapper(typeof(Func<IEnumerable<int>, int>), typeof(FunctionsProcessor).GetGenericMethodInfo("OneAndOnly", typeof(int), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:double-one-and-only", new DelegateWrapper(typeof(Func<IEnumerable<double>, double>), typeof(FunctionsProcessor).GetGenericMethodInfo("OneAndOnly", typeof(double), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:time-one-and-only", new DelegateWrapper(typeof(Func<IEnumerable<Time>, Time>), typeof(FunctionsProcessor).GetGenericMethodInfo("OneAndOnly", typeof(Time), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:date-one-and-only", new DelegateWrapper(typeof(Func<IEnumerable<Date>, Date>), typeof(FunctionsProcessor).GetGenericMethodInfo("OneAndOnly", typeof(Date), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:dateTime-one-and-only", new DelegateWrapper(typeof(Func<IEnumerable<DateTime>, DateTime>), typeof(FunctionsProcessor).GetGenericMethodInfo("OneAndOnly", typeof(DateTime), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:anyURI-one-and-only", new DelegateWrapper(typeof(Func<IEnumerable<Uri>, Uri>), typeof(FunctionsProcessor).GetGenericMethodInfo("OneAndOnly", typeof(Uri), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:hexBinary-one-and-only", new DelegateWrapper(typeof(Func<IEnumerable<HexBinary>, HexBinary>), typeof(FunctionsProcessor).GetGenericMethodInfo("OneAndOnly", typeof(HexBinary), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:base64Binary-one-and-only", new DelegateWrapper(typeof(Func<IEnumerable<Base64Binary>, Base64Binary>), typeof(FunctionsProcessor).GetGenericMethodInfo("OneAndOnly", typeof(Base64Binary), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:dayTimeDuration-one-and-only", new DelegateWrapper(typeof(Func<IEnumerable<DayTimeDuration>, DayTimeDuration>), typeof(FunctionsProcessor).GetGenericMethodInfo("OneAndOnly", typeof(DayTimeDuration), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:yearMonthDuration-one-and-only", new DelegateWrapper(typeof(Func<IEnumerable<YearMonthDuration>, YearMonthDuration>), typeof(FunctionsProcessor).GetGenericMethodInfo("OneAndOnly", typeof(YearMonthDuration), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:x500Name-one-and-only", new DelegateWrapper(typeof(Func<IEnumerable<x500Name>, x500Name>), typeof(FunctionsProcessor).GetGenericMethodInfo("OneAndOnly", typeof(x500Name), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:rfc822Name-one-and-only", new DelegateWrapper(typeof(Func<IEnumerable<Rfc822Name>, Rfc822Name>), typeof(FunctionsProcessor).GetGenericMethodInfo("OneAndOnly", typeof(Rfc822Name), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:3.0:function:dayTimeDuration-one-and-only", new DelegateWrapper(typeof(Func<IEnumerable<DayTimeDuration>, DayTimeDuration>), typeof(FunctionsProcessor).GetGenericMethodInfo("OneAndOnly", typeof(DayTimeDuration), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:3.0:function:yearMonthDuration-one-and-only", new DelegateWrapper(typeof(Func<IEnumerable<YearMonthDuration>, YearMonthDuration>), typeof(FunctionsProcessor).GetGenericMethodInfo("OneAndOnly", typeof(YearMonthDuration), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            
            { "urn:oasis:names:tc:xacml:1.0:function:string-bag-size", new DelegateWrapper(typeof(Func<IEnumerable<string>, int>), typeof(FunctionsProcessor).GetGenericMethodInfo("BagSize", typeof(string), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:boolean-bag-size", new DelegateWrapper(typeof(Func<IEnumerable<bool>, int>), typeof(FunctionsProcessor).GetGenericMethodInfo("BagSize", typeof(bool), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:integer-bag-size", new DelegateWrapper(typeof(Func<IEnumerable<int>, int>), typeof(FunctionsProcessor).GetGenericMethodInfo("BagSize", typeof(int), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:double-bag-size", new DelegateWrapper(typeof(Func<IEnumerable<double>, int>), typeof(FunctionsProcessor).GetGenericMethodInfo("BagSize", typeof(double), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:time-bag-size", new DelegateWrapper(typeof(Func<IEnumerable<Time>, int>), typeof(FunctionsProcessor).GetGenericMethodInfo("BagSize", typeof(Time), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:date-bag-size", new DelegateWrapper(typeof(Func<IEnumerable<Date>, int>), typeof(FunctionsProcessor).GetGenericMethodInfo("BagSize", typeof(Date), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:dateTime-bag-size", new DelegateWrapper(typeof(Func<IEnumerable<DateTime>, int>), typeof(FunctionsProcessor).GetGenericMethodInfo("BagSize", typeof(DateTime), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:anyURI-bag-size", new DelegateWrapper(typeof(Func<IEnumerable<Uri>, int>), typeof(FunctionsProcessor).GetGenericMethodInfo("BagSize", typeof(Uri), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:hexBinary-bag-size", new DelegateWrapper(typeof(Func<IEnumerable<HexBinary>, int>), typeof(FunctionsProcessor).GetGenericMethodInfo("BagSize", typeof(HexBinary), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:base64Binary-bag-size", new DelegateWrapper(typeof(Func<IEnumerable<Base64Binary>, int>), typeof(FunctionsProcessor).GetGenericMethodInfo("BagSize", typeof(Base64Binary), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:dayTimeDuration-bag-size", new DelegateWrapper(typeof(Func<IEnumerable<DayTimeDuration>, int>), typeof(FunctionsProcessor).GetGenericMethodInfo("BagSize", typeof(DayTimeDuration), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:yearMonthDuration-bag-size", new DelegateWrapper(typeof(Func<IEnumerable<YearMonthDuration>, int>), typeof(FunctionsProcessor).GetGenericMethodInfo("BagSize", typeof(YearMonthDuration), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:x500Name-bag-size", new DelegateWrapper(typeof(Func<IEnumerable<x500Name>, int>), typeof(FunctionsProcessor).GetGenericMethodInfo("BagSize", typeof(x500Name), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:rfc822Name-bag-size", new DelegateWrapper(typeof(Func<IEnumerable<Rfc822Name>, int>), typeof(FunctionsProcessor).GetGenericMethodInfo("BagSize", typeof(Rfc822Name), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:3.0:function:dayTimeDuration-bag-size", new DelegateWrapper(typeof(Func<IEnumerable<DayTimeDuration>, int>), typeof(FunctionsProcessor).GetGenericMethodInfo("BagSize", typeof(DayTimeDuration), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:3.0:function:yearMonthDuration-bag-size", new DelegateWrapper(typeof(Func<IEnumerable<YearMonthDuration>, int>), typeof(FunctionsProcessor).GetGenericMethodInfo("BagSize", typeof(YearMonthDuration), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },

            { "urn:oasis:names:tc:xacml:1.0:function:string-is-in", new DelegateWrapper(typeof(Func<string, IEnumerable<string>, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeIsIn", typeof(string), o => new Type[]{ o, typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:boolean-is-in", new DelegateWrapper(typeof(Func<bool, IEnumerable<bool>, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeIsIn", typeof(bool), o => new Type[]{ o, typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:integer-is-in", new DelegateWrapper(typeof(Func<int, IEnumerable<int>, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeIsIn", typeof(int), o => new Type[]{ o, typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:double-is-in", new DelegateWrapper(typeof(Func<double, IEnumerable<double>, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeIsIn", typeof(double), o => new Type[]{ o, typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:time-is-in", new DelegateWrapper(typeof(Func<Time, IEnumerable<Time>, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeIsIn", typeof(Time), o => new Type[]{ o, typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:date-is-in", new DelegateWrapper(typeof(Func<Date, IEnumerable<Date>, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeIsIn", typeof(Date), o => new Type[]{ o, typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:dateTime-is-in", new DelegateWrapper(typeof(Func<DateTime, IEnumerable<DateTime>, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeIsIn", typeof(DateTime), o => new Type[]{ o, typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:anyURI-is-in", new DelegateWrapper(typeof(Func<Uri, IEnumerable<Uri>, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeIsIn", typeof(Uri), o => new Type[]{ o, typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:hexBinary-is-in", new DelegateWrapper(typeof(Func<HexBinary, IEnumerable<HexBinary>, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeIsIn", typeof(HexBinary), o => new Type[]{ o, typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:base64Binary-is-in", new DelegateWrapper(typeof(Func<Base64Binary, IEnumerable<Base64Binary>, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeIsIn", typeof(Base64Binary), o => new Type[]{ o, typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:dayTimeDuration-is-in", new DelegateWrapper(typeof(Func<DayTimeDuration, IEnumerable<DayTimeDuration>, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeIsIn", typeof(DayTimeDuration), o => new Type[]{ o, typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:yearMonthDuration-is-in", new DelegateWrapper(typeof(Func<YearMonthDuration, IEnumerable<YearMonthDuration>, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeIsIn", typeof(YearMonthDuration), o => new Type[]{ o, typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:x500Name-is-in", new DelegateWrapper(typeof(Func<x500Name, IEnumerable<x500Name>, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeIsIn", typeof(x500Name), o => new Type[]{ o, typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:rfc822Name-is-in", new DelegateWrapper(typeof(Func<Rfc822Name, IEnumerable<Rfc822Name>, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeIsIn", typeof(Rfc822Name), o => new Type[]{ o, typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:3.0:function:dayTimeDuration-is-in", new DelegateWrapper(typeof(Func<DayTimeDuration, IEnumerable<DayTimeDuration>, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeIsIn", typeof(DayTimeDuration), o => new Type[]{ o, typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:3.0:function:yearMonthDuration-is-in", new DelegateWrapper(typeof(Func<YearMonthDuration, IEnumerable<YearMonthDuration>, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeIsIn", typeof(YearMonthDuration), o => new Type[]{ o, typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            
            { "urn:oasis:names:tc:xacml:1.0:function:string-bag", new DelegateWrapper(typeof(ParamsToEnumerable<string>), typeof(FunctionsProcessor).GetGenericMethodInfo("CreateBag", typeof(string), o => new Type[]{ o.MakeArrayType() })) },
            { "urn:oasis:names:tc:xacml:1.0:function:boolean-bag", new DelegateWrapper(typeof(ParamsToEnumerable<bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("CreateBag", typeof(bool), o => new Type[]{ o.MakeArrayType() })) },
            { "urn:oasis:names:tc:xacml:1.0:function:integer-bag", new DelegateWrapper(typeof(ParamsToEnumerable<int>), typeof(FunctionsProcessor).GetGenericMethodInfo("CreateBag", typeof(int), o => new Type[]{ o.MakeArrayType() })) },
            { "urn:oasis:names:tc:xacml:1.0:function:double-bag", new DelegateWrapper(typeof(ParamsToEnumerable<double>), typeof(FunctionsProcessor).GetGenericMethodInfo("CreateBag", typeof(double), o => new Type[]{ o.MakeArrayType() })) },
            { "urn:oasis:names:tc:xacml:1.0:function:time-bag", new DelegateWrapper(typeof(ParamsToEnumerable<Time>), typeof(FunctionsProcessor).GetGenericMethodInfo("CreateBag", typeof(Time), o => new Type[]{ o.MakeArrayType() })) },
            { "urn:oasis:names:tc:xacml:1.0:function:date-bag", new DelegateWrapper(typeof(ParamsToEnumerable<Date>), typeof(FunctionsProcessor).GetGenericMethodInfo("CreateBag", typeof(Date), o => new Type[]{ o.MakeArrayType() })) },
            { "urn:oasis:names:tc:xacml:1.0:function:dateTime-bag", new DelegateWrapper(typeof(ParamsToEnumerable<DateTime>), typeof(FunctionsProcessor).GetGenericMethodInfo("CreateBag", typeof(DateTime), o => new Type[]{ o.MakeArrayType() })) },
            { "urn:oasis:names:tc:xacml:1.0:function:anyURI-bag", new DelegateWrapper(typeof(ParamsToEnumerable<Uri>), typeof(FunctionsProcessor).GetGenericMethodInfo("CreateBag", typeof(Uri), o => new Type[]{ o.MakeArrayType() })) },
            { "urn:oasis:names:tc:xacml:1.0:function:hexBinary-bag", new DelegateWrapper(typeof(ParamsToEnumerable<HexBinary>), typeof(FunctionsProcessor).GetGenericMethodInfo("CreateBag", typeof(HexBinary), o => new Type[]{ o.MakeArrayType() })) },
            { "urn:oasis:names:tc:xacml:1.0:function:base64Binary-bag", new DelegateWrapper(typeof(ParamsToEnumerable<Base64Binary>), typeof(FunctionsProcessor).GetGenericMethodInfo("CreateBag", typeof(Base64Binary), o => new Type[]{ o.MakeArrayType() })) },
            { "urn:oasis:names:tc:xacml:1.0:function:dayTimeDuration-bag", new DelegateWrapper(typeof(ParamsToEnumerable<DayTimeDuration>), typeof(FunctionsProcessor).GetGenericMethodInfo("CreateBag", typeof(DayTimeDuration), o => new Type[]{ o.MakeArrayType() })) },
            { "urn:oasis:names:tc:xacml:1.0:function:yearMonthDuration-bag", new DelegateWrapper(typeof(ParamsToEnumerable<YearMonthDuration>), typeof(FunctionsProcessor).GetGenericMethodInfo("CreateBag", typeof(YearMonthDuration), o => new Type[]{ o.MakeArrayType() })) },
            { "urn:oasis:names:tc:xacml:1.0:function:x500Name-bag", new DelegateWrapper(typeof(ParamsToEnumerable<x500Name>), typeof(FunctionsProcessor).GetGenericMethodInfo("CreateBag", typeof(x500Name), o => new Type[]{ o.MakeArrayType() })) },
            { "urn:oasis:names:tc:xacml:1.0:function:rfc822Name-bag", new DelegateWrapper(typeof(ParamsToEnumerable<Rfc822Name>), typeof(FunctionsProcessor).GetGenericMethodInfo("CreateBag", typeof(Rfc822Name), o => new Type[]{ o.MakeArrayType() })) },
            { "urn:oasis:names:tc:xacml:3.0:function:dayTimeDuration-bag", new DelegateWrapper(typeof(ParamsToEnumerable<DayTimeDuration>), typeof(FunctionsProcessor).GetGenericMethodInfo("CreateBag", typeof(DayTimeDuration), o => new Type[]{ o.MakeArrayType() })) },
            { "urn:oasis:names:tc:xacml:3.0:function:yearMonthDuration-bag", new DelegateWrapper(typeof(ParamsToEnumerable<YearMonthDuration>), typeof(FunctionsProcessor).GetGenericMethodInfo("CreateBag", typeof(YearMonthDuration), o => new Type[]{ o.MakeArrayType() })) },
            
            // Higher-order bag functions
            { "urn:oasis:names:tc:xacml:1.0:function:any-of", new DelegateWithXPathContextWrapper(typeof(Func<XPathContext, DelegateWrapper, object, IEnumerable<object>, bool>), typeof(FunctionsProcessor).GetMethod("AnyOf", BindingFlags.Static | BindingFlags.Public)) },
            { "urn:oasis:names:tc:xacml:1.0:function:all-of", new DelegateWithXPathContextWrapper(typeof(Func<XPathContext, DelegateWrapper, object, IEnumerable<object>, bool>), typeof(FunctionsProcessor).GetMethod("AllOf", BindingFlags.Static | BindingFlags.Public)) },
            { "urn:oasis:names:tc:xacml:1.0:function:any-of-any", new DelegateWithXPathContextWrapper(typeof(Func<XPathContext, DelegateWrapper, IEnumerable<object>, IEnumerable<object>, bool>), typeof(FunctionsProcessor).GetMethod("AnyOfAny", BindingFlags.Static | BindingFlags.Public)) },
            { "urn:oasis:names:tc:xacml:1.0:function:all-of-any", new DelegateWithXPathContextWrapper(typeof(Func<XPathContext, DelegateWrapper, IEnumerable<object>, IEnumerable<object>, bool>), typeof(FunctionsProcessor).GetMethod("AllOfAny", BindingFlags.Static | BindingFlags.Public)) },
            { "urn:oasis:names:tc:xacml:1.0:function:any-of-all", new DelegateWithXPathContextWrapper(typeof(Func<XPathContext, DelegateWrapper, IEnumerable<object>, IEnumerable<object>, bool>), typeof(FunctionsProcessor).GetMethod("AnyOfAll", BindingFlags.Static | BindingFlags.Public)) },
            { "urn:oasis:names:tc:xacml:1.0:function:all-of-all", new DelegateWithXPathContextWrapper(typeof(Func<XPathContext, DelegateWrapper, IEnumerable<object>, IEnumerable<object>, bool>), typeof(FunctionsProcessor).GetMethod("AllOfAll", BindingFlags.Static | BindingFlags.Public)) },
            { "urn:oasis:names:tc:xacml:1.0:function:map", new DelegateWithXPathContextWrapper(typeof(Func<XPathContext, DelegateWrapper, IEnumerable<object>, IEnumerable<object>>), typeof(FunctionsProcessor).GetMethod("Map", BindingFlags.Static | BindingFlags.Public)) },

            { "urn:oasis:names:tc:xacml:3.0:function:any-of", new DelegateWithXPathContextWrapper(typeof(EnumerableToGenericParamWithTwoArg<object, XPathContext, DelegateWrapper, bool>), typeof(FunctionsProcessor).GetMethod("AnyOf_30", BindingFlags.Static | BindingFlags.Public)) },
            { "urn:oasis:names:tc:xacml:3.0:function:all-of", new DelegateWithXPathContextWrapper(typeof(EnumerableToGenericParamWithTwoArg<object, XPathContext, DelegateWrapper, bool>), typeof(FunctionsProcessor).GetMethod("AllOf_30", BindingFlags.Static | BindingFlags.Public)) },
            { "urn:oasis:names:tc:xacml:3.0:function:any-of-any", new DelegateWithXPathContextWrapper(typeof(EnumerableToGenericParamWithTwoArg<object, XPathContext, DelegateWrapper, bool>), typeof(FunctionsProcessor).GetMethod("AnyOfAny_30", BindingFlags.Static | BindingFlags.Public)) },
            { "urn:oasis:names:tc:xacml:3.0:function:map", new DelegateWithXPathContextWrapper(typeof(EnumerableToGenericParamWithTwoArg<object, XPathContext,  DelegateWrapper, IEnumerable<object>>), typeof(FunctionsProcessor).GetMethod("Map_30", BindingFlags.Static | BindingFlags.Public)) },

            // Regular expression based function
            { "urn:oasis:names:tc:xacml:1.0:function:regexp-string-match", new DelegateWrapper(typeof(Func<string, string, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("RegexpMatch", typeof(string), o => new Type[]{ typeof(string), o })) },
            { "urn:oasis:names:tc:xacml:1.0:function:string-regexp-match", new DelegateWrapper(typeof(Func<string, string, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("RegexpMatch", typeof(string), o => new Type[]{ typeof(string), o })) },
            { "urn:oasis:names:tc:xacml:2.0:function:anyURI-regexp-match", new DelegateWrapper(typeof(Func<string, Uri, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("RegexpMatch", typeof(Uri), o => new Type[]{ typeof(string), o })) },
            { "urn:oasis:names:tc:xacml:2.0:function:ipAddress-regexp-match", new DelegateWrapper(typeof(Func<string, IPAddress, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("RegexpMatch", typeof(IPAddress), o => new Type[]{ typeof(string), o })) },
            { "urn:oasis:names:tc:xacml:2.0:function:dnsName-regexp-match", new DelegateWrapper(typeof(Func<string, DNSName, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("RegexpMatch", typeof(DNSName), o => new Type[]{ typeof(string), o })) },
            { "urn:oasis:names:tc:xacml:2.0:function:rfc822Name-regexp-match", new DelegateWrapper(typeof(Func<string, Rfc822Name, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("RegexpMatch", typeof(Rfc822Name), o => new Type[]{ typeof(string), o })) },
            { "urn:oasis:names:tc:xacml:2.0:function:x500Name-regexp-match", new DelegateWrapper(typeof(Func<string, x500Name, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("RegexpMatch", typeof(x500Name), o => new Type[]{ typeof(string), o })) },
        
            // Arithmetic functions
            { "urn:oasis:names:tc:xacml:1.0:function:integer-add", new DelegateWrapper(typeof(EnumerableToGenericParam<int>), typeof(FunctionsProcessor).GetMethod("IntegerAdd", BindingFlags.Static | BindingFlags.Public)) },
            { "urn:oasis:names:tc:xacml:1.0:function:double-add", new DelegateWrapper(typeof(EnumerableToGenericParam<double>), typeof(FunctionsProcessor).GetMethod("DoubleAdd", BindingFlags.Static | BindingFlags.Public)) },
            { "urn:oasis:names:tc:xacml:1.0:function:integer-subtract", new DelegateWrapper(typeof(Func<int, int, int>), typeof(FunctionsProcessor).GetMethod("IntegerSubtract", BindingFlags.Static | BindingFlags.Public)) },
            { "urn:oasis:names:tc:xacml:1.0:function:double-subtract", new DelegateWrapper(typeof(Func<double, double, double>), typeof(FunctionsProcessor).GetMethod("DoubleSubtract", BindingFlags.Static | BindingFlags.Public)) },
            { "urn:oasis:names:tc:xacml:1.0:function:integer-multiply", new DelegateWrapper(typeof(Func<int, int, int>), typeof(FunctionsProcessor).GetMethod("IntegerMultiply", BindingFlags.Static | BindingFlags.Public)) },
            { "urn:oasis:names:tc:xacml:1.0:function:double-multiply", new DelegateWrapper(typeof(Func<double, double, double>), typeof(FunctionsProcessor).GetMethod("DoubleMultiply", BindingFlags.Static | BindingFlags.Public)) },
            { "urn:oasis:names:tc:xacml:1.0:function:integer-divide", new DelegateWrapper(typeof(Func<int, int, int?>), typeof(FunctionsProcessor).GetMethod("IntegerDivide", BindingFlags.Static | BindingFlags.Public)) },
            { "urn:oasis:names:tc:xacml:1.0:function:double-divide", new DelegateWrapper(typeof(Func<double, double, double?>), typeof(FunctionsProcessor).GetMethod("DoubleDivide", BindingFlags.Static | BindingFlags.Public)) },
            { "urn:oasis:names:tc:xacml:1.0:function:integer-mod", new DelegateWrapper(typeof(Func<int, int, int?>), typeof(FunctionsProcessor).GetMethod("IntegerMod", BindingFlags.Static | BindingFlags.Public)) },
            { "urn:oasis:names:tc:xacml:1.0:function:integer-abs", new DelegateWrapper(typeof(Func<int, int>), typeof(FunctionsProcessor).GetMethod("IntegerAbs", BindingFlags.Static | BindingFlags.Public)) },
            { "urn:oasis:names:tc:xacml:1.0:function:double-abs", new DelegateWrapper(typeof(Func<double, double>), typeof(FunctionsProcessor).GetMethod("DoubleAbs", BindingFlags.Static | BindingFlags.Public)) },
            { "urn:oasis:names:tc:xacml:1.0:function:round", new DelegateWrapper(typeof(Func<double, double>), typeof(FunctionsProcessor).GetMethod("Round", BindingFlags.Static | BindingFlags.Public)) },
            { "urn:oasis:names:tc:xacml:1.0:function:floor", new DelegateWrapper(typeof(Func<double, double>), typeof(FunctionsProcessor).GetMethod("Floor", BindingFlags.Static | BindingFlags.Public)) },        
        
            // Conversion function
            { "urn:oasis:names:tc:xacml:3.0:function:boolean-from-string", new DelegateWrapper(typeof(Func<string, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("ConvertFromString", typeof(bool), o => new Type[]{ typeof(string) })) },
            { "urn:oasis:names:tc:xacml:3.0:function:integer-from-string", new DelegateWrapper(typeof(Func<string, int>), typeof(FunctionsProcessor).GetGenericMethodInfo("ConvertFromString", typeof(int), o => new Type[]{ typeof(string) })) },
            { "urn:oasis:names:tc:xacml:3.0:function:double-from-string", new DelegateWrapper(typeof(Func<string, double>), typeof(FunctionsProcessor).GetGenericMethodInfo("ConvertFromString", typeof(double), o => new Type[]{ typeof(string) })) },
            { "urn:oasis:names:tc:xacml:3.0:function:time-from-string", new DelegateWrapper(typeof(Func<string, Time>), typeof(FunctionsProcessor).GetGenericMethodInfo("ConvertFromString", typeof(Time), o => new Type[]{ typeof(string) })) },
            { "urn:oasis:names:tc:xacml:3.0:function:date-from-string", new DelegateWrapper(typeof(Func<string, Date>), typeof(FunctionsProcessor).GetGenericMethodInfo("ConvertFromString", typeof(Date), o => new Type[]{ typeof(string) })) },
            { "urn:oasis:names:tc:xacml:3.0:function:dateTime-from-string", new DelegateWrapper(typeof(Func<string, DateTime>), typeof(FunctionsProcessor).GetGenericMethodInfo("ConvertFromString", typeof(DateTime), o => new Type[]{ typeof(string) })) },
            { "urn:oasis:names:tc:xacml:3.0:function:anyURI-from-string", new DelegateWrapper(typeof(Func<string, Uri>), typeof(FunctionsProcessor).GetMethod("UriConvertFromString", BindingFlags.Static | BindingFlags.Public)) },
            { "urn:oasis:names:tc:xacml:3.0:function:dayTimeDuration-from-string", new DelegateWrapper(typeof(Func<string, DayTimeDuration>), typeof(FunctionsProcessor).GetGenericMethodInfo("ConvertFromString", typeof(DayTimeDuration), o => new Type[]{ typeof(string) })) },
            { "urn:oasis:names:tc:xacml:3.0:function:yearMonthDuration-from-string", new DelegateWrapper(typeof(Func<string, YearMonthDuration>), typeof(FunctionsProcessor).GetGenericMethodInfo("ConvertFromString", typeof(YearMonthDuration), o => new Type[]{ typeof(string) })) },
            { "urn:oasis:names:tc:xacml:3.0:function:x500Name-from-string", new DelegateWrapper(typeof(Func<string, x500Name>), typeof(FunctionsProcessor).GetGenericMethodInfo("ConvertFromString", typeof(x500Name), o => new Type[]{ typeof(string) })) },
            { "urn:oasis:names:tc:xacml:3.0:function:rfc822Name-from-string", new DelegateWrapper(typeof(Func<string, Rfc822Name>), typeof(FunctionsProcessor).GetGenericMethodInfo("ConvertFromString", typeof(Rfc822Name), o => new Type[]{ typeof(string) })) },
            { "urn:oasis:names:tc:xacml:3.0:function:ipAddress-from-string", new DelegateWrapper(typeof(Func<string, IPAddress>), typeof(FunctionsProcessor).GetGenericMethodInfo("ConvertFromString", typeof(IPAddress), o => new Type[]{ typeof(string) })) },
            { "urn:oasis:names:tc:xacml:3.0:function:dnsName-from-string", new DelegateWrapper(typeof(Func<string, DNSName>), typeof(FunctionsProcessor).GetGenericMethodInfo("ConvertFromString", typeof(DNSName), o => new Type[]{ typeof(string) })) },

            { "urn:oasis:names:tc:xacml:3.0:function:string-from-boolean", new DelegateWrapper(typeof(Func<bool, string>), typeof(FunctionsProcessor).GetGenericMethodInfo("ConvertToString", typeof(bool), o => new Type[]{ o })) },
            { "urn:oasis:names:tc:xacml:3.0:function:string-from-integer", new DelegateWrapper(typeof(Func<int, string>), typeof(FunctionsProcessor).GetGenericMethodInfo("ConvertToString", typeof(int), o => new Type[]{ o })) },
            { "urn:oasis:names:tc:xacml:3.0:function:string-from-double", new DelegateWrapper(typeof(Func<double, string>), typeof(FunctionsProcessor).GetGenericMethodInfo("ConvertToString", typeof(double), o => new Type[]{ o })) },
            { "urn:oasis:names:tc:xacml:3.0:function:string-from-time", new DelegateWrapper(typeof(Func<Time, string>), typeof(FunctionsProcessor).GetGenericMethodInfo("ConvertToString", typeof(Time), o => new Type[]{ o })) },
            { "urn:oasis:names:tc:xacml:3.0:function:string-from-date", new DelegateWrapper(typeof(Func<Date, string>), typeof(FunctionsProcessor).GetGenericMethodInfo("ConvertToString", typeof(Date), o => new Type[]{ o })) },
            { "urn:oasis:names:tc:xacml:3.0:function:string-from-dateTime", new DelegateWrapper(typeof(Func<DateTime, string>), typeof(FunctionsProcessor).GetGenericMethodInfo("ConvertToString", typeof(DateTime), o => new Type[]{ o })) },
            { "urn:oasis:names:tc:xacml:3.0:function:string-from-anyURI", new DelegateWrapper(typeof(Func<Uri, string>), typeof(FunctionsProcessor).GetMethod("UriConvertToString", BindingFlags.Static | BindingFlags.Public)) },
            { "urn:oasis:names:tc:xacml:3.0:function:string-from-dayTimeDuration", new DelegateWrapper(typeof(Func<DayTimeDuration, string>), typeof(FunctionsProcessor).GetGenericMethodInfo("ConvertToString", typeof(DayTimeDuration), o => new Type[]{ o })) },
            { "urn:oasis:names:tc:xacml:3.0:function:string-from-yearMonthDuration", new DelegateWrapper(typeof(Func<YearMonthDuration, string>), typeof(FunctionsProcessor).GetGenericMethodInfo("ConvertToString", typeof(YearMonthDuration), o => new Type[]{ o })) },
            { "urn:oasis:names:tc:xacml:3.0:function:string-from-x500Name", new DelegateWrapper(typeof(Func<x500Name, string>), typeof(FunctionsProcessor).GetGenericMethodInfo("ConvertToString", typeof(x500Name), o => new Type[]{ o })) },
            { "urn:oasis:names:tc:xacml:3.0:function:string-from-rfc822Name", new DelegateWrapper(typeof(Func<Rfc822Name, string>), typeof(FunctionsProcessor).GetGenericMethodInfo("ConvertToString", typeof(Rfc822Name), o => new Type[]{ o })) },
            { "urn:oasis:names:tc:xacml:3.0:function:string-from-ipAddress", new DelegateWrapper(typeof(Func<IPAddress, string>), typeof(FunctionsProcessor).GetGenericMethodInfo("ConvertToString", typeof(IPAddress), o => new Type[]{ o })) },
            { "urn:oasis:names:tc:xacml:3.0:function:string-from-dnsName", new DelegateWrapper(typeof(Func<DNSName, string>), typeof(FunctionsProcessor).GetGenericMethodInfo("ConvertToString", typeof(DNSName), o => new Type[]{ o })) },

            // String functions
            { "urn:oasis:names:tc:xacml:1.0:function:string-normalize-space", new DelegateWrapper(typeof(Func<string, string>), typeof(FunctionsProcessor).GetMethod("StringNormalizeSpace", BindingFlags.Static | BindingFlags.Public)) },
            { "urn:oasis:names:tc:xacml:1.0:function:string-normalize-to-lower-case", new DelegateWrapper(typeof(Func<string, string>), typeof(FunctionsProcessor).GetMethod("StringNormalizeToLowerCase", BindingFlags.Static | BindingFlags.Public)) },        
        
            // Numeric data-type conversion functions
            { "urn:oasis:names:tc:xacml:1.0:function:double-to-integer", new DelegateWrapper(typeof(Func<double, int>), typeof(FunctionsProcessor).GetMethod("DoubleToInteger", BindingFlags.Static | BindingFlags.Public)) },
            { "urn:oasis:names:tc:xacml:1.0:function:integer-to-double", new DelegateWrapper(typeof(Func<int, double>), typeof(FunctionsProcessor).GetMethod("IntegerToDouble", BindingFlags.Static | BindingFlags.Public)) },

            // Logical functions
            { "urn:oasis:names:tc:xacml:1.0:function:or", new DelegateWrapper(typeof(EnumerableToGenericParam<bool>), typeof(FunctionsProcessor).GetMethod("Or", BindingFlags.Static | BindingFlags.Public)) },
            { "urn:oasis:names:tc:xacml:1.0:function:and", new DelegateWrapper(typeof(EnumerableToGenericParam<bool>), typeof(FunctionsProcessor).GetMethod("And", BindingFlags.Static | BindingFlags.Public)) },
            { "urn:oasis:names:tc:xacml:1.0:function:n-of", new DelegateWrapper(typeof(EnumerableToGenericParamWithOneArg<bool, int, bool?>), typeof(FunctionsProcessor).GetMethod("NOf", BindingFlags.Static | BindingFlags.Public)) },
            { "urn:oasis:names:tc:xacml:1.0:function:not", new DelegateWrapper(typeof(Func<bool, bool>), typeof(FunctionsProcessor).GetMethod("Not", BindingFlags.Static | BindingFlags.Public)) },
        
            // Numeric comparison functions
            { "urn:oasis:names:tc:xacml:1.0:function:integer-greater-than", new DelegateWrapper(typeof(Func<int, int, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("GreaterThan", typeof(int), o => new Type[]{ o, o })) },
            { "urn:oasis:names:tc:xacml:1.0:function:integer-greater-than-or-equal", new DelegateWrapper(typeof(Func<int, int, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("GreaterThanOrEqual", typeof(int), o => new Type[]{ o, o })) },
            { "urn:oasis:names:tc:xacml:1.0:function:integer-less-than", new DelegateWrapper(typeof(Func<int, int, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("LessThan", typeof(int), o => new Type[]{ o, o })) },
            { "urn:oasis:names:tc:xacml:1.0:function:integer-less-than-or-equal", new DelegateWrapper(typeof(Func<int, int, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("LessThanOrEqual", typeof(int), o => new Type[]{ o, o })) },
            { "urn:oasis:names:tc:xacml:1.0:function:double-greater-than", new DelegateWrapper(typeof(Func<double, double, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("GreaterThan", typeof(double), o => new Type[]{ o, o })) },
            { "urn:oasis:names:tc:xacml:1.0:function:double-greater-than-or-equal", new DelegateWrapper(typeof(Func<double, double, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("GreaterThanOrEqual", typeof(double), o => new Type[]{ o, o })) },
            { "urn:oasis:names:tc:xacml:1.0:function:double-less-than", new DelegateWrapper(typeof(Func<double, double, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("LessThan", typeof(double), o => new Type[]{ o, o })) },
            { "urn:oasis:names:tc:xacml:1.0:function:double-less-than-or-equal", new DelegateWrapper(typeof(Func<double, double, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("LessThanOrEqual", typeof(double), o => new Type[]{ o, o })) },

            // Non-numeric comparison functions
            { "urn:oasis:names:tc:xacml:1.0:function:string-greater-than", new DelegateWrapper(typeof(Func<string, string, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("GreaterThan", typeof(string), o => new Type[]{ o, o })) },
            { "urn:oasis:names:tc:xacml:1.0:function:string-greater-than-or-equal", new DelegateWrapper(typeof(Func<string, string, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("GreaterThanOrEqual", typeof(string), o => new Type[]{ o, o })) },
            { "urn:oasis:names:tc:xacml:1.0:function:string-less-than", new DelegateWrapper(typeof(Func<string, string, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("LessThan", typeof(string), o => new Type[]{ o, o })) },
            { "urn:oasis:names:tc:xacml:1.0:function:string-less-than-or-equal", new DelegateWrapper(typeof(Func<string, string, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("LessThanOrEqual", typeof(string), o => new Type[]{ o, o })) },
            
            { "urn:oasis:names:tc:xacml:1.0:function:time-greater-than", new DelegateWrapper(typeof(Func<Time, Time, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("GreaterThan", typeof(Time), o => new Type[]{ o, o })) },
            { "urn:oasis:names:tc:xacml:1.0:function:time-greater-than-or-equal", new DelegateWrapper(typeof(Func<Time, Time, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("GreaterThanOrEqual", typeof(Time), o => new Type[]{ o, o })) },
            { "urn:oasis:names:tc:xacml:1.0:function:time-less-than", new DelegateWrapper(typeof(Func<Time, Time, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("LessThan", typeof(Time), o => new Type[]{ o, o })) },
            { "urn:oasis:names:tc:xacml:1.0:function:time-less-than-or-equal", new DelegateWrapper(typeof(Func<Time, Time, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("LessThanOrEqual", typeof(Time), o => new Type[]{ o, o })) },
            
            { "urn:oasis:names:tc:xacml:1.0:function:time-in-range", new DelegateWrapper(typeof(Func<double, double, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("LessThanOrEqual", typeof(double), o => new Type[]{ o, o })) },
            
            { "urn:oasis:names:tc:xacml:1.0:function:dateTime-greater-than", new DelegateWrapper(typeof(Func<DateTime, DateTime, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("GreaterThan", typeof(DateTime), o => new Type[]{ o, o })) },
            { "urn:oasis:names:tc:xacml:1.0:function:dateTime-greater-than-or-equal", new DelegateWrapper(typeof(Func<DateTime, DateTime, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("GreaterThanOrEqual", typeof(DateTime), o => new Type[]{ o, o })) },
            { "urn:oasis:names:tc:xacml:1.0:function:dateTime-less-than", new DelegateWrapper(typeof(Func<DateTime, DateTime, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("LessThan", typeof(DateTime), o => new Type[]{ o, o })) },
            { "urn:oasis:names:tc:xacml:1.0:function:dateTime-less-than-or-equal", new DelegateWrapper(typeof(Func<DateTime, DateTime, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("LessThanOrEqual", typeof(DateTime), o => new Type[]{ o, o })) },
            
            { "urn:oasis:names:tc:xacml:1.0:function:date-greater-than", new DelegateWrapper(typeof(Func<Date, Date, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("GreaterThan", typeof(Date), o => new Type[]{ o, o })) },
            { "urn:oasis:names:tc:xacml:1.0:function:date-greater-than-or-equal", new DelegateWrapper(typeof(Func<Date, Date, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("GreaterThanOrEqual", typeof(Date), o => new Type[]{ o, o })) },
            { "urn:oasis:names:tc:xacml:1.0:function:date-less-than", new DelegateWrapper(typeof(Func<Date, Date, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("LessThan", typeof(Date), o => new Type[]{ o, o })) },
            { "urn:oasis:names:tc:xacml:1.0:function:date-less-than-or-equal", new DelegateWrapper(typeof(Func<Date, Date, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("LessThanOrEqual", typeof(Date), o => new Type[]{ o, o })) },

            // Special match functions
            { "urn:oasis:names:tc:xacml:1.0:function:x500Name-match", new DelegateWrapper(typeof(Func<x500Name, x500Name, bool>), typeof(FunctionsProcessor).GetMethod("x500NameMatch", BindingFlags.Static | BindingFlags.Public)) },
            { "urn:oasis:names:tc:xacml:1.0:function:rfc822Name-match", new DelegateWrapper(typeof(Func<string, Rfc822Name, bool>), typeof(FunctionsProcessor).GetMethod("Rfc822Match", BindingFlags.Static | BindingFlags.Public)) },
        
            // Date and time arithmetic functions
            { "urn:oasis:names:tc:xacml:1.0:function:dateTime-add-dayTimeDuration", new DelegateWrapper(typeof(Func<DateTime, DayTimeDuration, DateTime>), typeof(FunctionsProcessor).GetMethod("DateTimeAddDayTimeDuration", BindingFlags.Static | BindingFlags.Public)) },
            { "urn:oasis:names:tc:xacml:1.0:function:dateTime-add-yearMonthDuration", new DelegateWrapper(typeof(Func<DateTime, YearMonthDuration, DateTime>), typeof(FunctionsProcessor).GetMethod("DateTimeAddYearMonthDuration", BindingFlags.Static | BindingFlags.Public)) },
            { "urn:oasis:names:tc:xacml:1.0:function:dateTime-subtract-dayTimeDuration", new DelegateWrapper(typeof(Func<DateTime, DayTimeDuration, DateTime>), typeof(FunctionsProcessor).GetMethod("DateTimeSubtractDayTimeDuration", BindingFlags.Static | BindingFlags.Public)) },
            { "urn:oasis:names:tc:xacml:1.0:function:dateTime-subtract-yearMonthDuration", new DelegateWrapper(typeof(Func<DateTime, YearMonthDuration, DateTime>), typeof(FunctionsProcessor).GetMethod("DateTimeSubtractYearMonthDuration", BindingFlags.Static | BindingFlags.Public)) },
            { "urn:oasis:names:tc:xacml:1.0:function:date-add-yearMonthDuration", new DelegateWrapper(typeof(Func<Date, YearMonthDuration, Date>), typeof(FunctionsProcessor).GetMethod("DateAddYearMonthDuration", BindingFlags.Static | BindingFlags.Public)) },
            { "urn:oasis:names:tc:xacml:1.0:function:date-subtract-yearMonthDuration", new DelegateWrapper(typeof(Func<Date, YearMonthDuration, Date>), typeof(FunctionsProcessor).GetMethod("DateSubtractYearMonthDuration", BindingFlags.Static | BindingFlags.Public)) },

            // Set functions
            { "urn:oasis:names:tc:xacml:1.0:function:string-intersection", new DelegateWrapper(typeof(Func<IEnumerable<string>, IEnumerable<string>, IEnumerable<string>>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeIntersection", typeof(string), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}), typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:boolean-intersection", new DelegateWrapper(typeof(Func<IEnumerable<bool>, IEnumerable<bool>, IEnumerable<bool>>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeIntersection", typeof(bool), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}), typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:integer-intersection", new DelegateWrapper(typeof(Func<IEnumerable<int>, IEnumerable<int>, IEnumerable<int>>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeIntersection", typeof(int), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}), typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:double-intersection", new DelegateWrapper(typeof(Func<IEnumerable<double>, IEnumerable<double>, IEnumerable<double>>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeIntersection", typeof(double), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}), typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:time-intersection", new DelegateWrapper(typeof(Func<IEnumerable<Time>, IEnumerable<Time>, IEnumerable<Time>>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeIntersection", typeof(Time), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}), typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:date-intersection", new DelegateWrapper(typeof(Func<IEnumerable<Date>, IEnumerable<Date>, IEnumerable<Date>>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeIntersection", typeof(Date), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}), typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:dateTime-intersection", new DelegateWrapper(typeof(Func<IEnumerable<DateTime>, IEnumerable<DateTime>, IEnumerable<DateTime>>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeIntersection", typeof(DateTime), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}), typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:anyURI-intersection", new DelegateWrapper(typeof(Func<IEnumerable<Uri>, IEnumerable<Uri>, IEnumerable<Uri>>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeIntersection", typeof(Uri), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}), typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:hexBinary-intersection", new DelegateWrapper(typeof(Func<IEnumerable<HexBinary>, IEnumerable<HexBinary>, IEnumerable<HexBinary>>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeIntersection", typeof(HexBinary), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}), typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:base64Binary-intersection", new DelegateWrapper(typeof(Func<IEnumerable<Base64Binary>, IEnumerable<Base64Binary>, IEnumerable<Base64Binary>>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeIntersection", typeof(Base64Binary), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}), typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:dayTimeDuration-intersection", new DelegateWrapper(typeof(Func<IEnumerable<DayTimeDuration>, IEnumerable<DayTimeDuration>, IEnumerable<DayTimeDuration>>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeIntersection", typeof(DayTimeDuration), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}), typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:yearMonthDuration-intersection", new DelegateWrapper(typeof(Func<IEnumerable<YearMonthDuration>, IEnumerable<YearMonthDuration>, IEnumerable<YearMonthDuration>>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeIntersection", typeof(YearMonthDuration), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}), typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:x500Name-intersection", new DelegateWrapper(typeof(Func<IEnumerable<x500Name>, IEnumerable<x500Name>, IEnumerable<x500Name>>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeIntersection", typeof(x500Name), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}), typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:rfc822Name-intersection", new DelegateWrapper(typeof(Func<IEnumerable<Rfc822Name>, IEnumerable<Rfc822Name>, IEnumerable<Rfc822Name>>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeIntersection", typeof(Rfc822Name), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}), typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:3.0:function:dayTimeDuration-intersection", new DelegateWrapper(typeof(Func<IEnumerable<DayTimeDuration>, IEnumerable<DayTimeDuration>, IEnumerable<DayTimeDuration>>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeIntersection", typeof(DayTimeDuration), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}), typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:3.0:function:yearMonthDuration-intersection", new DelegateWrapper(typeof(Func<IEnumerable<YearMonthDuration>, IEnumerable<YearMonthDuration>, IEnumerable<YearMonthDuration>>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeIntersection", typeof(YearMonthDuration), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}), typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },

            { "urn:oasis:names:tc:xacml:1.0:function:string-at-least-one-member-of", new DelegateWrapper(typeof(Func<IEnumerable<string>, IEnumerable<string>, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeAtLeastOneMemberOf", typeof(string), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}), typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:boolean-at-least-one-member-of", new DelegateWrapper(typeof(Func<IEnumerable<bool>, IEnumerable<bool>, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeAtLeastOneMemberOf", typeof(bool), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}), typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:integer-at-least-one-member-of", new DelegateWrapper(typeof(Func<IEnumerable<int>, IEnumerable<int>, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeAtLeastOneMemberOf", typeof(int), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}), typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:double-at-least-one-member-of", new DelegateWrapper(typeof(Func<IEnumerable<double>, IEnumerable<double>, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeAtLeastOneMemberOf", typeof(double), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}), typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:time-at-least-one-member-of", new DelegateWrapper(typeof(Func<IEnumerable<Time>, IEnumerable<Time>, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeAtLeastOneMemberOf", typeof(Time), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}), typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:date-at-least-one-member-of", new DelegateWrapper(typeof(Func<IEnumerable<Date>, IEnumerable<Date>, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeAtLeastOneMemberOf", typeof(Date), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}), typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:dateTime-at-least-one-member-of", new DelegateWrapper(typeof(Func<IEnumerable<DateTime>, IEnumerable<DateTime>, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeAtLeastOneMemberOf", typeof(DateTime), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}), typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:anyURI-at-least-one-member-of", new DelegateWrapper(typeof(Func<IEnumerable<Uri>, IEnumerable<Uri>, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeAtLeastOneMemberOf", typeof(Uri), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}), typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:hexBinary-at-least-one-member-of", new DelegateWrapper(typeof(Func<IEnumerable<HexBinary>, IEnumerable<HexBinary>, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeAtLeastOneMemberOf", typeof(HexBinary), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}), typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:base64Binary-at-least-one-member-of", new DelegateWrapper(typeof(Func<IEnumerable<Base64Binary>, IEnumerable<Base64Binary>, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeAtLeastOneMemberOf", typeof(Base64Binary), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}), typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:dayTimeDuration-at-least-one-member-of", new DelegateWrapper(typeof(Func<IEnumerable<DayTimeDuration>, IEnumerable<DayTimeDuration>, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeAtLeastOneMemberOf", typeof(DayTimeDuration), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}), typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:yearMonthDuration-at-least-one-member-of", new DelegateWrapper(typeof(Func<IEnumerable<YearMonthDuration>, IEnumerable<YearMonthDuration>, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeAtLeastOneMemberOf", typeof(YearMonthDuration), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}), typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:x500Name-at-least-one-member-of", new DelegateWrapper(typeof(Func<IEnumerable<x500Name>, IEnumerable<x500Name>, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeAtLeastOneMemberOf", typeof(x500Name), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}), typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:rfc822Name-at-least-one-member-of", new DelegateWrapper(typeof(Func<IEnumerable<Rfc822Name>, IEnumerable<Rfc822Name>, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeAtLeastOneMemberOf", typeof(Rfc822Name), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}), typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:3.0:function:dayTimeDuration-at-least-one-member-of", new DelegateWrapper(typeof(Func<IEnumerable<DayTimeDuration>, IEnumerable<DayTimeDuration>, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeAtLeastOneMemberOf", typeof(DayTimeDuration), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}), typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:3.0:function:yearMonthDuration-at-least-one-member-of", new DelegateWrapper(typeof(Func<IEnumerable<YearMonthDuration>, IEnumerable<YearMonthDuration>, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeAtLeastOneMemberOf", typeof(YearMonthDuration), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}), typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },

            { "urn:oasis:names:tc:xacml:1.0:function:string-union", new DelegateWrapper(typeof(Func<IEnumerable<string>, IEnumerable<string>, IEnumerable<string>>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeUnion", typeof(string), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}), typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:boolean-union", new DelegateWrapper(typeof(Func<IEnumerable<bool>, IEnumerable<bool>, IEnumerable<bool>>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeUnion", typeof(bool), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}), typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:integer-union", new DelegateWrapper(typeof(Func<IEnumerable<int>, IEnumerable<int>, IEnumerable<int>>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeUnion", typeof(int), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}), typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:double-union", new DelegateWrapper(typeof(Func<IEnumerable<double>, IEnumerable<double>, IEnumerable<double>>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeUnion", typeof(double), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}), typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:time-union", new DelegateWrapper(typeof(Func<IEnumerable<Time>, IEnumerable<Time>, IEnumerable<Time>>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeUnion", typeof(Time), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}), typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:date-union", new DelegateWrapper(typeof(Func<IEnumerable<Date>, IEnumerable<Date>, IEnumerable<Date>>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeUnion", typeof(Date), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}), typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:dateTime-union", new DelegateWrapper(typeof(Func<IEnumerable<DateTime>, IEnumerable<DateTime>, IEnumerable<DateTime>>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeUnion", typeof(DateTime), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}), typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:anyURI-union", new DelegateWrapper(typeof(Func<IEnumerable<Uri>, IEnumerable<Uri>, IEnumerable<Uri>>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeUnion", typeof(Uri), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}), typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:hexBinary-union", new DelegateWrapper(typeof(Func<IEnumerable<HexBinary>, IEnumerable<HexBinary>, IEnumerable<HexBinary>>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeUnion", typeof(HexBinary), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}), typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:base64Binary-union", new DelegateWrapper(typeof(Func<IEnumerable<Base64Binary>, IEnumerable<Base64Binary>, IEnumerable<Base64Binary>>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeUnion", typeof(Base64Binary), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}), typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:dayTimeDuration-union", new DelegateWrapper(typeof(Func<IEnumerable<DayTimeDuration>, IEnumerable<DayTimeDuration>, IEnumerable<DayTimeDuration>>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeUnion", typeof(DayTimeDuration), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}), typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:yearMonthDuration-union", new DelegateWrapper(typeof(Func<IEnumerable<YearMonthDuration>, IEnumerable<YearMonthDuration>, IEnumerable<YearMonthDuration>>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeUnion", typeof(YearMonthDuration), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}), typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:x500Name-union", new DelegateWrapper(typeof(Func<IEnumerable<x500Name>, IEnumerable<x500Name>, IEnumerable<x500Name>>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeUnion", typeof(x500Name), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}), typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:rfc822Name-union", new DelegateWrapper(typeof(Func<IEnumerable<Rfc822Name>, IEnumerable<Rfc822Name>, IEnumerable<Rfc822Name>>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeUnion", typeof(Rfc822Name), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}), typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },        
            { "urn:oasis:names:tc:xacml:3.0:function:dayTimeDuration-union", new DelegateWrapper(typeof(Func<IEnumerable<DayTimeDuration>, IEnumerable<DayTimeDuration>, IEnumerable<DayTimeDuration>>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeUnion", typeof(DayTimeDuration), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}), typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:3.0:function:yearMonthDuration-union", new DelegateWrapper(typeof(Func<IEnumerable<YearMonthDuration>, IEnumerable<YearMonthDuration>, IEnumerable<YearMonthDuration>>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeUnion", typeof(YearMonthDuration), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}), typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
        

            { "urn:oasis:names:tc:xacml:1.0:function:string-subset", new DelegateWrapper(typeof(Func<IEnumerable<string>, IEnumerable<string>, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeSubset", typeof(string), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}), typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:boolean-subset", new DelegateWrapper(typeof(Func<IEnumerable<bool>, IEnumerable<bool>, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeSubset", typeof(bool), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}), typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:integer-subset", new DelegateWrapper(typeof(Func<IEnumerable<int>, IEnumerable<int>, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeSubset", typeof(int), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}), typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:double-subset", new DelegateWrapper(typeof(Func<IEnumerable<double>, IEnumerable<double>, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeSubset", typeof(double), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}), typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:time-subset", new DelegateWrapper(typeof(Func<IEnumerable<Time>, IEnumerable<Time>, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeSubset", typeof(Time), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}), typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:date-subset", new DelegateWrapper(typeof(Func<IEnumerable<Date>, IEnumerable<Date>, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeSubset", typeof(Date), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}), typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:dateTime-subset", new DelegateWrapper(typeof(Func<IEnumerable<DateTime>, IEnumerable<DateTime>, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeSubset", typeof(DateTime), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}), typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:anyURI-subset", new DelegateWrapper(typeof(Func<IEnumerable<Uri>, IEnumerable<Uri>, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeSubset", typeof(Uri), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}), typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:hexBinary-subset", new DelegateWrapper(typeof(Func<IEnumerable<HexBinary>, IEnumerable<HexBinary>, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeSubset", typeof(HexBinary), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}), typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:base64Binary-subset", new DelegateWrapper(typeof(Func<IEnumerable<Base64Binary>, IEnumerable<Base64Binary>, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeSubset", typeof(Base64Binary), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}), typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:dayTimeDuration-subset", new DelegateWrapper(typeof(Func<IEnumerable<DayTimeDuration>, IEnumerable<DayTimeDuration>, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeSubset", typeof(DayTimeDuration), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}), typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:yearMonthDuration-subset", new DelegateWrapper(typeof(Func<IEnumerable<YearMonthDuration>, IEnumerable<YearMonthDuration>, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeSubset", typeof(YearMonthDuration), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}), typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:x500Name-subset", new DelegateWrapper(typeof(Func<IEnumerable<x500Name>, IEnumerable<x500Name>, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeSubset", typeof(x500Name), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}), typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:rfc822Name-subset", new DelegateWrapper(typeof(Func<IEnumerable<Rfc822Name>, IEnumerable<Rfc822Name>, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeSubset", typeof(Rfc822Name), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}), typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },        
            { "urn:oasis:names:tc:xacml:3.0:function:dayTimeDuration-subset", new DelegateWrapper(typeof(Func<IEnumerable<DayTimeDuration>, IEnumerable<DayTimeDuration>, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeSubset", typeof(DayTimeDuration), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}), typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:3.0:function:yearMonthDuration-subset", new DelegateWrapper(typeof(Func<IEnumerable<YearMonthDuration>, IEnumerable<YearMonthDuration>, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeSubset", typeof(YearMonthDuration), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}), typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },

            { "urn:oasis:names:tc:xacml:1.0:function:string-set-equals", new DelegateWrapper(typeof(Func<IEnumerable<string>, IEnumerable<string>, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeSetEquals", typeof(string), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}), typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:boolean-set-equals", new DelegateWrapper(typeof(Func<IEnumerable<bool>, IEnumerable<bool>, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeSetEquals", typeof(bool), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}), typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:integer-set-equals", new DelegateWrapper(typeof(Func<IEnumerable<int>, IEnumerable<int>, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeSetEquals", typeof(int), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}), typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:double-set-equals", new DelegateWrapper(typeof(Func<IEnumerable<double>, IEnumerable<double>, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeSetEquals", typeof(double), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}), typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:time-set-equals", new DelegateWrapper(typeof(Func<IEnumerable<Time>, IEnumerable<Time>, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeSetEquals", typeof(Time), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}), typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:date-set-equals", new DelegateWrapper(typeof(Func<IEnumerable<Date>, IEnumerable<Date>, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeSetEquals", typeof(Date), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}), typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:dateTime-set-equals", new DelegateWrapper(typeof(Func<IEnumerable<DateTime>, IEnumerable<DateTime>, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeSetEquals", typeof(DateTime), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}), typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:anyURI-set-equals", new DelegateWrapper(typeof(Func<IEnumerable<Uri>, IEnumerable<Uri>, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeSetEquals", typeof(Uri), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}), typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:hexBinary-set-equals", new DelegateWrapper(typeof(Func<IEnumerable<HexBinary>, IEnumerable<HexBinary>, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeSetEquals", typeof(HexBinary), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}), typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:base64Binary-set-equals", new DelegateWrapper(typeof(Func<IEnumerable<Base64Binary>, IEnumerable<Base64Binary>, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeSetEquals", typeof(Base64Binary), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}), typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:dayTimeDuration-set-equals", new DelegateWrapper(typeof(Func<IEnumerable<DayTimeDuration>, IEnumerable<DayTimeDuration>, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeSetEquals", typeof(DayTimeDuration), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}), typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:yearMonthDuration-set-equals", new DelegateWrapper(typeof(Func<IEnumerable<YearMonthDuration>, IEnumerable<YearMonthDuration>, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeSetEquals", typeof(YearMonthDuration), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}), typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:x500Name-set-equals", new DelegateWrapper(typeof(Func<IEnumerable<x500Name>, IEnumerable<x500Name>, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeSetEquals", typeof(x500Name), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}), typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:1.0:function:rfc822Name-set-equals", new DelegateWrapper(typeof(Func<IEnumerable<Rfc822Name>, IEnumerable<Rfc822Name>, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeSetEquals", typeof(Rfc822Name), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}), typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) }, 
            { "urn:oasis:names:tc:xacml:3.0:function:dayTimeDuration-set-equals", new DelegateWrapper(typeof(Func<IEnumerable<DayTimeDuration>, IEnumerable<DayTimeDuration>, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeSetEquals", typeof(DayTimeDuration), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}), typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },
            { "urn:oasis:names:tc:xacml:3.0:function:yearMonthDuration-set-equals", new DelegateWrapper(typeof(Func<IEnumerable<YearMonthDuration>, IEnumerable<YearMonthDuration>, bool>), typeof(FunctionsProcessor).GetGenericMethodInfo("TypeSetEquals", typeof(YearMonthDuration), o => new Type[]{ typeof(IEnumerable<>).MakeGenericType(new Type[] {o}), typeof(IEnumerable<>).MakeGenericType(new Type[] {o}) })) },

            // XPath-based functions
            { "urn:oasis:names:tc:xacml:1.0:function:xpath-node-count", new DelegateWithXPathContextWrapper(typeof(Func<XPathContext, string, int>), typeof(FunctionsProcessor).GetMethod("XpathNodeCount", BindingFlags.Static | BindingFlags.Public)) },
            { "urn:oasis:names:tc:xacml:1.0:function:xpath-node-equal", new DelegateWithXPathContextWrapper(typeof(Func<XPathContext, string, string, bool>), typeof(FunctionsProcessor).GetMethod("XpathNodeEqual", BindingFlags.Static | BindingFlags.Public)) },
            { "urn:oasis:names:tc:xacml:1.0:function:xpath-node-match", new DelegateWithXPathContextWrapper(typeof(Func<XPathContext, string, string, bool>), typeof(FunctionsProcessor).GetMethod("XpathNodeMatch", BindingFlags.Static | BindingFlags.Public)) },        
        
            // 3.0
            { "urn:oasis:names:tc:xacml:3.0:function:dateTime-add-dayTimeDuration", new DelegateWrapper(typeof(Func<DateTime, DayTimeDuration, DateTime>), typeof(FunctionsProcessor).GetMethod("DateTimeAddDayTimeDuration", BindingFlags.Static | BindingFlags.Public)) },
            { "urn:oasis:names:tc:xacml:3.0:function:dateTime-add-yearMonthDuration", new DelegateWrapper(typeof(Func<DateTime, YearMonthDuration, DateTime>), typeof(FunctionsProcessor).GetMethod("DateTimeAddYearMonthDuration", BindingFlags.Static | BindingFlags.Public)) },
            { "urn:oasis:names:tc:xacml:3.0:function:dateTime-subtract-dayTimeDuration", new DelegateWrapper(typeof(Func<DateTime, DayTimeDuration, DateTime>), typeof(FunctionsProcessor).GetMethod("DateTimeSubtractDayTimeDuration", BindingFlags.Static | BindingFlags.Public)) },
            { "urn:oasis:names:tc:xacml:3.0:function:dateTime-subtract-yearMonthDuration", new DelegateWrapper(typeof(Func<DateTime, YearMonthDuration, DateTime>), typeof(FunctionsProcessor).GetMethod("DateTimeSubtractYearMonthDuration", BindingFlags.Static | BindingFlags.Public)) },
            { "urn:oasis:names:tc:xacml:3.0:function:date-add-yearMonthDuration", new DelegateWrapper(typeof(Func<Date, YearMonthDuration, Date>), typeof(FunctionsProcessor).GetMethod("DateAddYearMonthDuration", BindingFlags.Static | BindingFlags.Public)) },
            { "urn:oasis:names:tc:xacml:3.0:function:date-subtract-yearMonthDuration", new DelegateWrapper(typeof(Func<Date, YearMonthDuration, Date>), typeof(FunctionsProcessor).GetMethod("DateSubtractYearMonthDuration", BindingFlags.Static | BindingFlags.Public)) },
            
            { "urn:oasis:names:tc:xacml:3.0:function:xpath-node-count", new DelegateWithXPathContextWrapper(typeof(Func<XPathContext, XPathExpressionType, int>), typeof(FunctionsProcessor).GetMethod("XpathNodeCount_30", BindingFlags.Static | BindingFlags.Public)) },
            { "urn:oasis:names:tc:xacml:3.0:function:xpath-node-equal", new DelegateWithXPathContextWrapper(typeof(Func<XPathContext, XPathExpressionType, XPathExpressionType, bool>), typeof(FunctionsProcessor).GetMethod("XpathNodeEqual_30", BindingFlags.Static | BindingFlags.Public)) },
            { "urn:oasis:names:tc:xacml:3.0:function:xpath-node-match", new DelegateWithXPathContextWrapper(typeof(Func<XPathContext, XPathExpressionType, XPathExpressionType, bool>), typeof(FunctionsProcessor).GetMethod("XpathNodeMatch_30", BindingFlags.Static | BindingFlags.Public)) },        
        
            // String functions
            { "urn:oasis:names:tc:xacml:3.0:function:string-starts-with", new DelegateWrapper(typeof(Func<string, string, bool>), typeof(FunctionsProcessor).GetMethod("StringStartsWith", BindingFlags.Static | BindingFlags.Public)) },
            { "urn:oasis:names:tc:xacml:3.0:function:anyURI-starts-with", new DelegateWrapper(typeof(Func<string, Uri, bool>), typeof(FunctionsProcessor).GetMethod("UriStartsWith", BindingFlags.Static | BindingFlags.Public)) },
            { "urn:oasis:names:tc:xacml:3.0:function:string-ends-with", new DelegateWrapper(typeof(Func<string, string, bool>), typeof(FunctionsProcessor).GetMethod("StringEndsWith", BindingFlags.Static | BindingFlags.Public)) },
            { "urn:oasis:names:tc:xacml:3.0:function:anyURI-ends-with", new DelegateWrapper(typeof(Func<string, Uri, bool>), typeof(FunctionsProcessor).GetMethod("UriEndsWith", BindingFlags.Static | BindingFlags.Public)) },
            { "urn:oasis:names:tc:xacml:3.0:function:string-contains", new DelegateWrapper(typeof(Func<string, string, bool>), typeof(FunctionsProcessor).GetMethod("StringContains", BindingFlags.Static | BindingFlags.Public)) },
            { "urn:oasis:names:tc:xacml:3.0:function:anyURI-contains", new DelegateWrapper(typeof(Func<string, Uri, bool>), typeof(FunctionsProcessor).GetMethod("UriContains", BindingFlags.Static | BindingFlags.Public)) },
            { "urn:oasis:names:tc:xacml:3.0:function:string-substring", new DelegateWrapper(typeof(Func<string, int, int, string>), typeof(FunctionsProcessor).GetMethod("StringSubstring", BindingFlags.Static | BindingFlags.Public)) },
            { "urn:oasis:names:tc:xacml:3.0:function:anyURI-substring", new DelegateWrapper(typeof(Func<Uri, int, int, string>), typeof(FunctionsProcessor).GetMethod("UriSubstring", BindingFlags.Static | BindingFlags.Public)) },
        };

        /// <summary>
        /// Prevents a default instance of the <see cref="FunctionsProcessor"/> class from being created.
        /// </summary>
        private FunctionsProcessor() {
        }

#region Atithmetic functions

        public static int IntegerAdd(params int[] obj) {
            return obj.Sum();
        }

        public static double DoubleAdd(params double[] obj) {
            return obj.Sum();
        }

        public static int IntegerSubtract(int first, int second) {
            return first - second;
        }

        public static double DoubleSubtract(double first, double second) {
            return first - second;
        }

        public static int IntegerMultiply(int first, int second) {
            return first * second;
        }

        public static double DoubleMultiply(double first, double second) {
            return first * second;
        }

        public static int? IntegerDivide(int first, int second) {
            if (second == 0)
                return null;

            return first / second;
        }

        public static double? DoubleDivide(double first, double second) {
            if (second == 0.0)
                return null;

            return first / second;
        }

        public static int? IntegerMod(int first, int second) {
            if (second == 0)
                return null;

            return first % second;
        }

        public static int IntegerAbs(int value) {
            return Math.Abs(value);
        }

        public static double DoubleAbs(double value) {
            return Math.Abs(value);
        }

        public static double Round(double value) {
            return Math.Round(value);
        }

        public static double Floor(double value) {
            return Math.Floor(value);
        }

#endregion

#region String conversion functions

        public static string StringNormalizeSpace(string value) {
            return value.Trim();
        }

        public static string StringNormalizeToLowerCase(string value) {
            return value.ToLowerInvariant();
        }

#endregion

#region Data-type conversion functions

        public static T ConvertFromString<T>(string source) {
            return (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromString(source);
        }

        public static Uri UriConvertFromString(string source) {
            Uri result;
            if (Uri.TryCreate(source, UriKind.RelativeOrAbsolute, out result)) {
                return result;
            }
            else {
                throw new XacmlInvalidDataTypeException("Cannot convert to anyUri type");
            }
        }

        public static string ConvertToString<T>(T source) {
            return source.ToString();
        }

        public static string UriConvertToString(Uri source) {
            return source.OriginalString;
        }

#endregion

#region Numeric data-type conversion functions

        public static int DoubleToInteger(double value) {
            return (int)value;
        }

        public static double IntegerToDouble(int value) {
            return Convert.ToDouble(value);
        }

#endregion

#region Logical function

        public static bool Or(params bool[] values) {
            return values.Any(o => o == true);
        }

        public static bool And(params bool[] values) {
            return !values.Any(o => o == false);
        }

        public static bool? NOf(int count, params bool[] values) {
            if (values.Count() < count)
                return null;

            if (count == 0)
                return true;

            foreach (bool val in values) {
                if (val) {
                    count--;
                }

                if (count == 0) {
                    return true;
                }
            }

            return false;
        }

        public static bool Not(bool value) {
            return !value;
        }

#endregion

#region Comparison functions

        public static bool GreaterThan<T>(T first, T second) where T : IComparable<T> {
            return first.CompareTo(second) > 0;
        }

        public static bool GreaterThanOrEqual<T>(T first, T second) where T : IComparable<T> {
            return first.CompareTo(second) >= 0;
        }

        public static bool LessThan<T>(T first, T second) where T : IComparable<T> {
            return first.CompareTo(second) < 0;
        }

        public static bool LessThanOrEqual<T>(T first, T second) where T : IComparable<T> {
            return first.CompareTo(second) <= 0;
        }

#endregion

#region Special match functions

        public static bool x500NameMatch(x500Name first, x500Name second) {
            return first.Match(second);
        }

        public static bool Rfc822Match(string value, Rfc822Name element) {
            return element.Match(value);
        }

#endregion

#region Date and time arithmetic functions

        public static DateTime DateTimeAddDayTimeDuration(DateTime dateTime, DayTimeDuration duration) {
            return dateTime + duration;
        }

        public static DateTime DateTimeAddYearMonthDuration(DateTime dateTime, YearMonthDuration duration) {
            return dateTime + duration;
        }

        public static DateTime DateTimeSubtractDayTimeDuration(DateTime dateTime, DayTimeDuration duration) {
            return dateTime - duration;
        }

        public static DateTime DateTimeSubtractYearMonthDuration(DateTime dateTime, YearMonthDuration duration) {
            return dateTime - duration;
        }

        public static Date DateAddYearMonthDuration(Date date, YearMonthDuration duration) {
            return date + duration;
        }

        public static Date DateSubtractYearMonthDuration(Date date, YearMonthDuration duration) {
            return date - duration;
        }

#endregion

#region Higher-order bag functions

        public static bool AnyOf(XPathContext xpathContext, DelegateWrapper del, object elem, IEnumerable<object> collection) {
            foreach (object obj in collection) {
                object result = del.DynamicInvoke(xpathContext, new[] { elem, obj });
                bool? resultBoolean = result as bool?;
                if (!resultBoolean.HasValue) {
                    throw new XacmlInvalidDataTypeException("Function is not predicate");
                }

                if (resultBoolean.Value) {
                    return true;
                }
            }

            return false;
        }

        public static bool AnyOf_30(XPathContext xpathContext, DelegateWrapper del, params object[] par) {
            IEnumerable<object> lists = par.Where(o => (typeof(System.Collections.IEnumerable)).IsAssignableFrom(o.GetType()))
                .Where(o => !(o is string));

            bool globalResult = false;

            if (lists.Count() != 1) {
                throw new XacmlInvalidDataTypeException("Function require one and only one Enumerable parameter");
            }
            else {
                object res = lists.First();

                List<object> otherParams = par.Except(lists).ToList();

                foreach (object elem in (IEnumerable)res) {
                    object result = del.DynamicInvoke(xpathContext, otherParams.Concat(new[] { elem }).ToArray());

                    bool? resultBoolean = result as bool?;
                    if (!resultBoolean.HasValue) {
                        throw new XacmlInvalidDataTypeException("Function is not predicate");
                    }

                    if (resultBoolean.Value) {
                        globalResult = true;
                        break;
                    }
                }
            }

            return globalResult;
        }

        public static bool AllOf_30(XPathContext xpathContext, DelegateWrapper del, params object[] par) {
            IEnumerable<object> lists = par.Where(o => (typeof(System.Collections.IEnumerable)).IsAssignableFrom(o.GetType()))
                .Where(o => !(o is string));

            bool globalResult = true;
            Type i = typeof(IEnumerable);

            if (lists.Count() != 1) {
                throw new XacmlInvalidDataTypeException("Function require one and only one Enumerable parameter");
            }
            else {
                object res = lists.First();
                List<object> otherParams = par.Except(lists).ToList();

                foreach (object elem in (IEnumerable)res) {
                    object result = del.DynamicInvoke(xpathContext, otherParams.Concat(new[] { elem }).ToArray());

                    bool? resultBoolean = result as bool?;
                    if (!resultBoolean.HasValue) {
                        throw new XacmlInvalidDataTypeException("Function is not predicate");
                    }

                    if (!resultBoolean.Value) {
                        globalResult = false;
                        break;
                    }
                }
            }

            return globalResult;
        }

        public static bool AllOf(XPathContext xpathContext, DelegateWrapper del, object elem, IEnumerable<object> collection) {
            foreach (object obj in collection) {
                object result = del.DynamicInvoke(xpathContext, new[] { elem, obj });
                bool? resultBoolean = result as bool?;
                if (!resultBoolean.HasValue) {
                    throw new XacmlInvalidDataTypeException("Function is not predicate");
                }

                if (!resultBoolean.Value) {
                    return false;
                }
            }

            return true;
        }

        public static bool AnyOfAny(XPathContext xpathContext, DelegateWrapper del, IEnumerable<object> collection1, IEnumerable<object> collection2) {
            foreach (object obj1 in collection1) {
                foreach (object obj2 in collection2) {
                    object result = del.DynamicInvoke(xpathContext, new[] { obj1, obj2 });
                    bool? resultBoolean = result as bool?;
                    if (!resultBoolean.HasValue) {
                        throw new XacmlInvalidDataTypeException("Function is not predicate");
                    }

                    if (resultBoolean.Value) {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool AnyOfAny_30(XPathContext xpathContext, DelegateWrapper del, params object[] par) {
            bool atLeastOneTrue = false;

            PermutationProcessor permutationGenerator = new PermutationProcessor(par, o => {
                object result = del.DynamicInvoke(xpathContext, o.ToArray());
                bool? resultBoolean = result as bool?;
                if (!resultBoolean.HasValue) {
                    throw new XacmlInvalidDataTypeException("Function is not predicate");
                }

                if (resultBoolean.Value) {
                    atLeastOneTrue = true;
                }

                // if true -> break
                return resultBoolean.Value;
            });

            permutationGenerator.Run();

            return atLeastOneTrue;
        }

        public static bool AllOfAny(XPathContext xpathContext, DelegateWrapper del, IEnumerable<object> collection1, IEnumerable<object> collection2) {
            foreach (object obj1 in collection1) {
                bool anyTrue = false;
                foreach (object obj2 in collection2) {
                    object result = del.DynamicInvoke(xpathContext, new[] { obj1, obj2 });
                    bool? resultBoolean = result as bool?;
                    if (!resultBoolean.HasValue) {
                        throw new XacmlInvalidDataTypeException("Function is not predicate");
                    }

                    if (resultBoolean.Value) {
                        anyTrue = true;
                        break;
                    }
                }

                if (!anyTrue) {
                    return false;
                }
            }

            return true;
        }

        public static bool AnyOfAll(XPathContext xpathContext, DelegateWrapper del, IEnumerable<object> collection1, IEnumerable<object> collection2) {
            foreach (object obj2 in collection2) {
                bool anyTrue = false;
                foreach (object obj1 in collection1) {
                    object result = del.DynamicInvoke(xpathContext, new[] { obj1, obj2 });
                    bool? resultBoolean = result as bool?;
                    if (!resultBoolean.HasValue) {
                        throw new XacmlInvalidDataTypeException("Function is not predicate");
                    }

                    if (resultBoolean.Value) {
                        anyTrue = true;
                        break;
                    }
                }

                if (!anyTrue) {
                    return false;
                }
            }

            return true;
        }

        public static bool AllOfAll(XPathContext xpathContext, DelegateWrapper del, IEnumerable<object> collection1, IEnumerable<object> collection2) {
            foreach (object obj1 in collection1) {
                foreach (object obj2 in collection2) {
                    object result = del.DynamicInvoke(xpathContext, new[] { obj1, obj2 });
                    bool? resultBoolean = result as bool?;
                    if (!resultBoolean.HasValue) {
                        throw new XacmlInvalidDataTypeException("Function is not predicate");
                    }

                    if (!resultBoolean.Value) {
                        return false;
                    }
                }
            }

            return true;
        }

        public static IEnumerable<object> Map(XPathContext xpathContext, DelegateWrapper del, IEnumerable<object> collection) {
            foreach (object obj in collection) {
                yield return del.DynamicInvoke(xpathContext, new[] { obj });
            }
        }

        public static IEnumerable<object> Map_30(XPathContext xpathContext, DelegateWrapper del, params object[] par) {
            List<object> result = new List<object>();

            IEnumerable<object> lists = par.Where(o => (typeof(IEnumerable)).IsAssignableFrom(o.GetType()))
                .Where(o => !(o is string));

            if (lists.Count() != 1) {
                throw new XacmlInvalidDataTypeException("Function require one and only one Enumerable parameter");
            }
            else {
                object res = lists.First();

                List<object> otherParams = par.Except(lists).ToList();

                foreach (object elem in (IEnumerable)res) {
                    result.Add(del.DynamicInvoke(xpathContext, otherParams.Concat(new[] { elem }).ToArray()));
                }
            }

            return result;
        }

#endregion

#region Set functions

        public static IEnumerable<T> TypeIntersection<T>(IEnumerable<T> collection1, IEnumerable<T> collection2) {
            return collection1.Intersect(collection2);
            //return collection1.Where(o => collection2.Contains(o));
        }

        public static bool TypeAtLeastOneMemberOf<T>(IEnumerable<T> collection1, IEnumerable<T> collection2) {
            return collection1.Any(o => FunctionsProcessor.TypeIsIn(o, collection2));
        }

        public static IEnumerable<T> TypeUnion<T>(IEnumerable<T> collection1, IEnumerable<T> collection2) {
            return collection1.Union(collection2);
        }

        public static bool TypeSubset<T>(IEnumerable<T> collection1, IEnumerable<T> collection2) {
            IEnumerable<T> collection2Distinct = collection2.Distinct();
            return collection1.Distinct().All(o => collection2Distinct.Contains(o));
        }

        public static bool TypeSetEquals<T>(IEnumerable<T> collection1, IEnumerable<T> collection2) {
            return FunctionsProcessor.TypeSubset(collection1, collection2) && FunctionsProcessor.TypeSubset(collection2, collection1);
        }

#endregion

#region Bag functions

        public static IEnumerable<T> CreateBag<T>(params T[] values) {
            return values.AsEnumerable();
        }

        public static T OneAndOnly<T>(IEnumerable<T> collection) {
            return collection.Single();
        }

        public static int BagSize<T>(IEnumerable<T> collection) {
            return collection.Count();
        }

        public static bool TypeIsIn<T>(T elem, IEnumerable<T> collection) {
            return collection.Contains(elem);
        }

#endregion

#region Equal function

        public static bool EqualFunc<T>(T obj1, T obj2) where T : IEquatable<T> {
            return obj1.Equals(obj2);
        }

        public static bool EqualFunc(Uri obj1, Uri obj2) {
            return Uri.Compare(obj1, obj2, UriComponents.AbsoluteUri, UriFormat.UriEscaped, StringComparison.OrdinalIgnoreCase) == 0;
        }

        public static bool StringEqualIgnoreCaseFunc(string obj1, string obj2) {
            return string.Equals(obj1, obj1, StringComparison.OrdinalIgnoreCase);
        }

#endregion

#region Regex functions

        /// <summary>
        /// Ir nepieciešams atbalsts http://www.w3.org/TR/2002/WD-xquery-operators-20020816/#func-matches <code>xf:matches</code> funkcijai
        /// Tas ir Perl sintakse, tāpēc, .NET vajadzētu atbalstīt
        /// Var but vajadzes pievienot RegexOptions napametrus
        /// http://stackoverflow.com/questions/3417644/translate-perl-regular-expressions-to-net/3417674#3417674
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="regexp"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool RegexpMatch<T>(string regexp, T value) {
            // This variant is better, but use Saxon library
            //XQueryEngine engine = new XQueryEngine();
            //bool result = engine.EvaluateExpression<bool>(string.Format("matches('{0}', '{1}')", value, regexp));

            string valueString = value.ToString();
            return Regex.IsMatch(valueString, regexp);
        }

#endregion

#region XPath-based functions

        public static int XpathNodeCount(XPathContext xpathContext, string xpath) {
            if (xpathContext.Version == null) {
                throw new ArgumentException("<XPathVersion> element is REQUIRED");
            }

            IEnumerable<XmlNode> result = XPathProcessor.Instance[xpathContext.Version.ToString()].Invoke(xpathContext.Document, @"/*[local-name()='Request']", xpath, xpathContext.Namespaces);
            return result.Count();
        }

        public static bool XpathNodeEqual(XPathContext xpathContext, string xpath1, string xpath2) {
            if (xpathContext.Version == null) {
                throw new ArgumentException("<XPathVersion> element is REQUIRED");
            }

            IEnumerable<XElement> result1 = XPathProcessor.Instance[xpathContext.Version.ToString()].Invoke(xpathContext.Document, @"/*[local-name()='Request']", xpath1, xpathContext.Namespaces)
                .Select(o => XElement.Load(o.CreateNavigator().ReadSubtree()));
            IEnumerable<XElement> result2 = XPathProcessor.Instance[xpathContext.Version.ToString()].Invoke(xpathContext.Document, @"/*[local-name()='Request']", xpath2, xpathContext.Namespaces)
                .Select(o => XElement.Load(o.CreateNavigator().ReadSubtree()));

            return result1.Any(o => result2.Any(b => XElement.DeepEquals(o, b)));
        }

        public static bool XpathNodeMatch(XPathContext xpathContext, string xpath1, string xpath2) {
            if (xpathContext.Version == null) {
                throw new ArgumentException("<XPathVersion> element is REQUIRED");
            }

            IEnumerable<XElement> result1 = XPathProcessor.Instance[xpathContext.Version.ToString()].Invoke(xpathContext.Document, @"/*[local-name()='Request']", xpath1, xpathContext.Namespaces)
                .Select(o => XElement.Load(o.CreateNavigator().ReadSubtree()));
            IEnumerable<XElement> result2 = XPathProcessor.Instance[xpathContext.Version.ToString()].Invoke(xpathContext.Document, @"/*[local-name()='Request']", xpath2, xpathContext.Namespaces)
                .Select(o => XElement.Load(o.CreateNavigator().ReadSubtree()));

            foreach (XElement el1 in result1) {
                foreach (XElement el2 in result2) {
                    var d1 = el1.DescendantNodes();
                    var d2 = el2.DescendantNodes();

                    if (d1.Any(o => d2.Any(b => XElement.DeepEquals(o, b)))) {
                        return true;
                    }
                }
            }

            return false;
        }

        public static int XpathNodeCount_30(XPathContext xpathContext, XPathExpressionType xpath) {
            if (xpathContext.Version == null) {
                throw new ArgumentException("<XPathVersion> element is REQUIRED");
            }

            IEnumerable<XmlNode> result = XPathProcessor.Instance[xpathContext.Version.ToString()].Invoke(xpathContext.Document, string.Format(@"//*[local-name()='Attributes'][@Category='{0}']/*[local-name()='Content']/*", xpath.XPathCategory), xpath.ToString(), xpathContext.Namespaces);
            return result.Count();
        }

        public static bool XpathNodeEqual_30(XPathContext xpathContext, XPathExpressionType xpath1, XPathExpressionType xpath2) {
            if (xpathContext.Version == null) {
                throw new ArgumentException("<XPathVersion> element is REQUIRED");
            }

            IEnumerable<XElement> result1 = XPathProcessor.Instance[xpathContext.Version.ToString()].Invoke(xpathContext.Document, string.Format(@"//*[local-name()='Attributes'][@Category='{0}']/*[local-name()='Content']/*", xpath1.XPathCategory), xpath1.ToString(), xpathContext.Namespaces)
                .Select(o => XElement.Load(o.CreateNavigator().ReadSubtree()));
            IEnumerable<XElement> result2 = XPathProcessor.Instance[xpathContext.Version.ToString()].Invoke(xpathContext.Document, string.Format(@"//*[local-name()='Attributes'][@Category='{0}']/*[local-name()='Content']/*", xpath2.XPathCategory), xpath2.ToString(), xpathContext.Namespaces)
                .Select(o => XElement.Load(o.CreateNavigator().ReadSubtree()));

            return result1.Any(o => result2.Any(b => XElement.DeepEquals(o, b)));
        }

        public static bool XpathNodeMatch_30(XPathContext xpathContext, XPathExpressionType xpath1, XPathExpressionType xpath2) {
            if (xpathContext.Version == null) {
                throw new ArgumentException("<XPathVersion> element is REQUIRED");
            }

            IEnumerable<XElement> result1 = XPathProcessor.Instance[xpathContext.Version.ToString()].Invoke(xpathContext.Document, string.Format(@"//*[local-name()='Attributes'][@Category='{0}']/*[local-name()='Content']/*", xpath1.XPathCategory), xpath1.ToString(), xpathContext.Namespaces)
                .Select(o => XElement.Load(o.CreateNavigator().ReadSubtree()));
            IEnumerable<XElement> result2 = XPathProcessor.Instance[xpathContext.Version.ToString()].Invoke(xpathContext.Document, string.Format(@"//*[local-name()='Attributes'][@Category='{0}']/*[local-name()='Content']/*", xpath2.XPathCategory), xpath2.ToString(), xpathContext.Namespaces)
                .Select(o => XElement.Load(o.CreateNavigator().ReadSubtree()));

            foreach (XElement el1 in result1) {
                foreach (XElement el2 in result2) {
                    var d1 = el1.DescendantNodes();
                    var d2 = el2.DescendantNodes();

                    if (d1.Any(o => d2.Any(b => XElement.DeepEquals(o, b)))) {
                        return true;
                    }
                }
            }

            return false;
        }

#endregion

#region String functions

        public static bool StringStartsWith(string pattern, string value) {
            return value.StartsWith(pattern);
        }

        public static bool UriStartsWith(string pattern, Uri value) {
            return value.OriginalString.StartsWith(pattern);
        }

        public static bool StringEndsWith(string pattern, string value) {
            return value.EndsWith(pattern);
        }

        public static bool UriEndsWith(string pattern, Uri value) {
            return value.OriginalString.EndsWith(pattern);
        }

        public static bool StringContains(string pattern, string value) {
            return value.Contains(pattern);
        }

        public static bool UriContains(string pattern, Uri value) {
            return value.OriginalString.Contains(pattern);
        }

        public static string StringSubstring(string value, int start, int end) {
            try {
                // end of string
                if (end == -1) {
                    end = value.Length;
                }

                return value.Substring(start, end - start);
            }
            catch (ArgumentOutOfRangeException ex) {
                throw new XacmlInvalidDataTypeException("Substring function OutOfRange", ex);
            }

        }

        public static string UriSubstring(Uri value, int start, int end) {
            string substring = FunctionsProcessor.StringSubstring(value.OriginalString, start, end);

            Uri uri;
            if (!Uri.TryCreate(substring, UriKind.RelativeOrAbsolute, out uri)) {
                throw new XacmlInvalidDataTypeException("Wrong Uri format of Substring function result");
            }

            return substring;
        }

        #endregion

        public DelegateWrapper this[string value] {
            get {
                DelegateWrapper action;
                if (functions.TryGetValue(value, out action)) {
                    return action;
                }

                throw new ArgumentException("Unknown function name in match expression", nameof(value));
            }
        }

        internal static FunctionsProcessor Instance {
            get {
                if (processor == null) {
                    lock (locker) {
                        if (processor == null) {
                            processor = new FunctionsProcessor();
#if NET40 || NET45
                            var catalog = new DirectoryCatalog(AppDomain.CurrentDomain.BaseDirectory, "Abc.Xacml.*.dll");
                            var container = new CompositionContainer(catalog);
                            var exportedTypes = container.GetExportedValues<IFunctionsExtender>();
                            foreach (var t in exportedTypes) {
                                IDictionary<string, DelegateWrapper> extensionTypes = t.GetExtensionFunctions();
                                foreach (var extensionType in extensionTypes) {
                                    FunctionsProcessor.functions.Add(extensionType.Key, extensionType.Value);
                                }
                            }
#endif
                        }
                    }
                }

                return processor;
            }
        }
    }

    public static class TypeExtensions {
        public static MethodInfo GetGenericMethodInfo(this Type instance, string methodName, Type genericType, Func<Type, Type[]> typeGetter) {
            MemberInfo[] mis = instance.GetMember(methodName + "*", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static);

            if (mis.Length == 0) {
                throw new NotImplementedException("Unknown function");
            }

            Type U = ((MethodInfo)mis[0]).GetGenericArguments()[0]; // assume we know the class structure above, for simplicity.
            MethodInfo mInfo = instance.GetMethod(methodName, typeGetter(U));

            if (!mInfo.IsGenericMethod) {
                throw new NotImplementedException("Unknown generic method");
            }

            mInfo = mInfo.MakeGenericMethod(genericType);
            return mInfo;
        }
    }
}
