using System;

namespace SistemaFinanceiro
{
    public class Transacao
    {
        public Guid Id { get; }
        public DateTime DataHora { get; }
        public TipoTransacao Tipo { get; }
        public decimal Valor { get; }
        public Moeda Moeda { get; }
        public string? Descricao { get; }
        public string? Categoria { get; }
        public Guid? IdContaContraparte { get; }

        public Transacao(TipoTransacao tipo, decimal valor, Moeda moeda, DateTime dataHora, string? descricao = null, string? categoria = null, Guid? idContaContraparte = null)
        {
            if (valor <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(valor), "O valor da transação deve ser positivo.");
            }

            Id = Guid.NewGuid();
            Tipo = tipo;
            Valor = valor;
            Moeda = moeda;
            DataHora = dataHora;
            Descricao = descricao;
            Categoria = categoria;
            IdContaContraparte = idContaContraparte;
        }

        public override string ToString()
        {
            string contraparteInfo = IdContaContraparte.HasValue ? $" (Contraparte: {IdContaContraparte.Value.ToString().Substring(0, 8)}...)" : "";
            return $"[{DataHora:dd/MM/yyyy HH:mm:ss}] {Tipo,-20}: {Valor,10:C2} {Moeda} - {Descricao ?? "N/A"}{contraparteInfo}";
        }
    }
}