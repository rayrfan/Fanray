using Fan.Data;
using System;
using System.Threading.Tasks;

namespace Fan.Medias
{
    public interface IMediaRepository : IRepository<Media>
    {
        Task<Media> GetAsync(int mediaId);
        Task<Media> GetAsync(string fileName, DateTimeOffset uploadedOn);
    }
}
