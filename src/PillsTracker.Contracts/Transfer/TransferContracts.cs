using PillsTracker.Contracts.Medication;
using PillsTracker.Contracts.Plans;
using PillsTracker.Contracts.TimeAnchors;

namespace PillsTracker.Contracts.Transfer;

public sealed record ExportDto(List<MedicationDto> Medications, List<TimeAnchorDto> Anchors, List<PlanDto> Plans);
public sealed record ImportRequest(ExportDto Data, string Mode);
