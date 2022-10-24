using Avalonia;
using Avalonia.Media;
using FileTransfer.UtilityCollection;

namespace FileTransfer.Resources;

public static partial class Resources
{
    private const int StyleIndex = 2;
    
    // SolidColorBrush

    public static readonly SolidColorBrush FullyTransparentBrush =
        Utilities.GetResourceFromStyle<SolidColorBrush, Application>(Application.Current, "FullyTransparent", StyleIndex)
        ?? new SolidColorBrush(Color.Parse("#0000FFFF"));
    public static readonly SolidColorBrush AppPurpleBrush =
        Utilities.GetResourceFromStyle<SolidColorBrush, Application>(Application.Current, "AppPurple", StyleIndex)
        ?? new SolidColorBrush(Color.Parse("#660099"));
    public static readonly SolidColorBrush WhiteBrush =
        Utilities.GetResourceFromStyle<SolidColorBrush, Application>(Application.Current, "White", StyleIndex)
        ?? new SolidColorBrush(Color.Parse("#FFFFFF"));
    public static readonly SolidColorBrush BlackBrush =
        Utilities.GetResourceFromStyle<SolidColorBrush, Application>(Application.Current, "Black", StyleIndex)
        ?? new SolidColorBrush(Color.Parse("#0A0A0A"));
    
    public static readonly SolidColorBrush MattBlueBrush = 
        Utilities.GetResourceFromStyle<SolidColorBrush, Application>(Application.Current, "MattBlue", StyleIndex)
        ?? new SolidColorBrush(Color.Parse("#91A1E8"));
    public static readonly SolidColorBrush MainRedBrush = 
        Utilities.GetResourceFromStyle<SolidColorBrush, Application>(Application.Current, "MainRed", StyleIndex)
        ?? new SolidColorBrush(Color.Parse("#FF0000"));

    public static readonly SolidColorBrush MainGreyBrush =
        Utilities.GetResourceFromStyle<SolidColorBrush, Application>(Application.Current, "MainGrey", StyleIndex)
        ?? new SolidColorBrush(Color.Parse("#808080"));
    
    // Color

    public static readonly Color FullyTransparent = FullyTransparentBrush.Color;
    public static readonly Color AppPurple = AppPurpleBrush.Color;
    public static readonly Color White = WhiteBrush.Color;
    public static readonly Color Black = BlackBrush.Color;

    public static readonly Color MattBlue = MattBlueBrush.Color;
    public static readonly Color MainRed = MainRedBrush.Color;
    
    public static readonly Color MainGrey = MainGreyBrush.Color;
}