using Fan.Blogs.Models;
using Fan.Data;
using System;
using System.Threading.Tasks;

namespace Fan.Blogs.Data
{
    public interface IMediaRepository : IRepository<Media>
    {
        Task<Media> GetAsync(int mediaId);
        Task<Media> GetAsync(string fileName, DateTimeOffset uploadedOn);
    }
}
