using AgenticBoard.Application.Features.Comments.Dtos;
using FluentValidation;

namespace AgenticBoard.Application.Features.Comments.Validators;

public class CreateCommentDtoValidator : AbstractValidator<CreateCommentDto>
{
    public CreateCommentDtoValidator()
    {
        RuleFor(x => x.Text)
            .NotEmpty().WithMessage("Comment text cannot be empty.")
            .MaximumLength(2000).WithMessage("Comment text cannot exceed 2000 characters.");
    }
}
