using System;
using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using WalletPropertyTesting.Domain;
using WalletPropertyTesting.Tests.Arbitraries;

namespace WalletPropertyTesting.Tests.Properties
{
    public class MoneyProperties
    {
        // P1 · Туди й назад
        [Property(Arbitrary = new[] { typeof(MoneyGenerator) })]
        public void Addition_then_subtraction_returns_original_money(Money a, Money b)
        {
            var result = (a + b) - b;
            result.Should().Be(a);
        }

        // P8 · Без винятків
        [Property(Arbitrary = new[] { typeof(MoneyGenerator) })]
        public void Non_negative_money_addition_does_not_throw(Money a, Money b)
        {
            Action act = () => _ = a + b;
            act.Should().NotThrow();
        }

        // Перевірка винятку (Money конструктор)
        [Property]
        public void Negative_amount_throws_ArgumentException(PositiveInt raw)
        {
            decimal negativeAmount = -raw.Get / 100m;
            Action act = () => _ = new Money(negativeAmount);
            act.Should().Throw<ArgumentException>();
        }
    }
}
