using FsCheck;
using FsCheck.Fluent;
using WalletPropertyTesting.Domain;

namespace WalletPropertyTesting.Tests.Arbitraries
{
    public static class MoneyGenerator
    {
        // Money: Amount >= 0 (конструктор кидає ArgumentException для від'ємних значень).
        // Включаємо граничні значення (0 та 0.01) і суми з двома знаками після коми.
        public static Arbitrary<Money> Money()
        {
            var amountGen = Gen.Frequency(
                (1, Gen.Constant(0m)),
                (1, Gen.Constant(0.01m)),
                (8, Gen.Choose(0, 100_000).Select(c => c / 100m))
            );

            return Arb.From(amountGen.Select(amount => new Money(amount)));
        }
    }
}
