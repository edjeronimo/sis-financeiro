using System;
using System.Collections.Generic;
using System.Linq; // Necessário para o método Sort da lista de transações

namespace SistemaFinanceiro
{
    /// <summary>
    /// Representa uma conta bancária com seu histórico de transações.
    /// </summary>
    public class Conta
    {
        public Guid IdConta { get; }
        public string NumeroAgencia { get; private set; }
        public string NumeroConta { get; private set; }
        public decimal SaldoAtual { get; private set; }
        public Moeda MoedaPadrao { get; private set; }

        // Usa List para gerenciamento interno e IReadOnlyList para exposição externa
        private readonly List<Transacao> _historicoDeTransacoes;
        public IReadOnlyList<Transacao> HistoricoDeTransacoes => _historicoDeTransacoes.AsReadOnly();

        public Conta(string numeroAgencia, string numeroConta, Moeda moedaPadrao, decimal saldoInicial = 0)
        {
            if (saldoInicial < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(saldoInicial), "Saldo inicial não pode ser negativo.");
            }

            IdConta = Guid.NewGuid();
            NumeroAgencia = numeroAgencia;
            NumeroConta = numeroConta;
            MoedaPadrao = moedaPadrao;
            SaldoAtual = saldoInicial;
            _historicoDeTransacoes = new List<Transacao>();
        }

        /// <summary>
        /// Adiciona uma transação ao histórico e mantém a lista ordenada por data.
        /// (Uso interno pelo ServicoDeTransacoes)
        /// </summary>
        internal void AdicionarTransacao(Transacao transacao)
        {
            _historicoDeTransacoes.Add(transacao);
            // Sempre mantém o histórico ordenado cronologicamente
            _historicoDeTransacoes.Sort((t1, t2) => t1.DataHora.CompareTo(t2.DataHora));
        }

        /// <summary>
        /// Atualiza o saldo atual da conta.
        /// (Uso interno pelo ServicoDeTransacoes)
        /// </summary>
        internal void AtualizarSaldo(decimal valor)
        {
            SaldoAtual += valor;
        }

        public override string ToString()
        {
            return $"Conta ID: {IdConta.ToString().Substring(0, 8)}..., Agência: {NumeroAgencia}, Conta: {NumeroConta}, Saldo: {SaldoAtual:C2} {MoedaPadrao}";
        }
    }
}