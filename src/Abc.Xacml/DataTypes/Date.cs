// ----------------------------------------------------------------------------
// <copyright file="Date.cs" company="ABC Software Ltd">
//    Copyright © 2018 ABC Software Ltd. All rights reserved.
//
//    This library is free software; you can redistribute it and/or.
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
 
namespace Abc.Xacml.DataTypes {
    using System;
    using System.ComponentModel;
    using System.Xml;

    [TypeConverter(typeof(DateConverter))]
    [ImmutableObject(true)]
    public class Date : IComparable<Date>, IEquatable<Date> {
        private readonly DateTime date;

        public Date(string date) {
            this.date = XmlConvert.ToDateTime(date, XmlDateTimeSerializationMode.Utc).Date;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Date"/> class.
        /// </summary>
        /// <param name="date">The date.</param>
        private Date(DateTime date) {
            this.date = date.Date;
        }

        /// <inheritdoc/>
        public int CompareTo(Date other) {
            return this.date.CompareTo(other.date);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) {
            Date dt = obj as Date;
            if (dt != null) {
                return this.Equals(dt);
            }
            else {
                return false;
            }
        }

        /// <inheritdoc/>
        public override int GetHashCode() {
            return this.date.GetHashCode();
        }

        /// <inheritdoc/>
        public override string ToString() {
            return this.date.ToUniversalTime().ToString("yyyy-MM-dd");
        }

        public bool Equals(Date other) {
            return this.date.Equals(other.date);
        }

        public Date AddYearsMonths(int years, int months) {
            return new Date(this.date.AddYears(years).AddMonths(months));
        }
    }
}