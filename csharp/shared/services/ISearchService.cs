using System.Collections.Generic;
using System.Threading.Tasks;

namespace goqur.shared.services
{
    public interface ISearchService
    {
        Task CreateIndex<T>();
        Task DeleteIndex();
        Task UploadDocs<T>(List<T> docs);
    }
}
