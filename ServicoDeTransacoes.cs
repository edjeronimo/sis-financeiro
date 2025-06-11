using System;
using System.Collections.Generic;
using System.Linq;

namespace SistemaFinanceiro
{
    public class ServicoDeTransacoes
    {
        private readonly Dictionary<Guid, Conta> _contas;

        private readonly object _lockContas = new object();

        public ServicoDeTransacoes()
        {
            _contas = new Dictionary<Guid, Conta>();
        }

        public IReadOnlyDictionary<Guid, Conta> Contas => _contas;

        public Conta AbrirConta(string numeroAgencia, string numeroConta, Moeda moedaPadrao, decimal saldoInicial = 0)
        {
            var novaConta = new Conta(numeroAgencia, numeroConta, moedaPadrao, saldoInicial);
            lock (_lockContas)
            {
                _contas.Add(novaConta.IdConta, novaConta);
            }
            return novaConta;
        }

        public decimal ConsultarSaldo(Guid idConta)
        {
            if (!_contas.TryGetValue(idConta, out var conta))
            {
                throw new ContaNaoEncontradaException(idConta);
            }
            return conta.SaldoAtual;
        }

        public IEnumerable<Transacao> ObterExtrato(Guid idConta, DateTime periodoInicio, DateTime periodoFim)
        {
            if (!_contas.TryGetValue(idConta, out var conta))
            {
                throw new ContaNaoEncontradaException(idConta);
            }

            return conta.HistoricoDeTransacoes
                        .Where(t => t.DataHora >= periodoInicio && t.DataHora <= periodoFim)
                        .OrderBy(t => t.DataHora);
        }

        private void ValidarTransacao(Conta conta, decimal valor, Moeda moeda)
        {
            if (valor <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(valor), "O valor da transação deve ser positivo.");
            }

            if (moeda != conta.MoedaPadrao)
            {
                throw new MoedaInvalidaException(moeda, conta.MoedaPadrao);
            }
        }

        public void RegistrarCredito(Guid idConta, decimal valor, Moeda moeda, DateTime dataHora, string descricao)
        {
            if (!_contas.TryGetValue(idConta, out var conta))
            {
                throw new ContaNaoEncontradaException(idConta);
            }

            ValidarTransacao(conta, valor, moeda);

            var transacao = new Transacao(TipoTransacao.Credito, valor, moeda, dataHora, descricao);
            conta.AdicionarTransacao(transacao);
        }
    }
}
