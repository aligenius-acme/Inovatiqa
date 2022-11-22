namespace Inovatiqa.Services.Customers
{
    public class ChangePasswordRequest
    {
        public string Email { get; set; }

        public bool ValidateRequest { get; set; }

        public int NewPasswordFormatId { get; set; }

        public string NewPassword { get; set; }

        public string OldPassword { get; set; }

        public string HashedPasswordFormat { get; set; }

        public ChangePasswordRequest(string email, bool validateRequest,
            int newPasswordFormatId, string newPassword, string oldPassword = "",
            string hashedPasswordFormat = null)
        {
            Email = email;
            ValidateRequest = validateRequest;
            NewPasswordFormatId = newPasswordFormatId;
            NewPassword = newPassword;
            OldPassword = oldPassword;
            HashedPasswordFormat = hashedPasswordFormat;
        }
    }
}