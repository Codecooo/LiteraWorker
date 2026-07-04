using System.ComponentModel.DataAnnotations;

namespace LiteraWorker.Core.Models.Enums;

public enum JobStatus
{
    Pending,
    Printing,
    Completed,
    Failed,
    Canceled,
    Queued,
    Uploading
}