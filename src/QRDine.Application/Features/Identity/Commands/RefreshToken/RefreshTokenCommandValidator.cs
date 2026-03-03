namespace QRDine.Application.Features.Identity.Commands.RefreshToken
{
    public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
    {
        public RefreshTokenCommandValidator()
        {
            RuleFor(x => x.refreshToken)
                .NotEmpty().WithMessage("Refresh token is required.")
                .MinimumLength(20).WithMessage("Invalid refresh token.");
        }
    }
}
