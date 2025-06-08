using System;
using System.Collections.Generic;
using System.Linq; // Necessário para LINQ em consultas

namespace SistemaFinanceiro
{
    /// <summary>
    /// Serviço central para gerenciar contas e processar transações financeiras.
    /// </summary>
    public class ServicoDeTransacoes
    {
        // Armazenamento em memória para as contas. Em um sistema real, seria um banco de dados.
        private readonly Dictionary<Guid, Conta> _contas;

        // Objeto para simular bloqueio pessimista em cenários concorrentes.
        // Em uma aplicação real, a granularidade do bloqueio seria mais fina.
        private readonly object _lockContas = new object();

        public ServicoDeTransacoes()
        {
            _contas = new Dictionary<Guid, Conta>();
        }

        // Expor as contas para facilitar a seleção pelo usuário
        public IReadOnlyDictionary<Guid, Conta> Contas => _contas;

        // --- Operações de Conta ---

        /// <summary>
        /// Cria e adiciona uma nova conta ao sistema.
        /// </summary>
        /// <returns>A nova conta criada.</returns>
        public Conta AbrirConta(string numeroAgencia, string numeroConta, Moeda moedaPadrao, decimal saldoInicial = 0)
        {
            // Em um sistema de produção, você adicionaria validações para unicidade de agência/conta, etc.
            var novaConta = new Conta(numeroAgencia, numeroConta, moedaPadrao, saldoInicial);
            lock (_lockContas) // Protege o acesso ao dicionário de contas
            {
                _contas.Add(novaConta.IdConta, novaConta);
            }
            return novaConta;
        }

        /// <summary>
        /// Retorna o saldo atual de uma conta específica.
        /// </summary>
        /// <param name="idConta">ID da conta.</param>
        /// <returns>O saldo atual da conta.</returns>
        /// <exception cref="ContaNaoEncontradaException">Se a conta não for encontrada.</exception>
        public decimal ConsultarSaldo(Guid idConta)
        {
            if (!_contas.TryGetValue(idConta, out var conta))
            {
                throw new ContaNaoEncontradaException(idConta);
            }
            return conta.SaldoAtual;
        }

        /// <summary>
        /// Retorna as transações de uma conta em um período específico, ordenadas cronologicamente.
        /// </summary>
        /// <param name="idConta">ID da conta.</param>
        /// <param name="periodoInicio">Data de início do período (inclusive).</param>
        /// <param name="periodoFim">Data de fim do período (inclusive).</param>
        /// <returns>Uma coleção de transações.</returns>
        /// <exception cref="ContaNaoEncontradaException">Se a conta não for encontrada.</exception>
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

        // --- Processamento de Transações e Validações ---

        /// <summary>
        /// Valida as condições básicas de uma transação antes de processá-la.
        /// </summary>
        /// <param name="conta">A conta envolvida na transação.</param>
        /// <param name="valor">O valor da transação.</param>
        /// <param name="moeda">A moeda da transação.</param>
        /// <exception cref="ArgumentOutOfRangeException">Se o valor for zero ou negativo.</exception>
        /// <exception cref="MoedaInvalidaException">Se a moeda da transação for diferente da moeda da conta.</exception>
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

        /// <summary>
        /// Registra uma operação de crédito em uma conta.
        /// </summary>
        /// <exception cref="ContaNaoEncontradaException">Se a conta não for encontrada.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Se o valor for inválido.</exception>
        /// <exception cref="MoedaInvalidaException">Se a moeda for inconsistente.</exception>
        public void RegistrarCredito(Guid idConta, decimal valor, Moeda moeda, DateTime dataHora, string? descricao = null, string? categoria = null)
        {
            lock (_lockContas) // Garante que apenas uma operação por vez altere a conta
            {
                if (!_contas.TryGetValue(idConta, out var conta))
                {
                    throw new ContaNaoEncontradaException(idConta);
                }

                ValidarTransacao(conta, valor, moeda);

                var transacao = new Transacao(TipoTransacao.Credito, valor, moeda, dataHora, descricao, categoria);
                conta.AdicionarTransacao(transacao);
                conta.AtualizarSaldo(valor);
            }
        }

        /// <summary>
        /// Registra uma operação de débito em uma conta.
        /// </summary>
        /// <exception cref="ContaNaoEncontradaException">Se a conta não for encontrada.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Se o valor for inválido.</exception>
        /// <exception cref="MoedaInvalidaException">Se a moeda for inconsistente.</exception>
        /// <exception cref="SaldoInsuficienteException">Se o saldo for insuficiente.</exception>
        public void RegistrarDebito(Guid idConta, decimal valor, Moeda moeda, DateTime dataHora, string? descricao = null, string? categoria = null)
        {
            lock (_lockContas)
            {
                if (!_contas.TryGetValue(idConta, out var conta))
                {
                    throw new ContaNaoEncontradaException(idConta);
                }

                ValidarTransacao(conta, valor, moeda);

                if (conta.SaldoAtual < valor)
                {
                    throw new SaldoInsuficienteException(idConta, conta.SaldoAtual, valor);
                }

                var transacao = new Transacao(TipoTransacao.Debito, valor, moeda, dataHora, descricao, categoria);
                conta.AdicionarTransacao(transacao);
                conta.AtualizarSaldo(-valor); // Atualiza o saldo
            }
        }

        /// <summary>
        /// Realiza uma transferência atômica entre duas contas.
        /// </summary>
        /// <remarks>
        /// A atomicidade é simulada em memória. Em um cenário real, usaria transações de banco de dados.
        /// </remarks>
        /// <exception cref="ContaNaoEncontradaException">Se uma das contas não for encontrada.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Se o valor for inválido.</exception>
        /// <exception cref="MoedaInvalidaException">Se as moedas forem inconsistentes entre as contas.</exception>
        /// <exception cref="SaldoInsuficienteException">Se o saldo da conta de origem for insuficiente.</exception>
        public void RealizarTransferencia(Guid idContaOrigem, Guid idContaDestino, decimal valor, Moeda moeda, DateTime dataHora, string? descricao = null)
        {
            // O bloqueio aqui é mais complexo, pois envolve duas contas.
            // Para simplicidade e evitar deadlocks triviais, bloqueamos o objeto global.
            // Em um cenário real, você bloquearia as contas individuais de forma ordenada.
            lock (_lockContas)
            {
                if (!_contas.TryGetValue(idContaOrigem, out var contaOrigem))
                {
                    throw new ContaNaoEncontradaException(idContaOrigem);
                }
                if (!_contas.TryGetValue(idContaDestino, out var contaDestino))
                {
                    throw new ContaNaoEncontradaException(idContaDestino);
                }

                ValidarTransacao(contaOrigem, valor, moeda);
                ValidarTransacao(contaDestino, valor, moeda); // Garante que as moedas são compatíveis para transferência

                if (contaOrigem.SaldoAtual < valor)
                {
                    throw new SaldoInsuficienteException(idContaOrigem, contaOrigem.SaldoAtual, valor);
                }

                // Simulação de atomicidade: Se uma parte falhar, a outra não será registrada.
                // Em um sistema real com persistência, isso envolveria transações de banco de dados (ACID).
                try
                {
                    // Débito na conta de origem
                    var transacaoSaida = new Transacao(TipoTransacao.TransferenciaEnviada, valor, moeda, dataHora, descricao, idContaContraparte: idContaDestino);
                    contaOrigem.AdicionarTransacao(transacaoSaida);
                    contaOrigem.AtualizarSaldo(-valor);

                    // Crédito na conta de destino
                    var transacaoEntrada = new Transacao(TipoTransacao.TransferenciaRecebida, valor, moeda, dataHora, descricao, idContaContraparte: idContaOrigem);
                    contaDestino.AdicionarTransacao(transacaoEntrada);
                    contaDestino.AtualizarSaldo(valor);
                }
                catch (Exception)
                {
                    // Em um sistema real, você faria um rollback explícito da transação de banco de dados.
                    // Aqui, a exceção já sinaliza a falha e evita que uma parte seja persistida isoladamente.
                    throw;
                }
            }
        }

        // --- Consultas Avançadas ---

        /// <summary>
        /// Lista transações de uma conta por tipo em um período específico.
        /// </summary>
        /// <returns>Uma coleção de transações filtradas.</returns>
        /// <exception cref="ContaNaoEncontradaException">Se a conta não for encontrada.</exception>
        public IEnumerable<Transacao> ListarTransacoesPorTipo(Guid idConta, TipoTransacao tipoTransacao, DateTime periodoInicio, DateTime periodoFim)
        {
            if (!_contas.TryGetValue(idConta, out var conta))
            {
                throw new ContaNaoEncontradaException(idConta);
            }

            return conta.HistoricoDeTransacoes
                        .Where(t => t.Tipo == tipoTransacao && t.DataHora >= periodoInicio && t.DataHora <= periodoFim)
                        .OrderBy(t => t.DataHora);
        }

        /// <summary>
        /// Calcula o saldo de uma conta em uma data específica, processando o histórico.
        /// </summary>
        /// <param name="dataAlvo">A data para a qual o saldo deve ser calculado.</param>
        /// <returns>O saldo da conta na data alvo.</returns>
        /// <exception cref="ContaNaoEncontradaException">Se a conta não for encontrada.</exception>
        public decimal CalcularSaldoEmDataEspecifica(Guid idConta, DateTime dataAlvo)
        {
            if (!_contas.TryGetValue(idConta, out var conta))
            {
                throw new ContaNaoEncontradaException(idConta);
            }

            // Para um cálculo preciso do saldo em uma data passada,
            // precisaremos de um ponto de partida. Aqui assumimos que o saldo inicial
            // da conta é 0 e todas as transações desde a abertura estão no histórico.
            // Em um sistema real, o saldo inicial da conta na data de abertura seria o ponto de partida.
            decimal saldoCalculado = 0;

            // Processa apenas as transações até a data alvo
            foreach (var transacao in conta.HistoricoDeTransacoes.Where(t => t.DataHora <= dataAlvo).OrderBy(t => t.DataHora))
            {
                switch (transacao.Tipo)
                {
                    case TipoTransacao.Credito:
                    case TipoTransacao.TransferenciaRecebida:
                        saldoCalculado += transacao.Valor;
                        break;
                    case TipoTransacao.Debito:
                    case TipoTransacao.TransferenciaEnviada:
                    case TipoTransacao.PagamentoBoleto:
                        saldoCalculado -= transacao.Valor;
                        break;
                }
            }
            return saldoCalculado;
        }

        /// <summary>
        /// Encontra a transação de maior valor (crédito ou débito) de um tipo específico em um período.
        /// </summary>
        /// <returns>A transação de maior valor, ou null se nenhuma for encontrada.</returns>
        /// <exception cref="ContaNaoEncontradaException">Se a conta não for encontrada.</exception>
        public Transacao? EncontrarTransacaoMaisValiosa(Guid idConta, TipoTransacao tipoTransacao, DateTime periodoInicio, DateTime periodoFim)
        {
            if (!_contas.TryGetValue(idConta, out var conta))
            {
                throw new ContaNaoEncontradaException(idConta);
            }

            return conta.HistoricoDeTransacoes
                        .Where(t => t.Tipo == tipoTransacao && t.DataHora >= periodoInicio && t.DataHora <= periodoFim)
                        .OrderByDescending(t => t.Valor)
                        .FirstOrDefault();
        }
    }
}