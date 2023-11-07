// ----------------------------------------------------------------------------
// <copyright file="DayTimeDuration.cs" company="ABC Software Ltd">
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

    [TypeConverter(typeof(DayTimeDurationConverter))]
    public class DayTimeDuration : IEquatable<DayTimeDuration> {
        /// <summary>
        /// The regular expression used to validate the day time duration as a string value.
        /// </summary>
        private const string Pattern = @"[\-]?P([0-9]+D(T([0-9]+(H([0-9]+(M([0-9]+(\.[0-9]*)?S|\.[0-9]+S)?|(\.[0-9]*)?S)|(\.[0-9]*)?S)?|M([0-9](\.[0-9]*)?S|\.[0-9]+S)?|(\.[0-9]*)?S)|\.[0-9]+S))?|T([0-9]+(H([0-9]+(M([0-9]+(\.[0-9]*)?S|\.[0-9]+S)?|(\.[0-9]*)?S)|(\.[0-9]*)?S)?|M([0-9]+(\.[0-9]*)?S|\.[0-9]+S)?|(\.[0-9]*)?S)|\.[0-9]+S))";

        /// <summary>
        /// The regular expression used to match the day time duration and extract some values.
        /// </summary>
        private const string PatternMatch = @"(?<n>[\-]?)P((?<d>(\d+|\.\d+|\d+\.\d+))D)?T((?<h>(\d+|\.\d+|\d+\.\d+))H)?((?<m>(\d+|\.\d+|\d+\.\d+))M)?((?<s>(\d+|\.\d+|\d+\.\d+))S)?";

        /// <summary>
        /// The original value found in the document.
        /// </summary>
        private readonly string durationValue;

        private readonly TimeSpan duration;

        /// <summary>
        /// Whether is a negative duration.
        /// </summary>
        private readonly bool negative;

        /// <summary>
        /// Initializes a new instance of the <see cref="DayTimeDuration"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <exception cref="System.FormatException">
        /// Wrong DayTime duration value.
        /// </exception>
        public DayTimeDuration(string value) {
            durationValue = value;
            Regex re = new Regex(Pattern);
            Match m = re.Match(value);
            if (m.Success) {
                re = new Regex(PatternMatch);
                m = re.Match(value);
                if (m.Success) {
                    this.duration = new TimeSpan(
                            int.Parse(string.IsNullOrEmpty(m.Groups["d"].Value) ? "0" : m.Groups["d"].Value, CultureInfo.InvariantCulture),
                            int.Parse(string.IsNullOrEmpty(m.Groups["h"].Value) ? "0" : m.Groups["h"].Value, CultureInfo.InvariantCulture),
                            int.Parse(string.IsNullOrEmpty(m.Groups["m"].Value) ? "0" : m.Groups["m"].Value, CultureInfo.InvariantCulture),
                            int.Parse(string.IsNullOrEmpty(m.Groups["s"].Value) ? "0" : m.Groups["s"].Value, CultureInfo.InvariantCulture));

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

        /// <summary>Adds a specified date and time duration to a specified date and time, yielding a new date and time.</summary>
        /// <param name="dateTime">The date and time value to add. </param>
        /// <param name="duration">The date and time duration to add. </param>
        /// <returns>An object that is the sum of the values of <paramref name="dateTime" /> and <paramref name="duration" />.</returns>
        public static DateTime operator +(DateTime dateTime, DayTimeDuration duration) {
            if (!duration.negative) {
                return dateTime.Add(duration.duration);
            }
            else {
                return dateTime.Add(duration.duration.Negate());
            }
        }

        /// <summary>Subtracts a specified date and time duration from another specified date and time and returns a time interval.</summary>
        /// <param name="dateTime">The date and time value to subtract from. </param>
        /// <param name="duration">The date and time duration to subtract. </param>        
        /// <returns>The time interval between <paramref name="dateTime" /> and <paramref name="duration" />; that is, <paramref name="dateTime" /> minus <paramref name="duration" />.</returns>
        public static DateTime operator -(DateTime dateTime, DayTimeDuration duration) {
            if (!duration.negative) {
                return dateTime.Add(duration.duration.Negate());
            }
            else {
                return dateTime.Add(duration.duration);
            }
        }

        /// <inheritdoc/>
        public bool Equals(DayTimeDuration other) {
            return !(this.negative ^ other.negative) && (TimeSpan.Equals(this.duration, other.duration));
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) {
            DayTimeDuration dt = obj as DayTimeDuration;
            if (dt == null) {
                return false;
            }

            return this.Equals(dt);
        }

        /// <inheritdoc/>
        public override int GetHashCode() {
            // Overflow is fine, just wrap
            unchecked {
                int hash = 17;
                // Suitable nullity checks etc, of course :)
                hash = hash * 23 + this.negative.GetHashCode();
                hash = hash * 23 + this.duration.GetHashCode();
                return hash;
            }
        }

        /// <inheritdoc/>
        public override string ToString() {
            return this.durationValue;
        }
    }
}