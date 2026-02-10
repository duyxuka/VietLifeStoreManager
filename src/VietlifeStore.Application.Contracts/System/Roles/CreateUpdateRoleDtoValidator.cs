using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VietlifeStore.System.Roles
{
    public class CreateUpdateRoleDtoValidator : AbstractValidator<CreateUpdateRoleDto>
    {
        public CreateUpdateRoleDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.Description).NotEmpty();
        }
    }
}
