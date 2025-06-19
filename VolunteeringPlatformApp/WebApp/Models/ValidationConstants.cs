namespace WebApp.Models
{
    public static class ValidationConstants
    {
        // User validation constants
        public const int UsernameMinLength = 3;
        public const int UsernameMaxLength = 50;
        public const int PasswordMinLength = 8;
        public const int PasswordMaxLength = 256;
        public const int NameMinLength = 2;
        public const int NameMaxLength = 50;
        public const int EmailMaxLength = 256;

        // Project validation constants
        public const int ProjectTitleMaxLength = 100;
        public const int ProjectDescriptionMaxLength = 2000;

        // Common validation error messages
        public static class ErrorMessages
        {
            public const string Required = "{0} is required";
            public const string StringLength = "{0} must be between {2} and {1} characters long";
            public const string MaxLength = "{0} cannot exceed {1} characters";
            public const string MinLength = "{0} must be at least {1} characters long";
            public const string EmailFormat = "Please provide a valid email address";
            public const string UsernameLength = "Username must be between 3 and 50 characters long";
            public const string PasswordLength = "Password must be at least 8 characters long";
            public const string NameLength = "Name must be between 2 and 50 characters long";
        }
    }
} 