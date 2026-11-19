using System;
using FluentAssertions;
using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;
using WalletPropertyTesting.Domain;
using WalletPropertyTesting.Tests.Arbitraries;

namespace WalletPropertyTesting.Tests.Properties
{
    public class WalletProperties
    {
        // P2 · Інваріант
        [Property(Arbitrary = new[] { typeof(TransactionGenerator) })]
        public void Balance_is_never_negative_and_history_matches_operations(Transaction[] transactions)
        {
            var wallet = new Wallet();
            int appliedCount = 0;

            foreach (var tx in transactions)
            {
                if (tx.Type == TransactionType.Deposit)
                {
                    wallet.Deposit(tx.Amount);
                    appliedCount++;
                }
                else if (wallet.Balance.Amount >= tx.Amount.Amount)
                {
                    wallet.Withdraw(tx.Amount);
                    appliedCount++;
                }

                wallet.Balance.Amount.Should().BeGreaterThanOrEqualTo(0m);
            }

            wallet.History.Should().HaveCount(appliedCount);
        }

        // P1 · Туди й назад
        [Property(Arbitrary = new[] { typeof(MoneyGenerator) })]
        public void Deposit_then_withdraw_same_amount_restores_balance(Money[] initialDeposits, Money amount)
        {
            var wallet = new Wallet();
            foreach (var d in initialDeposits)
                wallet.Deposit(d);

            var balanceBefore = wallet.Balance;

            wallet.Deposit(amount);
            wallet.Withdraw(amount);

            wallet.Balance.Should().Be(balanceBefore);
        }

        // P4 · Різні шляхи
        [Property(Arbitrary = new[] { typeof(MoneyGenerator) })]
        public void Two_deposits_equal_single_combined_deposit(Money[] initialDeposits, Money a, Money b)
        {
            var wallet1 = new Wallet();
            var wallet2 = new Wallet();

            foreach (var d in initialDeposits)
            {
                wallet1.Deposit(d);
                wallet2.Deposit(d);
            }

            wallet1.Deposit(a);
            wallet1.Deposit(b);

            wallet2.Deposit(a + b);

            wallet1.Balance.Should().Be(wallet2.Balance);
        }

        // P6 · Метаморфічна
        [Property(Arbitrary = new[] { typeof(MoneyGenerator) })]
        public void Doubling_all_deposits_doubles_final_balance(Money[] deposits)
        {
            var walletSingle = new Wallet();
            var walletDouble = new Wallet();

            foreach (var d in deposits)
            {
                walletSingle.Deposit(d);
                walletDouble.Deposit(d + d);
            }

            walletDouble.Balance.Amount.Should().Be(walletSingle.Balance.Amount * 2m);
        }

        // P7 · Легко перевірити (з передумовою When)
        [Property(Arbitrary = new[] { typeof(MoneyGenerator) })]
        public Property Withdraw_decreases_balance_and_adds_history_entry_when_sufficient_funds(
            Money[] deposits,
            Money withdrawAmount)
        {
            var wallet = new Wallet();
            foreach (var d in deposits)
                wallet.Deposit(d);

            bool applicable = wallet.Balance.Amount >= withdrawAmount.Amount;
            bool ok = true;

            if (applicable)
            {
                var balanceBefore = wallet.Balance;
                int historyBefore = wallet.History.Count;

                wallet.Withdraw(withdrawAmount);

                ok = (balanceBefore - wallet.Balance).Equals(withdrawAmount)
                     && wallet.History.Count == historyBefore + 1;
            }

            return ok.When(applicable);
        }

        // Перевірка винятку з передумовою When (Wallet.Withdraw)
        [Property(Arbitrary = new[] { typeof(MoneyGenerator) })]
        public Property Withdraw_more_than_balance_throws_InvalidOperationException(
            Money initialDeposit,
            Money withdrawAmount)
        {
            var wallet = new Wallet();
            wallet.Deposit(initialDeposit);

            bool applicable = withdrawAmount.Amount > wallet.Balance.Amount;
            bool ok = true;

            if (applicable)
            {
                Action act = () => wallet.Withdraw(withdrawAmount);
                act.Should().Throw<InvalidOperationException>();
            }

            return ok.When(applicable);
        }
    }
}
