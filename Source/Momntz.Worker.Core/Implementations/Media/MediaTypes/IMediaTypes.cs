namespace Momntz.Worker.Core.Implementations.Media.MediaTypes
{
    public interface IMedia
    {
        string Media { get; }

        void Process(MediaItem message);
    }
}
