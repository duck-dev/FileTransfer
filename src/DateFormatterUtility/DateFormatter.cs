using System;
using System.Threading.Tasks;
using FileTransfer.Enums;
using FileTransfer.Extensions;
using FileTransfer.Interfaces;

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
    
    // ReSharper disable once RedundantAssignment
    internal static void UpdateTime(WaitTime waitTimeType, IFormattableTime formattableTime, DateTime time, Action? action = null)
    {
        formattableTime.FormattedTimeString = FormatDate(time);
        action?.Invoke();

        if (waitTimeType == WaitTime.ConstantDate)
            return;
        
        TimeSpan waitTime;
        switch (waitTimeType)
        {
            case WaitTime.OneMinute:
                waitTime = TimeSpan.FromSeconds(60);
                break;
            case WaitTime.EndOfCurrentDay:
            case WaitTime.EndOfNextDay:
                DateTime tomorrow = DateTime.Now.Date.AddDays(1);
                waitTime = tomorrow.Subtract(DateTime.Now);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(waitTimeType), waitTimeType, null);
        }
        
        Task.Delay(waitTime).ContinueWith(x => { UpdateTime(waitTimeType.Next(), formattableTime, time); });
    }
}