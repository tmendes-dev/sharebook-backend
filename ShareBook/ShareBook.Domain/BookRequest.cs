using ShareBook.Domain.Common;
using ShareBook.Domain.Enums;
using System;

namespace ShareBook.Domain
{
    public class BookRequest : BaseEntity
    {
        public Guid BookRequestedId { get; set; }
        public Book BookRequested { get; set; }
        public User DonorUser { get; set; }
        public User RequestUser { get; set; }
        public Guid RequestUserId { get; set; }
        public string NickName { get; set; }
        public DonationStatus Status { get; private set; } = DonationStatus.WaitingAction;
        public EmailStatus RequestBookEmailStatus { get; private set; } = EmailStatus.Queued;
        public string DonorNote { get; set; } // motivo do doador ter escolhido.
        public string Reason { get; set; } // justificativa do interessado.

        public void UpdateBookRequest(DonationStatus status, string note)
        {
            this.Status = status;
            this.DonorNote = note;
        }

        public void UpdateRequestEmailStatus(EmailStatus status)
        {
            this.RequestBookEmailStatus = status;
        }
    }
}