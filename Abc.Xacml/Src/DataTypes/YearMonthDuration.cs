// ----------------------------------------------------------------------------
// <copyright file="YearMonthDuration.cs" company="ABC Software Ltd">
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
    using System.Globalization;
    using System.Text.RegularExpressions;

    [TypeConverter(typeof(YearMonthDurationConverter))]
    public class YearMonthDuration : IEquatable<YearMonthDuration> {
        /// <summary>
        /// The regular expression used to validate the year month duration as a string value.
        /// </summary>
        private const string Pattern = @"[\-]?P[0-9]+(Y([0-9]+M)?|M)";

        /// <summary>
        /// The regular expression used to match the year month duration and extract some values.
        /// </summary>
        private const string PatternMatch = @"(?<n>[\-]?)P((?<y>\d+)Y)?((?<m>\d+)M)?";

        private readonly string durationValue;

        /// <summary>
        /// Whether this is a negative duration.
        /// </summary>
        private readonly bool negative;

        private readonly int years;
        private readonly int months;

        public YearMonthDuration(string value) {
            this.durationValue = value;
            Regex re = new Regex(Pattern);
            Match m = re.Match(value);
            if (m.Success) {
                re = new Regex(PatternMatch);
                m = re.Match(value);
                if (m.Success) {
                    this.years = int.Parse(string.IsNullOrEmpty(m.Groups["y"].Value) ? "0" : m.Groups["y"].Value, CultureInfo.InvariantCulture);
                    this.months = int.Parse(string.IsNullOrEmpty(m.Groups["m"].Value) ? "0" : m.Groups["m"].Value, CultureInfo.InvariantCulture);
                    this.negative = (m.Groups["n"].Value == "-");
                }
                else {
                    throw new FormatException("Wrong YearMonth duration value: " + value);
                }
            }
            else {
                throw new FormatException("Wrong YearMonth duration value: " + value);
            }
        }
        
        #region Operators

        public static DateTime operator +(DateTime dateTime, YearMonthDuration duration) {
            if (!duration.negative) {
                return dateTime.AddYears(duration.years).AddMonths(duration.months);
            }
            else {
                return dateTime.AddYears(duration.years * -1).AddMonths(duration.months * -1);
            }
        }

        public static DateTime operator -(DateTime dateTime, YearMonthDuration duration) {
            if (!duration.negative) {
                return dateTime.AddYears(duration.years * -1).AddMonths(duration.months * -1);
            }
            else {
                return dateTime.AddYears(duration.years).AddMonths(duration.months);
            }
        }

        public static Date operator +(Date date, YearMonthDuration duration) {
            if (!duration.negative) {
                return date.AddYearsMonths(duration.years, duration.months);
            }
            else {
                return date.AddYearsMonths(duration.years * -1, duration.months * -1);
            }
        }

        #endregion

        public static Date operator -(Date date, YearMonthDuration duration) {
            if (!duration.negative) {
                return date.AddYearsMonths(duration.years * -1, duration.months * -1);
            }
            else {
                return date.AddYearsMonths(duration.years, duration.months);
            }
        }

        public bool Equals(YearMonthDuration other) {
            return !(this.negative ^ other.negative) && (this.years == other.years) && (this.months == other.months);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) {
            YearMonthDuration dt = obj as YearMonthDuration;
            if (dt != null) {
                return this.Equals(dt);
            }
            else {
                return false;
            }
        }

        /// <inheritdoc/>
        public override int GetHashCode() {
            // Overflow is fine, just wrap
            unchecked {
                int hash = 17;
                // Suitable nullity checks etc, of course :)
                hash = hash * 23 + this.negative.GetHashCode();
                hash = hash * 23 + this.years.GetHashCode();
                hash = hash * 23 + this.months.GetHashCode();
                return hash;
            }
        }

        /// <inheritdoc/>
        public override string ToString() {
            return this.durationValue;
        }
    }
}