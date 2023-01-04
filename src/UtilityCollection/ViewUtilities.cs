using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Threading;
using FileTransfer.Enums;

namespace FileTransfer.UtilityCollection;

internal static partial class Utilities
{
    internal static void LimitAutoSize(Grid grid, Layoutable elementToResize, GridOrientation orientation,
        int autoIndex, int fixedIndex, Action? additionalAction = null)
    {
        additionalAction?.Invoke();

        grid.PropertyChanged += async (o, args) =>
        {
            if (args.Property != Visual.BoundsProperty) 
                return;
            
            if (orientation == GridOrientation.Column)
                await UpdateColumns(grid, elementToResize, autoIndex, fixedIndex);
            else
                await UpdateRows(grid, elementToResize, autoIndex, fixedIndex);
        };
    }
    
    internal static async Task UpdateColumns(Grid grid, Layoutable elementToResize, int autoIndex, int fixedIndex, 
        bool callFromUIThread = false)
    {
        ColumnDefinition autoColumn = grid.ColumnDefinitions[autoIndex]; // 1* at the beginning
        ColumnDefinition fixedColumn = grid.ColumnDefinitions[fixedIndex]; // Auto at the beginning
        
        if (callFromUIThread)
            await Dispatcher.UIThread.InvokeAsync(() => ResetColumns(autoColumn, fixedColumn));
        else
            ResetColumns(autoColumn, fixedColumn);

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            elementToResize.MaxWidth = autoColumn.ActualWidth;
            // Switch ColumnDefinitions into the correct/desired order
            autoColumn.Width = GridLength.Auto;
            fixedColumn.Width = new GridLength(1, GridUnitType.Star);
        });
    }

    internal static async Task UpdateRows(Grid grid, Layoutable elementToResize, int autoIndex, int fixedIndex, 
        bool callFromUIThread = false)
    {
        RowDefinition autoRow = grid.RowDefinitions[autoIndex];
        RowDefinition fixedRow = grid.RowDefinitions[fixedIndex];
        
        if (callFromUIThread)
            await Dispatcher.UIThread.InvokeAsync(() => ResetRows(autoRow, fixedRow));
        else
            ResetRows(autoRow, fixedRow);

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            elementToResize.MaxHeight = autoRow.ActualHeight;
            // Switch ColumnDefinitions into the correct/desired order
            autoRow.Height = GridLength.Auto;
            fixedRow.Height = new GridLength(1, GridUnitType.Star);
        });
    }
    
    private static void ResetColumns(ColumnDefinition autoColumn, ColumnDefinition fixedColumn)
    {
        fixedColumn.Width = GridLength.Auto;
        autoColumn.Width = new GridLength(1, GridUnitType.Star);
    }

    private static void ResetRows(RowDefinition autoRow, RowDefinition fixedRow)
    {
        fixedRow.Height = GridLength.Auto;
        autoRow.Height = new GridLength(1, GridUnitType.Star);
    }
}