using System.Collections.Generic;
using System.Threading.Tasks;
using HmiInspection.Models;
using global::InspectionSystem.Models;

namespace InspectionSystem.Services;

public class InspectionFacade : IInspectionFacade
{
    readonly IInspectionFormService _formSvc;
    readonly IInspectionDetailService _detailSvc;

    public InspectionFacade(IInspectionFormService formSvc, IInspectionDetailService detailSvc)
    { _formSvc = formSvc; _detailSvc = detailSvc; }

    public async Task<InspectionForm> CreateFormFlowAsync(InspectionForm form, bool autoStartAndCreateObject)
    {
        await _formSvc.CreateFormAsync(form);
        if (autoStartAndCreateObject)
        {
            await _formSvc.StartFormAsync(form);
        }
        await _formSvc.CreateObjectsFromTemplateAsync(form);
        return form;
    }

    public async Task SaveDetailFlowAsync(InspectionFormObject obj, string? sampleBatchNo, List<InspectionEntryRow> rows)
    {
        await _detailSvc.SaveBatchAsync(obj, sampleBatchNo, rows);
    }
}

