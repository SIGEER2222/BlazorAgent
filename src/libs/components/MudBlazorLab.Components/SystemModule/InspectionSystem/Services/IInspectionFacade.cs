using System.Collections.Generic;
using System.Threading.Tasks;
using HmiInspection.Models;
using global::InspectionSystem.Models;

namespace InspectionSystem.Services;

public interface IInspectionFacade
{
    Task<InspectionForm> CreateFormFlowAsync(InspectionForm form, bool autoStartAndCreateObject);
    Task SaveDetailFlowAsync(InspectionFormObject obj, string? sampleBatchNo, List<InspectionEntryRow> rows);
}

