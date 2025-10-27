using citi_core.Dto;
using FluentValidation.TestHelper;
using Xunit;

namespace citi_core.tests.UnitTests.Validators
{
    public class CreateUserValidatorTests
    {
        private readonly CreateUserValidator _validator;

        public CreateUserValidatorTests()
        {
            _validator = new CreateUserValidator();
        }

        [Fact]
        public void Should_HaveError_WhenFullNameIsEmpty()
        {
            var dto = new CreateUserDto { FullName = "" };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.FullName)
                  .WithErrorMessage("Full Name is required.");
        }

        [Fact]
        public void Should_HaveError_WhenEmailIsInvalid()
        {
            var dto = new CreateUserDto { Email = "invalidemail" };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Email)
                  .WithErrorMessage("Invalid email format.");
        }

        [Fact]
        public void Should_HaveError_WhenPasswordTooShort()
        {
            var dto = new CreateUserDto { Password = "Ab1!" };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Password)
                  .WithErrorMessage("Password must be at least 8 characters.");
        }

        [Fact]
        public void Should_HaveError_WhenPasswordMissingUppercase()
        {
            var dto = new CreateUserDto { Password = "password1!" };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Password)
                  .WithErrorMessage("Password must contain at least one uppercase letter.");
        }

        [Fact]
        public void Should_HaveError_WhenPasswordMissingLowercase()
        {
            var dto = new CreateUserDto { Password = "PASSWORD1!" };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Password)
                  .WithErrorMessage("Password must contain at least one lowercase letter.");
        }

        [Fact]
        public void Should_HaveError_WhenPasswordMissingNumber()
        {
            var dto = new CreateUserDto { Password = "Password!" };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Password)
                  .WithErrorMessage("Password must contain at least one number.");
        }

        [Fact]
        public void Should_HaveError_WhenPasswordMissingSpecialCharacter()
        {
            var dto = new CreateUserDto { Password = "Password1" };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Password)
                  .WithErrorMessage("Password must contain at least one special character.");
        }

        [Fact]
        public void Should_HaveError_WhenPhoneIsInvalid()
        {
            var dto = new CreateUserDto { PhoneNumber = "abc123" };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.PhoneNumber)
                  .WithErrorMessage("Phone number must be a valid international number and between 10 to 15 digits.");
        }

        [Fact]
        public void Should_NotHaveErrors_WhenAllFieldsValid()
        {
            var dto = new CreateUserDto
            {
                FullName = "Test User",
                Email = "test@example.com",
                Password = "StrongPass1!",
                PhoneNumber = "+12345678901"
            };

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
