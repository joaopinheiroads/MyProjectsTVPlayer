using Cardápio.Infra.Model;

namespace Cardápio.Infra.Interfaces.Repositories
{
    public interface IProdutoThumbnailRepo : IRepo<ThumbnailProduct>
    {
        Task<ThumbnailProduct> GetThumbnailByID(int thumbnailID);
    }
}
