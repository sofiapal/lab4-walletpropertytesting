using FsCheck;
using FsCheck.Fluent;
using WalletPropertyTesting.Domain;

namespace WalletPropertyTesting.Tests.Arbitraries
{
    public static class TransactionGenerator
    {
        // Транзакція = сума (з MoneyGenerator) + тип (Deposit або Withdraw).
        // Конструктор Transaction закритий, тому використовуємо фабричні методи.
        public static Arbitrary<Transaction> Transaction()
        {
            var gen =
                from money in MoneyGenerator.Money().Generator
                from type in Gen.Elements(TransactionType.Deposit, TransactionType.Withdraw)
                select type == TransactionType.Deposit
                    ? Domain.Transaction.Deposit(money)
                    : Domain.Transaction.Withdraw(money);

            return Arb.From(gen);
        }
    }
}
