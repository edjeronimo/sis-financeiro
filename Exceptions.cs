using System;

namespace SistemaFinanceiro
{
    public class ContaNaoEncontradaException : Exception
    {
        public Guid IdConta { get; }

        public ContaNaoEncontradaException(Guid idConta)
            : base($"Conta com ID '{idConta}' não encontrada.")
        {
            IdConta = idConta;
        }
    }

    public class SaldoInsuficienteException : Exception
    {
        public Guid IdConta { get; }
        public decimal SaldoAtual { get; }
        public decimal ValorTransacao { get; }

        public SaldoInsuficienteException(Guid idConta, decimal saldoAtual, decimal valorTransacao)
            : base($"Saldo insuficiente na conta {idConta}. Saldo atual: {saldoAtual}, Valor da transação: {valorTransacao}.")
        {
            IdConta = idConta;
            SaldoAtual = saldoAtual;
            ValorTransacao = valorTransacao;
        }
    }

    public class MoedaInvalidaException : Exception
    {
        public Moeda MoedaTransacao { get; }
        public Moeda MoedaConta { get; }

        public MoedaInvalidaException(Moeda moedaTransacao, Moeda moedaConta)
            : base($"Moeda da transação ({moedaTransacao}) difere da moeda padrão da conta ({moedaConta}).")
        {
            MoedaTransacao = moedaTransacao;
            MoedaConta = moedaConta;
        }
    }
}