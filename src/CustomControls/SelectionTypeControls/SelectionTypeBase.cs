namespace FileTransfer.CustomControls.SelectionTypeControls;

internal abstract class SelectionTypeBase<T> where T : class
{
    protected SelectionTypeBase(T reference) => Reference = reference;

    protected internal T Reference { get; }
}