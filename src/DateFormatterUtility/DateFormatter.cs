using System;

namespace FileTransfer.DateFormatterUtility;

public static class DateFormatter
{
    public static string FormatDate(DateTime dateTime)
    {
        DateTime now = DateTime.Now;
        if (now.Subtract(dateTime) < TimeSpan.FromSeconds(60))
            return "Just now";
        if (dateTime.Day == now.Day && dateTime.Month == now.Month && dateTime.Year == now.Year)
            return dateTime.ToShortTimeString();

        DateTime yesterday = now.Subtract(TimeSpan.FromHours(24));
        if (dateTime.Day == yesterday.Day && dateTime.Month == yesterday.Month && dateTime.Year == yesterday.Year)
            return $"Yesterday, {dateTime.ToShortTimeString()}";
        
        return $"{dateTime.ToShortDateString()}, {dateTime.ToShortTimeString()}";
    }
}