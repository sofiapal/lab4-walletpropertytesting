using System;
using System.Collections.Generic;
using FluentAssertions;
using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;
using WalletPropertyTesting.Domain;
using WalletPropertyTesting.Tests.Arbitraries;

namespace WalletPropertyTesting.Tests.Properties
{
    public class RepositoryProperties
    {
        // P5 · Оракул (з передумовою When)
        [Property(Arbitrary = new[] { typeof(MoneyGenerator) })]
        public Property WalletService_operations_match_direct_wallet_operations(
            Money[] deposits,
            Money withdrawAmount)
        {
            var repo = new InMemoryWalletRepository();
            var service = new WalletService(repo);
            var serviceWallet = service.CreateWallet();

            var directWallet = new Wallet();

            foreach (var d in deposits)
            {
                service.Deposit(serviceWallet.Id, d.Amount);
                directWallet.Deposit(d);
            }

            bool applicable = directWallet.Balance.Amount >= withdrawAmount.Amount;
            bool ok = true;

            if (applicable)
            {
                service.Withdraw(serviceWallet.Id, withdrawAmount.Amount);
                directWallet.Withdraw(withdrawAmount);

                ok = service.GetBalance(serviceWallet.Id).Equals(directWallet.Balance)
                     && serviceWallet.History.Count == directWallet.History.Count;
            }

            return ok.When(applicable);
        }

        // P3 · Ідемпотентність
        [Property(Arbitrary = new[] { typeof(MoneyGenerator) })]
        public void Saving_same_wallet_twice_is_idempotent(Money[] deposits)
        {
            var repo = new InMemoryWalletRepository();
            var wallet = new Wallet();

            foreach (var d in deposits)
                wallet.Deposit(d);

            repo.Save(wallet);
            repo.Save(wallet);

            repo.All().Should().HaveCount(1);
            repo.Get(wallet.Id).Balance.Should().Be(wallet.Balance);
        }

        // Перевірка винятку (InMemoryWalletRepository.Get)
        [Property]
        public void Getting_unknown_wallet_id_throws_KeyNotFoundException(Guid unknownId)
        {
            var repo = new InMemoryWalletRepository();
            Action act = () => repo.Get(unknownId);
            act.Should().Throw<KeyNotFoundException>();
        }
    }
}
