using FluentValidation;

namespace ASP_Rest_API.DTO
{
    public class DocumentItemDtoValidator : AbstractValidator<DocumentItemDto>
    {
        public DocumentItemDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("The task name cannot be empty.")
                .MaximumLength(100).WithMessage("The task name must not exceed 100 chars.");
        }
    }
}
