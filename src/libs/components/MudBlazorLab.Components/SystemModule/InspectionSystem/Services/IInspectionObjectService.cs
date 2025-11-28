using HmiInspection.Models;
using InspectionSystem.Models;

namespace InspectionSystem.Services;

public interface IInspectionObjectService
{
    Task<List<InspectionFormObject>> GetObjectsAsync(Guid formSysid);
    Task<List<InspectionObjectView>> GetObjectViewsAsync(InspectionForm form);
    Task<InspectionFormObject> CreateObjectAsync(InspectionForm form, string objectType, string objectName);
}

