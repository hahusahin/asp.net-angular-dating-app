namespace API.Interfaces;

public interface IUnitOfWork
{
    public IUserRepository UserRepository { get; }
    public IMessageRepository MessageRepository { get; }
    public ILikesRepository LikesRepository { get; }
    public IPhotoRepository PhotoRepository { get; }
    Task<bool> Complete();
    bool HasChanges();
}
