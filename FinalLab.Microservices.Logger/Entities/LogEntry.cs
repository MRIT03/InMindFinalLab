using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace FinalLab.Microservices.Logger.Entities;
public class LogEntry
{
    public long Id { get; set; }
    public Guid RequestId { get; set; }
    [Column(TypeName = "jsonb")]
    public string RequestObject { get; set; }
    public string RouteURL { get; set; }
    public DateTime Timestamp { get; set; }
    
    [NotMapped]
    public Dictionary<string, object> RequestData
    {
        get => JsonConvert.DeserializeObject<Dictionary<string, object>>(RequestObject);
        set => RequestObject = JsonConvert.SerializeObject(value);
    }
}