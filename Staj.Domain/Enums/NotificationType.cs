// Staj.Domain/Enums/NotificationType.cs
namespace Staj.Domain.Enums;

// Sistem bildirim tipleri
public enum NotificationType
{
    // Bir ilana yeni başvuru geldi (hedef: Client)
    NewApplication = 1,

    // Freelancer'a doğrudan teklif geldi (hedef: Freelancer)
    NewDirectOffer = 2,

    // Teklif durumu değişti — Accepted / Rejected / Withdrawn (hedef: karşı taraf)
    ProposalStatusChanged = 3,

    // Yeni birebir mesaj (hedef: alıcı)
    NewMessage = 4,

    // Yeni değerlendirme yazıldı (hedef: değerlendirilen)
    NewReview = 5
}