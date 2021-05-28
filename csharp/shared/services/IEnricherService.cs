using System.Collections.Generic;
using System.Threading.Tasks;
using goqur.shared.models;

namespace goqur.shared.services
{
    public interface IEnricherService
    {
        Task<List<Ayah>> Translate(List<Ayah> ayahs);
    }
}
