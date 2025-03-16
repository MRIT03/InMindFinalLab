using System;

namespace FinalLab.API.Dtos
{
    public class EventRequestDto
    {
        public string EventType { get; set; } // "TransactionUpdate" or "AccountUpdate"
        public long TransactionId { get; set; }
        public long AccountId { get; set; }
        public string OldStatus { get; set; }
        public string NewStatus { get; set; }
        public decimal OldBalance { get; set; }
        public decimal NewBalance { get; set; }
        public DateTime Timestamp { get; set; }
        public long FromAccountId { get; set; }
        public long ToAccountId { get; set; }
        public bool isReverted { get; set; }
        public int? ParentEventId { get; set; }
        public decimal Amount { get; set; }
    }
}