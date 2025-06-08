using System;
using System.Linq; // Para o método .Any() e outras operações LINQ
using System.Collections.Generic; // Para Dictionary, List

// Nota: Todas as outras classes e enums estão no mesmo namespace (SistemaFinanceiro),
// então não precisamos de 'using' explícitos para elas aqui além do System e Linq.

namespace SistemaFinanceiro
{
    // --- Métodos Auxiliares para Interação do Usuário ---
    public static class ConsoleHelper
    {
        public static string GetString(string prompt, bool canBeEmpty = false)
        {
            string? input;
            do
            {
                Console.Write(prompt);
                input = Console.ReadLine();
                if (!canBeEmpty && string.IsNullOrWhiteSpace(input))
                {
                    Console.WriteLine("Entrada não pode ser vazia. Tente novamente.");
                }
            } while (!canBeEmpty && string.IsNullOrWhiteSpace(input));
            return input?.Trim() ?? ""; // Retorna string vazia se for null e permitida vazia
        }

        public static decimal GetDecimal(string prompt)
        {
            decimal value;
            while (true)
            {
                Console.Write(prompt);
                if (decimal.TryParse(Console.ReadLine(), out value) && value >= 0) // Permitir 0 como saldo inicial
                {
                    return value;
                }
                Console.WriteLine("Valor inválido. Por favor, insira um número não negativo.");
            }
        }

        public static Moeda GetMoeda(string prompt)
        {
            Moeda moeda;
            while (true)
            {
                Console.Write(prompt + " (BRL, USD, EUR): ");
                string? input = Console.ReadLine()?.ToUpper();
                if (Enum.TryParse(input, out moeda) && Enum.IsDefined(typeof(Moeda), moeda))
                {
                    return moeda;
                }
                Console.WriteLine("Moeda inválida. Tente novamente (BRL, USD, EUR).");
            }
        }

        public static DateTime GetDateTime(string prompt)
        {
            DateTime date;
            while (true)
            {
                Console.Write(prompt + " (dd/MM/yyyy HH:mm:ss): ");
                if (DateTime.TryParseExact(Console.ReadLine(), "dd/MM/yyyy HH:mm:ss", null, System.Globalization.DateTimeStyles.None, out date))
                {
                    return date;
                }
                Console.WriteLine("Formato de data/hora inválido. Use dd/MM/yyyy HH:mm:ss.");
            }
        }

        public static Guid SelectConta(ServicoDeTransacoes servico)
        {
            if (!servico.Contas.Any())
            {
                Console.WriteLine("Nenhuma conta cadastrada.");
                return Guid.Empty; // Retorna GUID vazio para indicar que não há contas
            }

            Console.WriteLine("\nContas disponíveis:");
            foreach (var conta in servico.Contas.Values)
            {
                Console.WriteLine($"- {conta.IdConta.ToString().Substring(0, 8)}... | Ag: {conta.NumeroAgencia} | Conta: {conta.NumeroConta} | Saldo: {conta.SaldoAtual:C2} {conta.MoedaPadrao}");
            }

            Guid idConta;
            while (true)
            {
                Console.Write("Digite os primeiros 8 caracteres do ID da conta ou 'sair' para cancelar: ");
                string? input = Console.ReadLine()?.Trim();

                if (input?.ToLower() == "sair")
                {
                    return Guid.Empty;
                }

                // Tenta encontrar uma conta pelo prefixo do GUID
                var matchingAccount = servico.Contas.Values
                    .FirstOrDefault(c => c.IdConta.ToString().StartsWith(input!, StringComparison.OrdinalIgnoreCase));

                if (matchingAccount != null)
                {
                    idConta = matchingAccount.IdConta;
                    Console.WriteLine($"Conta selecionada: {matchingAccount.NumeroAgencia}-{matchingAccount.NumeroConta}");
                    return idConta;
                }
                Console.WriteLine("ID de conta inválido ou não encontrado. Tente novamente.");
            }
        }
    }


    // --- Programa Principal com Menu de Interação ---
    class Program
    {
        static ServicoDeTransacoes _servico = new ServicoDeTransacoes();

        static void Main(string[] args)
        {
            Console.WriteLine("=== Sistema de Gerenciamento Financeiro ===\n");

            while (true)
            {
                Console.WriteLine("\n--- MENU ---");
                Console.WriteLine("1. Abrir Nova Conta");
                Console.WriteLine("2. Registrar Crédito");
                Console.WriteLine("3. Registrar Débito");
                Console.WriteLine("4. Realizar Transferência");
                Console.WriteLine("5. Consultar Saldo");
                Console.WriteLine("6. Obter Extrato");
                Console.WriteLine("7. Calcular Saldo em Data Específica");
                Console.WriteLine("8. Listar Transações por Tipo");
                Console.WriteLine("9. Encontrar Transação Mais Valiosa");
                Console.WriteLine("0. Sair");
                Console.Write("Escolha uma opção: ");

                string? opcao = Console.ReadLine();

                try
                {
                    switch (opcao)
                    {
                        case "1": AbrirNovaConta(); break;
                        case "2": RegistrarCredito(); break;
                        case "3": RegistrarDebito(); break;
                        case "4": RealizarTransferencia(); break;
                        case "5": ConsultarSaldo(); break;
                        case "6": ObterExtrato(); break;
                        case "7": CalcularSaldoEmDataEspecifica(); break;
                        case "8": ListarTransacoesPorTipo(); break;
                        case "9": EncontrarTransacaoMaisValiosa(); break;
                        case "0":
                            Console.WriteLine("Saindo do sistema. Obrigado!");
                            return;
                        default:
                            Console.WriteLine("Opção inválida. Tente novamente.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\nERRO: {ex.Message}");
                    // Opcional: Logar a exceção completa em um arquivo para depuração
                }

                Console.WriteLine("\nPressione qualquer tecla para continuar...");
                Console.ReadKey();
            }
        }

        static void AbrirNovaConta()
        {
            Console.WriteLine("\n--- Abrir Nova Conta ---");
            string agencia = ConsoleHelper.GetString("Número da Agência: ");
            string contaNum = ConsoleHelper.GetString("Número da Conta: ");
            Moeda moeda = ConsoleHelper.GetMoeda("Moeda Padrão da Conta");
            decimal saldoInicial = ConsoleHelper.GetDecimal("Saldo Inicial: "); // Permite 0 agora

            try
            {
                Conta novaConta = _servico.AbrirConta(agencia, contaNum, moeda, saldoInicial);
                Console.WriteLine($"\nCONTA ABERTA COM SUCESSO!");
                Console.WriteLine($"ID da Conta: {novaConta.IdConta}");
                Console.WriteLine($"Agência: {novaConta.NumeroAgencia}, Conta: {novaConta.NumeroConta}");
                Console.WriteLine($"Saldo Atual: {novaConta.SaldoAtual:C2} {novaConta.MoedaPadrao}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Falha ao abrir conta: {ex.Message}");
            }
        }

        static void RegistrarCredito()
        {
            Console.WriteLine("\n--- Registrar Crédito ---");
            Guid idConta = ConsoleHelper.SelectConta(_servico);
            if (idConta == Guid.Empty) return;

            decimal valor = ConsoleHelper.GetDecimal("Valor do Crédito: ");
            Moeda moeda = ConsoleHelper.GetMoeda("Moeda da Transação");
            string descricao = ConsoleHelper.GetString("Descrição (opcional): ", true);
            string categoria = ConsoleHelper.GetString("Categoria (opcional): ", true);
            DateTime dataHora = ConsoleHelper.GetDateTime("Data e Hora da Transação");

            try
            {
                _servico.RegistrarCredito(idConta, valor, moeda, dataHora, descricao, categoria);
                Console.WriteLine($"Crédito de {valor:C2} {moeda} registrado na conta {idConta.ToString().Substring(0, 8)}... com sucesso.");
                Console.WriteLine($"Novo saldo: {_servico.ConsultarSaldo(idConta):C2}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Falha ao registrar crédito: {ex.Message}");
            }
        }

        static void RegistrarDebito()
        {
            Console.WriteLine("\n--- Registrar Débito ---");
            Guid idConta = ConsoleHelper.SelectConta(_servico);
            if (idConta == Guid.Empty) return;

            decimal valor = ConsoleHelper.GetDecimal("Valor do Débito: ");
            Moeda moeda = ConsoleHelper.GetMoeda("Moeda da Transação");
            string descricao = ConsoleHelper.GetString("Descrição (opcional): ", true);
            string categoria = ConsoleHelper.GetString("Categoria (opcional): ", true);
            DateTime dataHora = ConsoleHelper.GetDateTime("Data e Hora da Transação");

            try
            {
                _servico.RegistrarDebito(idConta, valor, moeda, dataHora, descricao, categoria);
                Console.WriteLine($"Débito de {valor:C2} {moeda} registrado na conta {idConta.ToString().Substring(0, 8)}... com sucesso.");
                Console.WriteLine($"Novo saldo: {_servico.ConsultarSaldo(idConta):C2}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Falha ao registrar débito: {ex.Message}");
            }
        }

        static void RealizarTransferencia()
        {
            Console.WriteLine("\n--- Realizar Transferência ---");
            Console.WriteLine("Selecione a CONTA DE ORIGEM:");
            Guid idContaOrigem = ConsoleHelper.SelectConta(_servico);
            if (idContaOrigem == Guid.Empty) return;

            Console.WriteLine("Selecione a CONTA DE DESTINO:");
            Guid idContaDestino = ConsoleHelper.SelectConta(_servico);
            if (idContaDestino == Guid.Empty) return;

            if (idContaOrigem == idContaDestino)
            {
                Console.WriteLine("Erro: Conta de origem e destino não podem ser a mesma.");
                return;
            }

            decimal valor = ConsoleHelper.GetDecimal("Valor da Transferência: ");
            Moeda moeda = ConsoleHelper.GetMoeda("Moeda da Transação");
            string descricao = ConsoleHelper.GetString("Descrição (opcional): ", true);
            DateTime dataHora = ConsoleHelper.GetDateTime("Data e Hora da Transação");

            try
            {
                _servico.RealizarTransferencia(idContaOrigem, idContaDestino, valor, moeda, dataHora, descricao);
                Console.WriteLine($"Transferência de {valor:C2} {moeda} da conta {idContaOrigem.ToString().Substring(0, 8)}... para {idContaDestino.ToString().Substring(0, 8)}... realizada com sucesso.");
                Console.WriteLine($"Saldo atual Origem: {_servico.ConsultarSaldo(idContaOrigem):C2}");
                Console.WriteLine($"Saldo atual Destino: {_servico.ConsultarSaldo(idContaDestino):C2}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Falha ao realizar transferência: {ex.Message}");
            }
        }

        static void ConsultarSaldo()
        {
            Console.WriteLine("\n--- Consultar Saldo ---");
            Guid idConta = ConsoleHelper.SelectConta(_servico);
            if (idConta == Guid.Empty) return;

            try
            {
                decimal saldo = _servico.ConsultarSaldo(idConta);
                Console.WriteLine($"\nSaldo atual da conta {idConta.ToString().Substring(0, 8)}...: {saldo:C2}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Falha ao consultar saldo: {ex.Message}");
            }
        }

        static void ObterExtrato()
        {
            Console.WriteLine("\n--- Obter Extrato ---");
            Guid idConta = ConsoleHelper.SelectConta(_servico);
            if (idConta == Guid.Empty) return;

            DateTime inicio = ConsoleHelper.GetDateTime("Data de Início (dd/MM/yyyy HH:mm:ss)");
            DateTime fim = ConsoleHelper.GetDateTime("Data de Fim (dd/MM/yyyy HH:mm:ss)");

            try
            {
                var extrato = _servico.ObterExtrato(idConta, inicio, fim).ToList();
                Console.WriteLine($"\nExtrato da conta {idConta.ToString().Substring(0, 8)}... de {inicio:dd/MM/yyyy} a {fim:dd/MM/yyyy}:");
                if (extrato.Any())
                {
                    foreach (var t in extrato)
                    {
                        Console.WriteLine($"- {t}");
                    }
                }
                else
                {
                    Console.WriteLine("Nenhuma transação encontrada no período.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Falha ao obter extrato: {ex.Message}");
            }
        }

        static void CalcularSaldoEmDataEspecifica()
        {
            Console.WriteLine("\n--- Calcular Saldo em Data Específica ---");
            Guid idConta = ConsoleHelper.SelectConta(_servico);
            if (idConta == Guid.Empty) return;

            DateTime dataAlvo = ConsoleHelper.GetDateTime("Data Alvo (dd/MM/yyyy HH:mm:ss)");

            try
            {
                decimal saldo = _servico.CalcularSaldoEmDataEspecifica(idConta, dataAlvo);
                Console.WriteLine($"\nSaldo da conta {idConta.ToString().Substring(0, 8)}... em {dataAlvo:dd/MM/yyyy HH:mm:ss}: {saldo:C2}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Falha ao calcular saldo em data específica: {ex.Message}");
            }
        }

        static void ListarTransacoesPorTipo()
        {
            Console.WriteLine("\n--- Listar Transações por Tipo ---");
            Guid idConta = ConsoleHelper.SelectConta(_servico);
            if (idConta == Guid.Empty) return;

            TipoTransacao tipo;
            while (true)
            {
                Console.Write("Tipo de Transação (Credito, Debito, TransferenciaEnviada, TransferenciaRecebida, PagamentoBoleto): ");
                string? input = Console.ReadLine()?.Trim();
                if (Enum.TryParse(input, true, out tipo) && Enum.IsDefined(typeof(TipoTransacao), tipo))
                {
                    break;
                }
                Console.WriteLine("Tipo de transação inválido. Tente novamente.");
            }

            DateTime inicio = ConsoleHelper.GetDateTime("Data de Início (dd/MM/yyyy HH:mm:ss)");
            DateTime fim = ConsoleHelper.GetDateTime("Data de Fim (dd/MM/yyyy HH:mm:ss)");

            try
            {
                var transacoes = _servico.ListarTransacoesPorTipo(idConta, tipo, inicio, fim).ToList();
                Console.WriteLine($"\nTransações do tipo '{tipo}' da conta {idConta.ToString().Substring(0, 8)}... de {inicio:dd/MM/yyyy} a {fim:dd/MM/yyyy}:");
                if (transacoes.Any())
                {
                    foreach (var t in transacoes)
                    {
                        Console.WriteLine($"- {t}");
                    }
                }
                else
                {
                    Console.WriteLine("Nenhuma transação encontrada para o tipo e período especificados.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Falha ao listar transações por tipo: {ex.Message}");
            }
        }

        static void EncontrarTransacaoMaisValiosa()
        {
            Console.WriteLine("\n--- Encontrar Transação Mais Valiosa ---");
            Guid idConta = ConsoleHelper.SelectConta(_servico);
            if (idConta == Guid.Empty) return;

            TipoTransacao tipo;
            while (true)
            {
                Console.Write("Tipo de Transação (Credito, Debito, TransferenciaEnviada, TransferenciaRecebida, PagamentoBoleto): ");
                string? input = Console.ReadLine()?.Trim();
                if (Enum.TryParse(input, true, out tipo) && Enum.IsDefined(typeof(TipoTransacao), tipo))
                {
                    break;
                }
                Console.WriteLine("Tipo de transação inválida. Tente novamente.");
            }

            DateTime inicio = ConsoleHelper.GetDateTime("Data de Início (dd/MM/yyyy HH:mm:ss)");
            DateTime fim = ConsoleHelper.GetDateTime("Data de Fim (dd/MM/yyyy HH:mm:ss)");

            try
            {
                var transacao = _servico.EncontrarTransacaoMaisValiosa(idConta, tipo, inicio, fim);
                if (transacao != null)
                {
                    Console.WriteLine($"\nTransação mais valiosa do tipo '{tipo}' na conta {idConta.ToString().Substring(0, 8)}... no período:");
                    Console.WriteLine($"- {transacao}");
                }
                else
                {
                    Console.WriteLine("Nenhuma transação encontrada para o tipo e período especificados.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Falha ao encontrar transação mais valiosa: {ex.Message}");
            }
        }
    }
}