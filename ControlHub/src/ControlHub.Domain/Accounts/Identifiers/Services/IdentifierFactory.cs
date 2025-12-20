using ControlHub.Domain.Accounts.Enums;
using ControlHub.Domain.Accounts.Identifiers.Rules;
using ControlHub.Domain.Accounts.ValueObjects;
using ControlHub.SharedKernel.Accounts;
using ControlHub.SharedKernel.Results;

namespace ControlHub.Domain.Accounts.Identifiers.Services
{
    public class IdentifierFactory
    {
        private readonly IEnumerable<IIdentifierValidator> _validators;

        public IdentifierFactory(IEnumerable<IIdentifierValidator> validators)
        {
            _validators = validators;
        }

        public Result<Identifier> Create(IdentifierType type, string rawValue)
        {
            // 1. Tìm Chiến lược (Strategy) phù hợp
            var validator = _validators.FirstOrDefault(v => v.Type == type);

            if (validator == null)
            {
                // Logic nghiệp vụ: Không hỗ trợ loại này
                return Result<Identifier>.Failure(AccountErrors.UnsupportedIdentifierType);
            }

            // 2. Thực thi Validate & Normalize
            var (isValid, normalized, error) = validator.ValidateAndNormalize(rawValue);

            if (!isValid)
            {
                return Result<Identifier>.Failure(error!);
            }

            // 3. Tạo Value Object (Entity)
            // Lưu ý: Identifier.Create giờ chỉ nên được gọi từ đây để đảm bảo an toàn
            return Result<Identifier>.Success(Identifier.Create(type, rawValue, normalized));
        }
    }
}