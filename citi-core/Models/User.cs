using citi_core.Enums;
using System;
using System.Collections.Generic;

namespace citi_core.Models
{
    public class User : BaseEntity
    {
        public Guid UserId { get; set; } = Guid.NewGuid();
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public KycStatus KycStatus { get; set; } = KycStatus.Pending;
        public bool BiometricEnabled { get; set; } = false;
        public bool EmailVerified { get; set; } = false;
        public bool PhoneVerified { get; set; } = false;
        public DateTime? LastLoginAt { get; set; }
        public UserPreferences? UserPreferences { get; set; }
        public ICollection<Account> Accounts { get; set; } = new List<Account>();
        public ICollection<Card> Cards { get; set; } = new List<Card>();
    }
}