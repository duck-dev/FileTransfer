namespace FileTransfer.CustomControls.SelectionTypeControls;

internal class SelectionTypeOne<T> : SelectionTypeBase<T> where T : class
{
    internal SelectionTypeOne(T file) : base(file) { }
}