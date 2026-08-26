namespace Lamour.Domain.Enums;

/// <summary>
/// Đối tượng (payee/counterparty) type for Payment (Phiếu Chi) — polymorphic reference,
/// resolved against Supplier/Customer/Employee by Payment.PartnerId depending on this value.
/// </summary>
public enum PaymentPartnerType { Supplier, Customer, Employee }
