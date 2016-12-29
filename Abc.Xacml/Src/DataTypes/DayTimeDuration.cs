// ----------------------------------------------------------------------------
// <copyright file="DayTimeDuration.cs" company="ABC Software Ltd">
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

namespace Abc.Xacml.DataTypes {
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Text.RegularExpressions;

    [TypeConverter(typeof(DayTimeDurationConverter))]
    public class DayTimeDuration : IEquatable<DayTimeDuration> {

        /// <summary>
        /// The regular expression used to validate the day time duration as a string value.
        /// </summary>
        private const string PATTERN = @"[\-]?P([0-9]+D(T([0-9]+(H([0-9]+(M([0-9]+(\.[0-9]*)?S|\.[0-9]+S)?|(\.[0-9]*)?S)|(\.[0-9]*)?S)?|M([0-9](\.[0-9]*)?S|\.[0-9]+S)?|(\.[0-9]*)?S)|\.[0-9]+S))?|T([0-9]+(H([0-9]+(M([0-9]+(\.[0-9]*)?S|\.[0-9]+S)?|(\.[0-9]*)?S)|(\.[0-9]*)?S)?|M([0-9]+(\.[0-9]*)?S|\.[0-9]+S)?|(\.[0-9]*)?S)|\.[0-9]+S))";

        /// <summary>
        /// The regular expression used to match the day time duration and extract some values.
        /// </summary>
        private const string PATTERN_MATCH = @"(?<n>[\-]?)P((?<d>(\d+|\.\d+|\d+\.\d+))D)?T((?<h>(\d+|\.\d+|\d+\.\d+))H)?((?<m>(\d+|\.\d+|\d+\.\d+))M)?((?<s>(\d+|\.\d+|\d+\.\d+))S)?";

        /// <summary>
        /// The original value found in the document.
        /// </summary>
        private readonly string durationValue;

        private readonly TimeSpan duration;

        /// <summary>
        /// Whether is a negative duration.
        /// </summary>
        private readonly bool negative;

        public DayTimeDuration(string value) {
            durationValue = value;
            Regex re = new Regex(PATTERN);
            Match m = re.Match(value);
            if (m.Success) {
                re = new Regex(PATTERN_MATCH);
                m = re.Match(value);
                if (m.Success) {
                    this.duration = new TimeSpan(
                            int.Parse(string.IsNullOrEmpty(m.Groups["d"].Value) ? "0" : m.Groups["d"].Value, CultureInfo.InvariantCulture),
                            int.Parse(string.IsNullOrEmpty(m.Groups["h"].Value) ? "0" : m.Groups["h"].Value, CultureInfo.InvariantCulture),
                            int.Parse(string.IsNullOrEmpty(m.Groups["m"].Value) ? "0" : m.Groups["m"].Value, CultureInfo.InvariantCulture),
                            int.Parse(string.IsNullOrEmpty(m.Groups["s"].Value) ? "0" : m.Groups["s"].Value, CultureInfo.InvariantCulture)
                        );
                    negative = (m.Groups["n"].Value == "-");
                }
                else {
                    throw new FormatException("Wrong DayTime duration value: " + value);
                }
            }
            else {
                throw new FormatException("Wrong DayTime duration value: " + value);
            }
        }

        public bool Equals(DayTimeDuration other) {
            return !(this.negative ^ other.negative) && (TimeSpan.Equals(this.duration, other.duration));
        }

        public override bool Equals(object obj) {
            DayTimeDuration dt = obj as DayTimeDuration;
            if (dt == null) {
                return false;
            }

            return this.Equals(dt);
        }

        public override int GetHashCode() {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                // Suitable nullity checks etc, of course :)
                hash = hash * 23 + this.negative.GetHashCode();
                hash = hash * 23 + this.duration.GetHashCode();
                return hash;
            }
        }

        public override string ToString() {
            return this.durationValue;
        }

        public static DateTime operator +(DateTime dateTime, DayTimeDuration duration) {
            if (!duration.negative) {
                return dateTime.Add(duration.duration);
            }
            else {
                return dateTime.Add(duration.duration.Negate());
            }
        }

        public static DateTime operator -(DateTime dateTime, DayTimeDuration duration) {
            if (!duration.negative) {
                return dateTime.Add(duration.duration.Negate());
            }
            else {
                return dateTime.Add(duration.duration);
            }
        }
    }

    public class DayTimeDurationConverter : TypeConverter {
        /// <inheritdoc/>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
            if (sourceType == typeof(string)) {
                return true;
            }

            return base.CanConvertFrom(context, sourceType);
        }

        /// <inheritdoc/>
        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value) {
            if (value is string) {
                return new DayTimeDuration(value.ToString());
            }

            return base.ConvertFrom(context, culture, value);
        }
    }
}