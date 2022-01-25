using System;
using System.Collections.Generic;

namespace Api.Common
{
    public static class GeneralExtensions
    {
        /// <summary>
        /// Replaces middle characters (not the first, nor the last) of a string with asteriscs.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public static string HideMiddleChars(this string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            if (text.Length < 3)
                return text;

            return text.Substring(0, 1) + new string('*', text.Length - 2) + text.Substring(text.Length - 1, 1);
        }

        public static void AddRange<T>(this ICollection<T> sourceList, ICollection<T> collection)
        {
            if (collection is object)
            {
                foreach (T obj in collection)
                    sourceList.Add(obj);
            }
        }


        public static string Right(this string text, int count)
        {
            if (string.IsNullOrEmpty(text))
                return text;
            if (count > text.Length)
                count = text.Length;
            if (count < 0)
                count = 0;
            return text.Substring(text.Length - count, count);
        }

        /// <summary>
        /// Returns a string composed of the literal 'GMT' plus the time offset in hours+minutes
        /// </summary>
        /// <param name="timeOffsetInMinutes"></param>
        /// <returns></returns>
        public static string GetGMTStringFromTimeoffsetInMinutes(this int timeOffsetInMinutes)
        {
            timeOffsetInMinutes = AdjustTimeOffset(timeOffsetInMinutes);
            if (timeOffsetInMinutes == 0)
            {
                return " GMT";
            }

            int hours = Math.Abs(timeOffsetInMinutes / 60);
            int minutes = Math.Abs(timeOffsetInMinutes) - hours * 60;
            return $" GTM{(timeOffsetInMinutes < 0 ? "+" : "-")}{("00" + hours).Right(2)}:{("00" + minutes).Right(2)}";
        }

        /// <summary>
        /// Rounds a time offset sent by the client (in minutes) to a multiple of 30
        /// </summary>
        /// <param name="timeOffsetInMinutes"></param>
        /// <returns></returns>
        public static int AdjustTimeOffset(this int timeOffsetInMinutes)
        {
            if (timeOffsetInMinutes == 0)
            {
                return 0;
            }

            if (timeOffsetInMinutes > 720)
            {
                timeOffsetInMinutes = 720;
            }
            else if (timeOffsetInMinutes < -720)
            {
                timeOffsetInMinutes = -720;
            }

            int n = timeOffsetInMinutes / 30;
            return n * 30;
        }
    }
}