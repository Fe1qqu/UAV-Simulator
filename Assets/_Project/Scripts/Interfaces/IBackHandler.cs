public interface IBackHandler
{
    /// <summary>
    /// Returns TRUE if this handler consumed the cancel/back event.
    /// FALSE → event should pass to next handler.
    /// </summary>
    bool OnBack();
}
