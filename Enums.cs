namespace SistemaFinanceiro
{
    /// <summary>
    /// Define os tipos de transação possíveis no sistema.
    /// </summary>
    public enum TipoTransacao
    {
        Credito,
        Debito,
        TransferenciaEnviada,
        TransferenciaRecebida,
        PagamentoBoleto
    }

    /// <summary>
    /// Define as moedas suportadas pelo sistema.
    /// </summary>
    public enum Moeda
    {
        BRL, // Real Brasileiro
        USD, // Dólar Americano
        EUR  // Euro
    }
}