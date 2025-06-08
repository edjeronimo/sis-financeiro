using System;

namespace SistemaFinanceiro
{
    /// <summary>
    /// Exceção lançada quando uma conta não é encontrada.
    /// </summary>
    public class ContaNaoEncontradaException : Exception
    {
        public Guid IdConta { get; }

        public ContaNaoEncontradaException(Guid idConta)
            : base($"Conta com ID '{idConta}' não encontrada.")
        {
            IdConta = idConta;
        }
    }

    /// <summary>
    /// Exceção lançada quando há saldo insuficiente para uma operação.
    /// </summary>
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

    /// <summary>
    /// Exceção lançada quando a moeda da transação difere da moeda padrão da conta.
    /// </summary>
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