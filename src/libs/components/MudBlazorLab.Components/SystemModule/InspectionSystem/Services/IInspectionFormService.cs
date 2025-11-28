using InspectionSystem.Data;
using HmiInspection.Models;
using InspectionSystem.Models;

namespace InspectionSystem.Services;

public interface IInspectionFormService
{
    Task<string> GenerateFormNoAsync(FormTable formType);
    Task CreateFormAsync(InspectionForm form);
    Task StartFormAsync(InspectionForm form);
    Task EndFormAsync(InspectionForm form);
    Task CreateObjectsFromTemplateAsync(InspectionForm form);
}

