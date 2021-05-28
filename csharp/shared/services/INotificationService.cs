using System.Collections.Generic;
using System.Threading.Tasks;
using goqur.shared.models;

namespace goqur.shared.services
{
    public interface INotificationService
    {
        Task EnqueueAyahsList(List<Ayah> ayahs);
    }
}
