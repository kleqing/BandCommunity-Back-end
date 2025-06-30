namespace BandCommunity.Shared.Constant;

public static class LoginConstant
{
    //* Error Messages
    public static string LoginFailed = "Login failed. Please check your email and password.";
    public static string AccountNotFound = "Account not found!";
    public static string InvalidPassword = "Invalid password!";
    public static string EmailNotConfirmed = "Email not confirmed!";
    public static string EmailAlreadyConfirmed = "Email already confirmed!";
    public static string ResetPasswordFailed = "Reset password failed!";
    public static string CreateAccountFailed = "Create account failed! Please try again.";
    public static string ClaimPrincipalFailed = "Claim principal failed!";
    public static string RefreshTokenFailed = "Refresh token created failed! Please try again.";
    public static string EmailExists = "Email already exists!";
    public static string InvalidToken = "Invalid token!";
    public static string UsernameExists = "Username already exists!";
    
    //* Success Messages
    public static string LoginSuccess = "Login successful!";
    public static string AccountCreated = "Account created successfully!";
    public static string SendSuccess = "Send successfully!";
    public static string PasswordResetSuccess = "Password reset successfully!";
    public static string ValidToken = "Valid token!";
    public static string LogoutSuccess = "Logout successful!";
}