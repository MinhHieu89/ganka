namespace Clinical.Domain.Enums;

/// <summary>
/// Types of technician orders within the clinical workflow.
/// PreExam: Standard pre-examination order (refraction, VA, IOP).
/// AdditionalExam: Doctor-requested additional technician work.
/// </summary>
public enum TechnicianOrderType
{
    PreExam,
    AdditionalExam
}
