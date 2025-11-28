using InspectionSystem.Services;
using Microsoft.AspNetCore.Components;

namespace MudBlazorLab.Components.SystemModule.InspectionSystem.Components.IncomingInspection;

public partial class IncomingDocList : ComponentBase {
  [Inject] IInspectionConfigService ConfigSvc { get; set; }
  [Inject] IInspectionFormService FormSvc { get; set; }
  [Inject] IInspectionFacade Facade { get; set; }

  List<global::InspectionSystem.Models.ErpTicket> _erpTickets = new();
  HashSet<global::InspectionSystem.Models.ErpTicket> Tickets = new();
  InspectionForm? _currentForm;
  bool _openObjectsPanel;

  async Task OnNewDocAsync() {
    _erpTickets = (await ConfigSvc.GetErpTicketNosAsync()).ToList();
    Tickets = new();
  }

  bool ValidateExtra() => Tickets.Any();

  void MapExtraToForm(InspectionForm form) {
    form.ProductName = string.Empty;
    form.TicketNo = Tickets.FirstOrDefault()?.TicketNo;
  }

  Task OpenObjectsPanel(InspectionForm doc) {
    _currentForm = doc;
    _openObjectsPanel = true;
    return Task.CompletedTask;
  }
}
