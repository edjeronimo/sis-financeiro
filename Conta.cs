using System;
using System.Collections.Generic;
using System.Linq;

namespace SistemaFinanceiro
{
    public class Conta
    {
        public Guid IdConta { get; }
        public string NumeroAgencia { get; private set; }
        public string NumeroConta { get; private set; }
        public decimal SaldoAtual { get; private set; }
        public Moeda MoedaPadrao { get; private set; }

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

        internal void AdicionarTransacao(Transacao transacao)
        {
            _historicoDeTransacoes.Add(transacao);
            _historicoDeTransacoes.Sort((t1, t2) => t1.DataHora.CompareTo(t2.DataHora));
        }

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