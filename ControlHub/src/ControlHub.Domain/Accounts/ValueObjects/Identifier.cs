using ControlHub.Domain.Accounts.Enums;
using ControlHub.Domain.Common;
using ControlHub.SharedKernel.Results;

namespace ControlHub.Domain.Accounts.ValueObjects
{
    public sealed class Identifier : ValueObject
    {
        public IdentifierType Type { get; }
        public string Value { get; }
        public string NormalizedValue { get; }
        public string Regex { get; }
        public bool IsDeleted { get; private set; }

        private Identifier(IdentifierType type, string value, string normalizedValue, string regex)
        {
            Type = type;
            Value = value;
            NormalizedValue = normalizedValue;
            Regex = regex ?? "";
        }

        private Identifier(IdentifierType type, string value, string normalizedValue)
            : this(type, value, normalizedValue, "")
        {
        }

        public static Identifier Create(IdentifierType type, string value, string normalized)
            => new Identifier(type, value, normalized);

        public Result<Identifier> UpdateNormalizedValue(string value)
        {
            return Result<Identifier>.Success(this);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Type;
            yield return NormalizedValue; // equality dựa trên type + normalized value
        }

        public override string ToString() => $"{Type}:{NormalizedValue}";

        public void Delete()
        {
            IsDeleted = true;
        }
    }
}